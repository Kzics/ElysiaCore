using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Life;
using Life.Network;

namespace ElysiaInteractMenu
{
    public class VehicleInfoManager
    {
        private ElysiaDB _elysiaDB;
        public List<VehicleInfo> VehicleInfos = new List<VehicleInfo>();
        public List<Player> PlayersDriving = new List<Player>();
        
        public VehicleInfoManager(ElysiaDB elysiaDB)
        {
            _elysiaDB = elysiaDB;
            LoadVehicleInfo();
        }
        
        public VehicleInfo AddVehicleInfo(int characterId, int vehicleId)
        {
            Nova.server.SendMessageToAll("ici");
            VehicleInfo vehicleInfo = new VehicleInfo
            {
                CharacterId = characterId, VehicleId = vehicleId,
                DateDriving = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Kilometer = 0,
            };
            Nova.server.SendMessageToAll("la");

            
            VehicleInfos.Add(vehicleInfo);
            Nova.server.SendMessageToAll("laa");

            Task.Run(() =>
            { 
                return _elysiaDB.db.InsertOrReplaceAsync(new VehicleInfos(){VehicleId = vehicleInfo.VehicleId,CharacterId = vehicleInfo.CharacterId
                    , DateDriving = vehicleInfo.DateDriving, Kilometer = vehicleInfo.Kilometer});
            });

            return vehicleInfo;
        }
        
        
        public void RemoveVehicleInfo(VehicleInfo vehicleInfo)
        {
            this.VehicleInfos.Remove(vehicleInfo);
            
            _elysiaDB.DeleteVehicleInfo(vehicleInfo);
        }

        public VehicleInfo GetVehicleInfo(int vehicleId)
        {
            VehicleInfo foundInfo = this.VehicleInfos.FirstOrDefault(info => info.VehicleId == vehicleId);
            return foundInfo;
        }

        public async Task Save()
        {
            foreach (VehicleInfo vehicleInfo in VehicleInfos)
            {
                VehicleInfos existingInfo = await _elysiaDB.db.Table<VehicleInfos>()
                    .Where(info => info.VehicleId == vehicleInfo.VehicleId)
                    .FirstOrDefaultAsync();
        
                if (existingInfo != null)
                {
                    try
                    {
                        existingInfo.Kilometer = vehicleInfo.Kilometer;
                        existingInfo.DateDriving = 4393;
                        
                        Nova.server.SendMessageToAll("La");

                        int num = await _elysiaDB.db.UpdateAsync(existingInfo);
                        Nova.server.SendMessageToAll(num + "");
                    }
                    catch (Exception e)
                    {
                        Nova.server.SendMessageToAll(e.HResult + "");
                        throw;
                    }
                }
                else
                {
                    // Insertion d'un nouveau véhicule
                    await _elysiaDB.db.InsertAsync(new VehicleInfos()
                    {
                        VehicleId = vehicleInfo.VehicleId,
                        CharacterId = vehicleInfo.CharacterId,
                        DateDriving = vehicleInfo.DateDriving,
                        Kilometer = vehicleInfo.Kilometer
                    });
                }
            }
        }

        private async Task LoadVehicleInfo()
        {
            List<VehicleInfos> vehiclesInfoAsync = await _elysiaDB.LoadVehicleInfos();

            foreach (VehicleInfos vehicleInfo in vehiclesInfoAsync)
            {
                VehicleInfos.Add(new VehicleInfo()
                {
                    CharacterId = vehicleInfo.CharacterId,DateDriving = vehicleInfo.DateDriving,
                    Kilometer = vehicleInfo.Kilometer,VehicleId = vehicleInfo.VehicleId
                });

            }
        }
        
        
        
    }
}
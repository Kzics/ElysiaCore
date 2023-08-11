using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElysiaInteractMenu.Manager
{
    public class PlayerFineManager
    {
        public List<PlayerFine> PlayerFines { get; set; }

        private ElysiaDB ElysiaDB;
        
        public PlayerFineManager(ElysiaDB elysiaDB)
        {
            ElysiaDB = elysiaDB;
            PlayerFines = new List<PlayerFine>();
            LoadFines();
        }


        public List<PlayerFine> GetPlayerFines(int characterId)
        {
            return PlayerFines.Where(fine => fine.ReceiverId == characterId).ToList();
        }
        protected async Task LoadFines()
        {
            List<PlayerFines> playerFinesAsync = await ElysiaDB.LoadPlayerFines();
            foreach (var playerFineAsync in playerFinesAsync)
            {
                PlayerFine playerFine = new PlayerFine
                {
                    FineId = playerFineAsync.FineId,
                    Amount = playerFineAsync.Amount,
                    IsPaid = playerFineAsync.IsPaid,
                    Reason = playerFineAsync.Reason,
                    ReceiverId = playerFineAsync.ReceiverId,
                    SenderId = playerFineAsync.SenderId,
                    SentTimestamp = playerFineAsync.SentTimestamp
                };

                PlayerFines.Add(playerFine);
            }
        }


        public async Task Save()
        {
            await ElysiaDB.db.DeleteAllAsync<PlayerFines>();
            
            List<PlayerFine> fines = ElysiaMain.instance.PlayerFineManager.PlayerFines;

            foreach (var fine in fines)
            {
                await ElysiaDB.db.InsertAsync(new PlayerFines()
                {
                    SenderId = fine.SenderId,
                    ReceiverId = fine.ReceiverId,
                    Reason = fine.Reason,
                    IsPaid = fine.IsPaid,
                    SentTimestamp = fine.SentTimestamp
                });
            }
        }
    }
}
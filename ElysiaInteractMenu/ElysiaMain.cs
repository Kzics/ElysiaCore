using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElysiaInteractMenu.Commands;
using ElysiaInteractMenu.Manager;
using Life;
using Life.CharacterSystem;
using Life.Network;
using Life.VehicleSystem;
using Socket.Newtonsoft.Json.Linq;
using UnityEngine;

namespace ElysiaInteractMenu
{
    public class ElysiaMain : Plugin
    {
        private ElysiaDB ElysiaDB { get; }
        public static ElysiaMain instance { get; set; }
        public InvoiceManager InvoiceManager { get; }
        public PlayerAlcoholManager PlayerAlcoholManager { get; }
        public VehicleInfoManager VehicleInfoManager { get; }
        public PlayerFineManager PlayerFineManager { get; set; }
        public StorageManager StorageManager { get; set; }
        public EventManager EventManager { get; set; }
        public LifeServer server {get; }

        private CommandsManagement commandsManagement;

        
        public ElysiaMain(IGameAPI gameAPI) : base(gameAPI)
        {
            instance = this;
            ElysiaDB = new ElysiaDB();
            
            InvoiceManager = new InvoiceManager(ElysiaDB);
            PlayerAlcoholManager = new PlayerAlcoholManager();
            VehicleInfoManager = new VehicleInfoManager(ElysiaDB);
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            commandsManagement = new CommandsManagement();
            commandsManagement.parent = this;
            commandsManagement.Initialize();

            StorageManager = new StorageManager();
            EventManager = new EventManager();
            PlayerFineManager = new PlayerFineManager(ElysiaDB);

            Nova.man.StartCoroutine(CalculateDistance());
        }
        
        public override void OnPlayerEnterVehicle(Vehicle vehicle, int seatId, Player player)
        {
            Vehicle closest = player.GetClosestVehicle();

            VehicleInfo vehicleInfo = VehicleInfoManager.GetVehicleInfo(closest.vehicleDbId);

            if (vehicleInfo == null)
            {
                VehicleInfoManager.AddVehicleInfo(player.character.Id,closest.vehicleDbId);
            }
            VehicleInfoManager.PlayersDriving.Add(player);
        }
        
        public IEnumerator CalculateDistance()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f);

                if (VehicleInfoManager.PlayersDriving.Count != 0)
                {

                    foreach (Player p in VehicleInfoManager.PlayersDriving)
                    {
                        CharacterDriver driver = p.GetVehicle();
                        VehicleInfo drivingVehicleInfo = VehicleInfoManager.GetVehicleInfo(driver.vehicle.vehicleDbId);
                        double currentVelocityKmPerHour = driver.vehicle.newController.net.currentVelocity.magnitude * 3.6f; 
                        
                        drivingVehicleInfo.Kilometer += currentVelocityKmPerHour * (5f / 1000f);                        
                    }
                }

            }
        }
        public override void OnPlayerInput(Player player, KeyCode keyCode, bool onUI)
        {
            if (keyCode == KeyCode.P)
            {
                if (!player.HasBiz())
                {
                    NoBizMenu noBizMenu = new NoBizMenu(player);
                    
                    player.ShowPanelUI(noBizMenu);
                    return;
                }

                string activites = player.biz.Activities;
                JObject activitiesObject = JObject.Parse(activites);
                JArray jArray = activitiesObject["ids"] as JArray;

                if (jArray[0].Value<int>() == 0)
                {
                    player.ShowPanelUI(new CopsMenu("Interactions-Police", MenuType.Cops, player));
                }else if (jArray[0].Value<int>() == 2)
                {
                    player.ShowPanelUI(new MecanoMenu("Interactions-Mecano",MenuType.Mecano,player));
                }
                
            }
        }

        public override void OnPlayerConsumeAlcohol(Player player, int itemId, float alcoholValue)
        {
            PlayerAlcoholManager.AddAlcoholValue(player.character.Id,alcoholValue);
        }

        public override void OnPlayerExitVehicle(Vehicle vehicle, Player player)
        {
            if (VehicleInfoManager.PlayersDriving.Contains(player))
            {
                VehicleInfoManager.PlayersDriving.Remove(player);
            }
        }


        public InvoiceManager GetInvoiceManager()
        {
            return InvoiceManager;
        }
    }
}
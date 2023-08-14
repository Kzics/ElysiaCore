using System;
using System.Collections.Generic;
using Life;
using Life.Network;
using Life.UI;
using Life.VehicleSystem;

namespace ElysiaInteractMenu.Menu.Fine
{
    public class VehicleFineMenu : UIPanel
    {

        public Dictionary<string,int> Infractions = new Dictionary<string,int>();

        public VehicleFineMenu(Player player, Vehicle receiver) : base("Infractions", PanelType.Tab)
        {
            foreach (var crime in ElysiaMain.instance.StorageManager.ConfigStorage.GetVehicleCrimes())
            {
                AddTabLine($"Infraction:{crime.Key}     Amende:{crime.Value}", uiPanel =>
                {
                    foreach (var inf in Infractions)
                    {
                        PlayerFine playerFine = new PlayerFine()
                        {
                            Amount = inf.Value,
                            IsPaid = false,
                            Reason = inf.Key,
                            ReceiverId = Nova.v.GetVehicle(receiver.plate).permissions.owner.characterId,
                            SenderId = player.character.Id,
                            SentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                            Type = "VEHICLE",
                            VehicleId = receiver.vehicleDbId
                        };

                        ElysiaMain.instance.PlayerFineManager.PlayerFines.Add(playerFine);
                    }

                    int ownerId = Nova.v.GetVehicle(receiver.plate).permissions.owner.characterId;
                    Player owner = Nova.server.GetPlayer(ownerId);

                    if (owner != null)
                    {
                        owner.Notify("Police","Vous avez recu une <color=red>amende");

                    }
                    player.Notify("Police", "Vous avez envoyé une <color=green>amende");
                    
            
                    player.ClosePanel(this);
                });
            }
            
            AddButton("Envoyer", panel =>
            {
                panel.SelectTab();
            });

            
            AddButton("Ajouter Infraction", panel =>
            {
                string[] values = GetLinesAsArray()[panel.selectedTab].Split(':');
                foreach (var value in values)
                {
                    player.SendText(value);
                }

                if (Infractions.ContainsKey(values[1]))
                {
                    player.Notify("Police","Infraction déjà ajouté...");
                }
                Infractions.Add(values[1],Int32.Parse(values[2]));
                player.Notify("Police","Vous avez ajouté une <color=green>infraction");
            });
            

        }
    }
}
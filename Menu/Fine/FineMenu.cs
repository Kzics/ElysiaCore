using System;
using System.Collections.Generic;
using Life.Network;
using Life.UI;

namespace ElysiaInteractMenu.Menu.Fine
{
    public class FineMenu : UIPanel
    {
        public Dictionary<string,int> Infractions = new Dictionary<string,int>();
        public FineMenu(string title,Player player,Player receiver) : base(title,PanelType.Tab)
        {
            foreach (var entry in ElysiaMain.instance.StorageManager.ConfigStorage.GetCrimes())
            {
                AddTabLine($"Infraction:{entry.Key}   Amende:{entry.Value}", panel =>
                {
                    foreach (var infraction in Infractions)
                    {
                        PlayerFine playerFine = new PlayerFine
                        {
                            Amount = infraction.Value,
                            IsPaid = false,
                            Reason = infraction.Key,
                            ReceiverId = receiver.character.Id,
                            SenderId = player.character.Id,
                            SentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                            Type = "NORMAL",
                            VehicleId = 0
                        };
                        ElysiaMain.instance.PlayerFineManager.PlayerFines.Add(playerFine);
                        
                        ElysiaMain.instance.EventManager.OnPlayerSendFineEvent(playerFine,player,receiver);
                    }
                    
                    player.Notify("Police", "Vous avez envoyé une <color=green>amende");
                    receiver.Notify("Police","Vous avez recu une <color=red>amende");

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
using System;
using Life.Network;
using Life.UI;

namespace ElysiaInteractMenu.Menu.Fine
{
    public class FineMenu : UIPanel
    {
        public FineMenu(string title,Player player,Player receiver) : base(title,PanelType.Tab)
        {
            foreach (var entry in ElysiaMain.instance.StorageManager.ConfigStorage.GetCrimes())
            {
                AddTabLine($"Infraction:{entry.Key}   Amende:{entry.Value}", panel =>
                {
                    PlayerFine playerFine = new PlayerFine
                    {
                        Amount = entry.Value,
                        IsPaid = false,
                        Reason = entry.Key,
                        ReceiverId = receiver.character.Id,
                        SenderId = player.character.Id,
                        SentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                    };
                    
                    ElysiaMain.instance.PlayerFineManager.PlayerFines.Add(playerFine);
                    
                    player.Notify("Police", "Vous avez envoyé une <color=green>amende");
                    receiver.Notify("Police","Vous avez recu une <color=red>amende");

                    ElysiaMain.instance.EventManager.OnPlayerSendFineEvent(playerFine,player,receiver);

                    player.ClosePanel(this);
                });
            }

            AddButton("Envoyer", panel =>
            {
                panel.SelectTab();
            });

            AddButton("Envoyer", panel => panel.SelectTab());
        }
    }
}
using System;
using System.Collections.Generic;
using Life;
using Life.Network;
using Life.UI;

namespace ElysiaInteractMenu.Menu.Fine
{
    public class FineListMenu : UIPanel
    {
        public FineListMenu(string title, Player player) : base(title, PanelType.Tab)
        {
            int characterId = player.character.Id;
            List<PlayerFine> fines = ElysiaMain.instance.PlayerFineManager.GetPlayerFines(characterId);


            foreach (var fine in fines)
            {
                long difference = DateTimeOffset.Now.ToUnixTimeSeconds() - fine.SentTimestamp;
                double multiplier = 1;
                if (difference < 86400L)
                {
                    multiplier = 0.7;
                }else if (difference > 86400L && difference < 259200L)
                {
                    multiplier = 1;
                }else if (difference > 259200L && difference < 604800L)
                {
                    multiplier = 1.7;
                }
                
                AddTabLine($"Motif:{fine.Reason} Montant: {fine.Amount} * ({multiplier}) $", panel =>
                {
                    int bankAmount = player.character.Bank;

                    if (bankAmount - (fine.Amount * multiplier) < 0)
                    {
                        player.Notify("Police","Vous n'avez pas assez d'argent pour payer");
                        return;
                    }

                    player.character.Bank -= Convert.ToInt32(fine.Amount * multiplier);
                    
                    player.Notify("Police","Vous avez payé l'amende");

                    ElysiaMain.instance.PlayerFineManager.PlayerFines.Remove(fine);

                    Player sender = Nova.server.GetPlayer(fine.SenderId);

                    player.SendText("Here");

                    if (sender != null)
                    {
                        sender.Notify("Police",player.GetFullName() + " a payé son amende");
                    }
                    player.ClosePanel(panel);
                });
            }

            AddButton("Payer", panel => panel.SelectTab());
            AddButton("Quitter",panel =>player.ClosePanel(panel));
        }

    }
}
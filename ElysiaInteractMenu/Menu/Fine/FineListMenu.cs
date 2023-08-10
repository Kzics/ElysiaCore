using System;
using System.Collections.Generic;
using Life;
using Life.Network;

namespace ElysiaInteractMenu.Fine
{
    public class FineListMenu : InteractMenu
    {
        public FineListMenu(string title, Player player) : base(title, MenuType.Cops, player)
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
                
                AddTabLine($"Motif:{fine.Reason} Montant: {fine.Amount} * {multiplier}", panel =>
                {
                    int bankAmount = player.character.Bank;

                    if (bankAmount - (fine.Amount * multiplier) < 0)
                    {
                        player.Notify("Police","Vous n'avez pas assez d'argent pour payer");
                        return;
                    }

                    player.character.Bank -= Convert.ToInt32(fine.Amount * multiplier);
                    
                    player.Notify("Police","Vous avez payé l'amende");

                    Player sender = Nova.server.GetPlayer(fine.SenderId);

                    if (sender != null)
                    {
                        sender.Notify("Police",player.GetFullName() + " a payé son amende");
                    }
                });
            }
            player.ShowPanelUI(this);
        }

    }
}
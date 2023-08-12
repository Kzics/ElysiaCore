using System.Collections.Generic;
using Life;
using Life.Network;
using Life.UI;

namespace ElysiaInteractMenu
{
    public class InvoiceMenu : UIPanel
    {
        public InvoiceMenu(string title, Player player) : base(title, PanelType.Tab)
        {
            int characterId = player.character.Id;
            List<Invoice> playerInvoices = ElysiaMain.instance.InvoiceManager.GetUnPaidInvoices(characterId);

            foreach (Invoice invoice in playerInvoices)
            {
                //var paidMessage = invoice.IsPaid ? "Payé" : "Impayé";
                var invoiceString = $"Raison: {invoice.Reason} Prix: {invoice.Amount} BizId: {invoice.BizId}";
                
                AddTabLine(invoiceString,(panel =>
                {
                    if (!ElysiaMain.instance.InvoiceManager.TryPayInvoice(player,invoice))
                    {
                        player.Notify("Factures","Vous n'avez pas assez d'argent",NotificationManager.Type.Error);
                        return;
                    }
                    player.Notify("Factures",$"Vous avez payé une facture <color=red>-{invoice.Amount}$ ",NotificationManager.Type.Error);
                    player.ClosePanel(panel);
                } ));
            }
            
            AddButton("Quitter", panel =>
            {
                player.ClosePanel(panel);

            })
                .AddButton("Payer", panel =>
                {
                    panel.SelectTab();
                });
            {
                
            }
        }
    }
}
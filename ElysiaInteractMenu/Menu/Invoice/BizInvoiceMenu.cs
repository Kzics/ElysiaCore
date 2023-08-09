using System.Collections.Generic;
using Life;
using Life.Network;
using Life.UI;

namespace ElysiaInteractMenu
{
    public class BizInvoiceMenu : UIPanel
    {
        public BizInvoiceMenu(string title,Player player,BizMenuType type) : base(title, PanelType.Tab)
        {

            AddButton("Payée", panel =>
                {
                    player.ClosePanel(panel);
                    BizInvoiceMenu bizInvoiceMenu = new BizInvoiceMenu("Factures Entreprise", player, BizMenuType.PAID);
                    player.ShowPanelUI(bizInvoiceMenu);
            })
                .AddButton("Impayée", panel =>
                {
                    player.ClosePanel(panel);
                    BizInvoiceMenu bizInvoiceMenu = new BizInvoiceMenu("Factures Entreprise", player, BizMenuType.UNPAID);
                    player.ShowPanelUI(bizInvoiceMenu);
                });
            if (type.Equals(BizMenuType.PAID))
            {
                int bizId = player.biz.Id;
                List<Invoice> paidInvoices = ElysiaMain.instance.InvoiceManager.GetPaidBizSentInvoices(bizId);

                foreach (Invoice invoice in paidInvoices)
                {
                    AddTabLine($"{invoice.Reason}: {invoice.Amount}: {invoice.BizId}", uiPanel => { });
                }
            } 
            else
            {
                int bizId = player.biz.Id;
                List<Invoice> unPaidInvoices =
                    ElysiaMain.instance.InvoiceManager.GetUnPaidBizSentInvoices(bizId);

                foreach (Invoice invoice in unPaidInvoices)
                {
                    AddTabLine($"{invoice.Reason}: {invoice.Amount}: {invoice.BizId}", uiPanel =>
                    {
                        Player cPlayer = Nova.server.GetPlayer(invoice.CharacterId);
                        if (cPlayer == null)
                        {
                            player.Notify("Factures", "Joueur non en ligne!", NotificationManager.Type.Error);
                            return;
                        }

                        player.Notify("Factures", "Le joueur a été <color=green> rappelé",
                            NotificationManager.Type.Success);
                        player.Notify("Factures", "Vous avez une facture à <color=red>payer",
                            NotificationManager.Type.Warning);
                    });
                }

                AddButton("Rappeler", panel =>
                {
                    panel.SelectTab();
                });
            }
        }
    }
}
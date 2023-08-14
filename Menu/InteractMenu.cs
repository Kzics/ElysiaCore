using System;
using ElysiaInteractMenu.Menu.Fine;
using Life;
using Life.DB;
using Life.Network;
using Life.VehicleSystem;
using UnityEngine;
using UIPanel = Life.UI.UIPanel;

namespace ElysiaInteractMenu.Menu
{
    public abstract class InteractMenu : UIPanel
    {
        public InteractMenu(String title, MenuType type,Player player) : base(title,PanelType.Tab)
        {
            Title = title;
            Type = type;
            
            if (IsPlayerPassenger(player))
            {
                AddTabLine("Verifier le kilometrage", panel =>
                {
                    UIPanel kmPanel = new UIPanel("Kilometrage",PanelType.Text);
                    VehicleInfo vehicleInfo = ElysiaMain.instance.VehicleInfoManager.GetVehicleInfo(player.GetClosestVehicle().vehicleDbId);
                    kmPanel.SetText($"La voiture a roulé {vehicleInfo.Kilometer}km");
                    kmPanel.AddButton("Confirmer",uiPanel => player.ClosePanel(uiPanel));
                    
                    player.ShowPanelUI(kmPanel);
                });
            }

            AddButton("Voir ma carte", panel =>
                {
                    Character character = player.GetCharacterJson();
                    player.setup.TargetCreateCNI(character);
                    player.ClosePanel(panel);
                })
                .AddButton("Valider", (panel =>
                {
                    panel.SelectTab();
                }))
                .AddTabLine("Montrer ma carte", panel =>
                {
                    Player closestPlayer = player.GetClosestPlayer();
                    if (closestPlayer == null)
                    {
                        player.Notify("Interactions", "Aucun joueur autour");
                        return;
                    }
                    
                    Character playerCharacter = player.GetCharacterJson();
                    closestPlayer.setup.TargetCreateCNI(playerCharacter);
                    player.ClosePanel(panel);
                }).AddTabLine("Fouiller la personne", panel =>
                {
                    Player closestPlayer = player.GetClosestPlayer();
                    if (closestPlayer == null)
                    {
                        player.Notify("Interactions", "Aucun joueur autour", NotificationManager.Type.Error);
                        return;
                    }

                    player.setup.TargetOpenPlayerInventory(closestPlayer.netId);
                })
                .AddTabLine("Voir mes factures", panel =>
                {
                    player.ClosePanel(panel);

                    InvoiceMenu menu = new InvoiceMenu("Factures", player);
                    player.ShowPanelUI(menu);
                }).AddTabLine("Payer mes amendes", panel =>
                {
                    player.ClosePanel(panel);
                    FineListMenu fineListMenu = new FineListMenu("Amendes", player);
                    player.ShowPanelUI(fineListMenu);
                });
            if (player.HasBiz())
            {
                AddTabLine("Verifier entreprises factures", panel =>
                {
                    BizInvoiceMenu bizInvoiceMenu = new BizInvoiceMenu("Factures Entreprise",player,BizMenuType.UNPAID);
                    player.ShowPanelUI(bizInvoiceMenu);
                }).AddTabLine("Faire une facture", panel =>
                {
                    if (!player.HasBiz())
                    {
                        player.Notify("Interactions", "Vous n'avez pas d'entreprise", NotificationManager.Type.Error);
                        return;
                    }
                    Player closestPlayer = player.GetClosestPlayer();
                    if (closestPlayer == null)
                    {
                        player.Notify("Interactions", "Aucun joueur autour", NotificationManager.Type.Error);
                        return;
                    }

                    Bizs playerBizs = player.biz;

                    UIPanel amountPanel = new UIPanel("Montant", PanelType.Input);

                    amountPanel.SetText("Montant").AddButton("Valider", uiPanel =>
                    {
                        string amount = uiPanel.inputText;
                        player.SendText(amount);
                        int result;

                        if (int.TryParse(amount, out result))
                        {
                            UIPanel reasonPanel = new UIPanel("Raison", PanelType.Input);
                            player.ClosePanel(uiPanel);
                            reasonPanel.SetText("Raison").AddButton("Valider", pan =>
                            {
                                var reason = pan.inputText;
                                ElysiaMain.instance.InvoiceManager.SendInvoice(playerBizs.Id,
                                    closestPlayer.character.Id, result, reason);
                                player.Notify("Factures", "Vous avez envoyé une <color=green>facture");
                                player.ClosePanel(pan);

                            });
                            player.ShowPanelUI(reasonPanel);
                        }
                        player.ClosePanel(panel);
                    });
                    player.ShowPanelUI(amountPanel);
                });
            }
            
        }
        public virtual string Title { get; protected set; }
        
        public virtual MenuType Type { get; protected set; }

        public virtual void OpenMenu(Player player)
        {
            player.ShowPanelUI(this);
        }
        private bool IsPlayerPassenger(Player player)
        {
            Vehicle vehicle = player.GetClosestVehicle();
            
            if (vehicle == null) return false;

            foreach (NetVehicleSeat netSeat in vehicle.netSeats)
            {
                if (netSeat.passengerId == player.conn.identity.netId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
using System;
using ElysiaInteractMenu.Menu;
using ElysiaInteractMenu.Menu.Fine;
using Life.Network;
using Life.UI;

namespace ElysiaInteractMenu
{
    public class CopsMenu : InteractMenu
    {
        public CopsMenu(String title, MenuType type,Player player) : base(title,type,player)
        {
            AddTabLine("Demande dépistage de stupéfiants", panel =>
                {
                    Player closestPlayer = player.GetClosestPlayer();
                    if (closestPlayer == null)
                    {
                        player.Notify("Police","Aucun joueur autour");
                        return;
                    }

                    UIPanel askPanel = new UIPanel("Dépistage",PanelType.Text);
                    askPanel.SetText("Acceptez vous le dépistage de stupéfiants ?");
                    askPanel.AddButton("Accepter", uiPanel =>
                    {
                        closestPlayer.ClosePanel(uiPanel);
                        closestPlayer.Notify("Police","Vous avez accepté le test");
                        player.Notify("Police", closestPlayer.GetFullName() + " a accepté");

                        UIPanel infoText = new UIPanel("Informations",PanelType.Text);
                        infoText.SetText(closestPlayer.setup.isDruged
                            ? "L'individu est sous l'emprise de stupéfiants"
                            : "L'individu est sobre");

                        infoText.AddButton("Confirmer", panel1 => player.ClosePanel(panel1));
                        player.ShowPanelUI(askPanel);
                    })
                        .AddButton("Refuser", uiPanel =>
                        {
                            closestPlayer.ClosePanel(uiPanel);
                            player.Notify("Police",closestPlayer.GetFullName() + " refuse");
                        });
                    closestPlayer.ShowPanelUI(askPanel);
                })
                .AddTabLine("Demande de depistage d'alcool", panel =>
                {
                    Player closestPlayer = player.GetClosestPlayer();
                    if (closestPlayer == null)
                    {
                        player.Notify("Police","Aucun joueur autour");
                        return;
                    }
                    UIPanel askPanel = new UIPanel("Dépistage",PanelType.Text);
                    askPanel.SetText("Acceptez vous le dépistage d'alcool ?");

                    askPanel.AddButton("Accepter", uiPanel =>
                    {
                        closestPlayer.ClosePanel(uiPanel);
                        closestPlayer.Notify("Police","Vous avez accepté le test");
                        player.Notify("Police", player.GetFullName() + " a accepté");

                        UIPanel infoText = new UIPanel("Informations",PanelType.Text);
                        float alcoholValue = ElysiaMain.instance.PlayerAlcoholManager.GetAlcoholValue(player.character.Id);
                        
                        infoText.SetText(alcoholValue >= 5f ? "L'individu est bourré" : "L'individu est sobre");
                        player.ShowPanelUI(infoText);
                        infoText.AddButton("Confirmer", panel1 => player.ClosePanel(panel1));
                        player.ShowPanelUI(askPanel);

                    }).AddButton("Refuser", uiPanel =>
                    {
                        closestPlayer.ClosePanel(uiPanel);
                        player.Notify("Police",player.GetFullName() + " refuse");
                    });
                    closestPlayer.ShowPanelUI(askPanel);
                }).AddTabLine("Mettre une amende", panel =>
                {
                    Player closestPlayer = player.GetClosestPlayer();
                    if (closestPlayer == null)
                    {
                        player.Notify("Police","Aucun joueur autour");
                        return;
                    }
                    player.ClosePanel(panel);
                    FineMenu fineMenu = new FineMenu("Amendes", player,player);
                    player.ShowPanelUI(fineMenu);
                });
        }

        public override void OpenMenu(Player player)
        {
            base.OpenMenu(player);
        }
    }
}
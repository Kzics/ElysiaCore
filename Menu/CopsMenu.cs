using System;
using System.Collections.Generic;
using ElysiaInteractMenu.Features.ADN;
using ElysiaInteractMenu.Menu;
using ElysiaInteractMenu.Menu.Fine;
using Life;
using Life.Entities;
using Life.Network;
using Life.UI;
using Life.VehicleSystem;
using Entity = Life.PermissionSystem.Entity;

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
                        closestPlayer.ClosePanel(askPanel);
                        closestPlayer.Notify("Police","Vous avez accepté le test");
                        player.Notify("Police", closestPlayer.GetFullName() + " a accepté");

                        UIPanel infoText = new UIPanel("Informations",PanelType.Text);
                        infoText.SetText(closestPlayer.setup.isDruged
                            ? "L'individu est sous l'emprise de stupéfiants"
                            : "L'individu est sobre");

                        infoText.AddButton("Confirmer", panel1 => player.ClosePanel(panel1));
                        player.ShowPanelUI(infoText);
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
                        closestPlayer.ClosePanel(askPanel);
                        closestPlayer.Notify("Police","Vous avez accepté le test");
                        player.Notify("Police", player.GetFullName() + " a accepté");

                        UIPanel infoText = new UIPanel("Informations",PanelType.Text);
                        float alcoholValue = ElysiaMain.instance.PlayerAlcoholManager.GetAlcoholValue(player.character.Id);
                        
                        infoText.SetText(alcoholValue >= 5f ? "L'individu est bourré" : "L'individu est sobre");
                        infoText.AddButton("Confirmer", panel1 => player.ClosePanel(panel1));
                        
                        player.ShowPanelUI(infoText);
                        
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
                    FineMenu fineMenu = new FineMenu("Amendes", player,closestPlayer);
                    player.ShowPanelUI(fineMenu);
                }).AddTabLine("Verifier le proprietaire d'un véhicule", panel =>
                {
                    UIPanel searchPanel = new UIPanel("Recherche",PanelType.Input);

                   searchPanel.AddButton("Rechercher", uiPanel =>
                   {
                       string plate = uiPanel.inputText;
                       LifeVehicle lifeVehicle = Nova.v.GetVehicle(plate);
                       if (lifeVehicle == null)
                       {
                           player.Notify("Police","Véhicule introuvable !");
                           return;
                       }

                       Entity playerEntity = lifeVehicle.permissions.owner;
                       if (playerEntity == null)
                       {
                           player.Notify("Police","Propriétaire introuvable");
                           return;
                       }

                       Player targetPlayer = Nova.server.GetPlayer(playerEntity.characterId);
                       
                       player.setup.TargetCreateCNI(targetPlayer.GetCharacterJson());
                       player.ClosePanel(uiPanel);
                       
                   });
                   
                   player.ShowPanelUI(searchPanel);
                   
                }).AddTabLine("Correspondance ADN", panel =>
                {
                    UIPanel searchPanel = new UIPanel("Recherche",PanelType.Input);

                    searchPanel.AddButton("Correspondance", uiPanel =>
                    {
                        string searchedDna = searchPanel.inputText;
                        if (searchedDna == null || searchedDna.Length < 9)
                        {
                            player.Notify("Police","ADN Invalide");
                            player.ClosePanel(uiPanel);
                            return;
                        }
                        List<PlayerDNA> foundDnas = ElysiaMain.instance.PlayerDnaManager.playerDNAS;

                        foreach (var dna in foundDnas)
                        {
                            if (dna.DnaCode.Equals(searchedDna))
                            {
                                player.SendText(dna + "  " + searchedDna);
                                int characterId = dna.CharacterId;
                                Player target = Nova.server.GetPlayer(characterId);
                                if (target == null)
                                {
                                    player.Notify("Police","ADN Non associé...");
                                    return;
                                }
                                player.setup.TargetCreateCNI(target.GetCharacterJson());
                                
                                player.ClosePanel(uiPanel);
                                return;
                            }
                        }
                        player.Notify("Police","Aucune correspondance trouvé...");
                    });
                    
                    player.ShowPanelUI(searchPanel);
                });
        }
        
    }
}
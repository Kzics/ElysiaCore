using System.Collections;
using ElysiaInteractMenu.Menu;
using Life;
using Life.CharacterSystem;
using Life.Network;
using Life.UI;
using Life.VehicleSystem;
using UnityEngine;

namespace ElysiaInteractMenu
{
    public class PoundMenu : InteractMenu
    {
        public PoundMenu(string title, Player player) : base(title, MenuType.Pound, player)
        {
            AddTabLine("Verifier le moteur", panel =>
            {
                CharacterDriver characterDriver = player.GetVehicle();
                if (!characterDriver.isDriver)
                {
                    player.Notify("Fourriere","Vous devez etre conducteur dans un véhicule");
                    return;
                }

                Vehicle vehicle = characterDriver.vehicle;
                float motorHealth = vehicle.motorHealth;

                UIPanel statePanel = new UIPanel("Etats",PanelType.Text);

                if (motorHealth >= 90)
                {
                    statePanel.SetText("Le moteur est en très bonne état");
                }else if (motorHealth < 90 && motorHealth >= 70)
                {
                    statePanel.SetText("Le moteur est en bonne état");

                }else if (motorHealth < 70 && motorHealth >= 40)
                {
                    statePanel.SetText("Le moteur est en mauvaise état");

                }else if (motorHealth < 40 && motorHealth >= 0)
                {
                    statePanel.SetText("Le moteur est en très mauvaise état");
                }

                statePanel.AddButton("Confirmer", uiPanel =>
                {
                    player.ClosePanel(uiPanel);
                });
                player.ShowPanelUI(statePanel);
            })
                .AddTabLine("Faire le plein d'essence", panel =>
                {
                    Vehicle vehicle = player.GetClosestVehicle();
                    if (vehicle == null)
                    {
                        player.Notify("Fourriere","Aucun véhicule autour");
                        return;
                    }

                    for (int i = 0; i < vehicle.netSeats.Count; i++)
                    {
                        if (vehicle.netSeats[i].passengerId == player.character.Id)
                        {
                            player.Notify("Fourriere","Vous ne pouvez pas remplir l'essence dans le véhicule");
                            break;
                        }
                    }

                    player.Notify("Fourriere","Vous avez commencé à mettre de l'essence");
                    Nova.man.StartCoroutine(FillFuel(vehicle,player));
                }).AddTabLine("Forcer le démarrage", panel =>
                {
                    CharacterDriver vehicle = player.GetVehicle();

                    if (vehicle.vehicle.netSeats[0].passengerId != player.character.Id)
                    {
                        player.Notify("Mecano","Vous devez etre dans un véhicule");
                        return;
                    }
                    if (!vehicle.isDriver)
                    {
                        player.Notify("Mecano","Vous devez etre <color=green>conducteur");
                        return;
                    }
                    vehicle.vehicle.newController.StartEngine();
                    vehicle.vehicle.vehicleController.engineStarted = true;

                    vehicle.vehicle.UpdateStartEngine();
                });;
        }


        private IEnumerator FillFuel(Vehicle vehicle, Player player)
        {
            while (vehicle.fuel < 100f)
            {
                Vehicle close = player.GetClosestVehicle();
                
                if(close == null)
                {
                    player.Notify("Fourriere","Vous etes trop loin du vehicule...");
                    break;
                };
                player.setup.TargetShowCenterText("Essence",$"Remplis à {vehicle.fuel}% ",5f);
                yield return new WaitForSeconds(5f);
                player.setup.custom.PlayEmotion("Angry"); 
                vehicle.fuel += 10;
            }
        }

    }
}
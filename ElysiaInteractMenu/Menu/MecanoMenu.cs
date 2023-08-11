using System;
using ElysiaInteractMenu.Menu;
using Life.CharacterSystem;
using Life.Network;

namespace ElysiaInteractMenu
{
    public class MecanoMenu : InteractMenu
    {
        public MecanoMenu(String title, MenuType type,Player player) : base(title,type,player)
        {
            AddTabLine("Forcer le démarrage", panel =>
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
            });
        }
        
    }
}
using System;
using Life;
using Life.BizSystem;
using Life.Network;

namespace ElysiaInteractMenu
{
    public class ElysiaPlayer
    {

        public Player Player;

        public long LastTookEquipment;

        public bool IsForceEnter;
        public ElysiaPlayer(Player player)
        {
            Player = player;
            IsForceEnter = false;
            LastTookEquipment = DateTime.Now.Millisecond - (60 * 1000L);
        }
        
        
        public bool IsPoliceMan()
        {
            if (!Player.HasBiz()) return false;

            foreach (Activity.Type activity in Nova.biz.GetBizActivities(Player.biz.Id))
            {
                if (activity == Activity.Type.LawEnforcement)
                {
                    return true;
                }
            }

            return false;
        }

        public void TakePoliceEquipment()
        {
        }
    }
}
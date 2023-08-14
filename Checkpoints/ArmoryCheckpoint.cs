using System;
using Life.CheckpointSystem;
using Life.Network;
using UnityEngine;

namespace ElysiaInteractMenu.Checkpoints
{
    public class ArmoryCheckpoint : NCheckpoint
    {
        
        public ArmoryCheckpoint(Player player,Vector3 pos,Action<NCheckpoint> action) : base(player.netId,pos,action)
        {
        }
        
    }
}
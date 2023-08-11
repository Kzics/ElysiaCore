using Life.Network;
using Mirror;

namespace ElysiaInteractMenu
{
    public interface IEvents
    {
        void OnPlayerConnect(Player player);
        void OnPlayerBuyTerrain(Player player,int var1, int var2);
        void OnPlayerDisconnect(NetworkConnection player);
        void OnPlayerMoney(Player player, int var1, string var2);
        void OnPlayerPickUpItem(Player player, int var1, int var2, int var3);
        void OnPlayerDeath(Player player);
        void OnPlayerDamage(Player player, Player victim, int var1);
        void OnPlayerKillPlayer(Player player, Player victim);
        void OnPlayerReceiveMoney(Player player, int var1, string var2);
        void OnHourPassed();
        void OnPlayerDropItem(Player player, int var1,int var2,int var3);
        void OnPlayerSendFine(PlayerFine fine, Player var1, Player var2);

    }
}
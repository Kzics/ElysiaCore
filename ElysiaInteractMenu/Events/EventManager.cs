using System;
using System.Threading.Tasks;
using ElysiaInteractMenu.Discord;
using ElysiaInteractMenu.Storage;
using Life;
using Life.InventorySystem;
using Life.Network;
using Mirror;

namespace ElysiaInteractMenu
{
    public class EventManager : IEvents
    {
        protected DiscordWebhookSender GlobalSender;
        protected DiscordWebhookSender LifeSender;
        public Action<PlayerFine,Player,Player> OnPlayerSendFineEvent;
        
        public EventManager()
        {
            ConfigStorage configStorage = ElysiaMain.instance.StorageManager.ConfigStorage;
            
            GlobalSender = new DiscordWebhookSender(configStorage.GetWebhookUrl("global_webhook"),EmbedType.Global);
            LifeSender = new DiscordWebhookSender(configStorage.GetWebhookUrl("life_webhook"),EmbedType.Life);
            
            Nova.server.OnPlayerDisconnectEvent += OnPlayerDisconnect;
            Nova.server.OnPlayerSpawnCharacterEvent += OnPlayerConnect;
            Nova.server.OnPlayerBuyTerrainEvent += OnPlayerBuyTerrain;
            Nova.server.OnPlayerReceiveItemEvent += OnPlayerPickUpItem;
            Nova.server.OnPlayerDamagePlayerEvent += OnPlayerDamage;
            Nova.server.OnPlayerKillPlayerEvent += OnPlayerKillPlayer;
            Nova.server.OnPlayerMoneyEvent += OnPlayerReceiveMoney;
            Nova.server.OnMinutePassedEvent += OnMinutePassed;
            Nova.server.OnPlayerDropItemEvent += OnPlayerDropItem;
            
            //Custom Events
            OnPlayerSendFineEvent += OnPlayerSendFine;
        }

        public void OnPlayerSendFine(PlayerFine fine, Player var1, Player var2)
        {
            Task.Run(async () =>
            {
                await LifeSender.SendMessageAsync($"{var1.steamUsername} a envoyé une amende à {var2.steamUsername} pour le motif {fine.Reason}",embed:true);
            });
        }

        public void OnPlayerDropItem(Player player, int var1, int var2, int var3)
        {
            Task.Run(async () =>
            {
                if (ElysiaMain.instance.StorageManager.BannedItems.GetBannedItems().Contains(var1))
                {
                    Item item = Nova.man.item.GetItem(var1);
                    if(item == null)return;
                    await GlobalSender.SendMessageAsync($"{player.steamUsername} a drop {var3}x {var1}-{item.itemName}",embed:true);

                }
            });
        }

        public void OnMinutePassed()
        {
            Task.Run(async () =>{
                if(ElysiaMain.instance.PlayerAlcoholManager == null) return;
                
                await ElysiaMain.instance.VehicleInfoManager.Save();
                await ElysiaMain.instance.InvoiceManager.Save();
                await ElysiaMain.instance.PlayerFineManager.Save();

                PlayerAlcoholManager alcoholManager = ElysiaMain.instance.PlayerAlcoholManager;
                foreach (int key in alcoholManager.AlcoholData.Keys)
                {
                    long lastDrink = alcoholManager.GetLastAlcoholTimestamp(key);

                    if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= lastDrink + (60 * 1000L))
                    {
                        alcoholManager.LastAlcoholTimestamps.Remove(key);
                    }
                }
            });
        }

        public void OnHourPassed()
        {
            foreach (var fine in ElysiaMain.instance.PlayerFineManager.PlayerFines)
            {
                if (DateTimeOffset.Now.ToUnixTimeSeconds() > 604800L)
                {
                    int characterId = fine.ReceiverId;
                    Player player = Nova.server.GetPlayer(characterId);
                    if (player.isInGame)
                    {
                        player.Notify("Police","Vous avez été forcé de payer une amende");
                    }

                    player.character.Bank -= Convert.ToInt32(fine.Amount * 1.7);
                    fine.IsPaid = true;

                    int senderId = fine.SenderId;
                    Player sender = Nova.server.GetPlayer(senderId);
                    if (sender.isInGame)
                    {
                        player.Notify("Police",$"{player.GetFullName()} été forcé de payer une amende");
                    }
                }
                
            }
        }

        public void OnPlayerReceiveMoney(Player player, int var1, string var2)
        {
            Task.Run(async () =>
            {
                await LifeSender.SendMessageAsync($"{player.steamUsername} à reçu {var1} de la part d'un admin ({var2}",embed:true);
            });
        }

        public void OnPlayerKillPlayer(Player player, Player victim)
        {
            Task.Run(async () =>
            {
                await LifeSender.SendMessageAsync($"{player.steamUsername} a mis dans le coma {victim.steamUsername}",embed:true);
            });
        }

        public void OnPlayerDeath(Player player)
        {
            Task.Run(async () =>
            {
                await LifeSender.SendMessageAsync($"{player.steamUsername} est tombé dans le cas d'une cause inconnue",embed:true);
            });
        }

        public void OnPlayerDamage(Player player, Player victim, int var1)
        {
            Task.Run(async () =>
            {
                await LifeSender.SendMessageAsync($"{player.steamUsername} a agressé {victim.steamUsername} (-{var1}hp). Restant: {player.Health}",embed:true);
            });
        }

        public void OnPlayerPickUpItem(Player player, int var1, int var2, int var3)
        {
            Task.Run(async () =>
            {
                if (ElysiaMain.instance.StorageManager.BannedItems.GetBannedItems().Contains(var1))
                {
                    Item item = Nova.man.item.GetItem(var1);
                    if(item == null)return;
                    await GlobalSender.SendMessageAsync($"{player.steamUsername} a recuperé {var3}x {var1}-{item.itemName}",embed:true);
                }
            });
        }

        public void OnPlayerMoney(Player player, int var1, string var2)
        {
            Task.Run(async () =>
            {
                await GlobalSender.SendMessageAsync($"{player.steamUsername} a fait {var1} et {var2}",embed:true);
            });
        }

        public void OnPlayerConnect(Player player)
        {
            ElysiaMain.instance.PlayerFineManager.PlayerFines.Add(new PlayerFine()
            {
                Amount = 10,IsPaid = false,Reason = "Meurtre",
                ReceiverId = player.character.Id, SenderId = player.character.Id,SentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
            });
            
            Task.Run(async () =>
            {
                await GlobalSender.SendMessageAsync($"{player.steamUsername} ({player.conn.address}) a rejoint le serveur sous le personnage {player.GetFullName()}",embed:true);
            });
        }

        public void OnPlayerBuyTerrain(Player player, int var1, int var2)
        {
            Task.Run(async () =>
            {
                await GlobalSender.SendMessageAsync($"{player.steamUsername} ({player.conn.address}) a acheté le terrain {var1} pour {var2} euros",embed:true);
            });
        }

        public void OnPlayerDisconnect(NetworkConnection conn)
        {
            Player p = null;
            foreach (Player player in Nova.server.Players)
            {
                if (player.conn.address.Equals(conn.address))
                {
                    p = player;
                }
            }
            
            Task.Run(async () =>
            {
                await GlobalSender.SendMessageAsync($"{p?.steamUsername} ({conn.address}) a quitté le serveur",embed:true);
            });
        }
        /* public override async void OnPlayerText(Player player, string message)
         {
             if (!player.IsAdmin) return;
             if (!message.StartsWith("/")) return;
 
             string[] cmdArgs = message.Substring(1).Split(' ');
 
             string affectedPlayer = cmdArgs[1];
             string senderPlayer = player.steamUsername;
 
             DiscordWebhookSender sender = new DiscordWebhookSender(
                 "https://discord.com/api/webhooks/1138574860646367312/XvCTWkMfUqeyIPkNtdRJXawCxBAyXxQS_m_3p6Zd6rn0ISz759CMRgvCzIXkg65c1Gt8");
 
             switch (cmdArgs[0])
             {
                 case "ban-ip":
                     string bannedReason;
 
                     if (cmdArgs.Length < 3)
                     {
                         bannedReason = "Aucune";
                     }
                     else
                     {
                         string[] reasonArgs = new string[cmdArgs.Length - 2];
                         Array.Copy(cmdArgs, 2, reasonArgs, 0, cmdArgs.Length - 2);
 
                         bannedReason = string.Join(" ", reasonArgs);
                     }
                     
                     await sender.SendMessageAsync($"{senderPlayer} a ban-IP {affectedPlayer} pour le motif: {bannedReason}");
                     break;
                 case "kick":
                     string kickedReason;
                     
                     if (cmdArgs.Length < 3)
                     {
                         kickedReason = "Aucune";
                     }
                     else
                     {
                         string[] reasonArgs = new string[cmdArgs.Length - 2];
                         Array.Copy(cmdArgs, 2, reasonArgs, 0, cmdArgs.Length - 2);
 
                         kickedReason = string.Join(" ", reasonArgs);
                     }
                     
                     await sender.SendMessageAsync($"{senderPlayer} a kick {affectedPlayer} pour le motif: {kickedReason}");
                     break;
                 case "freeze":
                     await sender.SendMessageAsync($"{senderPlayer} a freeze {affectedPlayer}");
                     break;
                 case ""
             }
         }*/
    }
}
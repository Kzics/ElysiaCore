using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ElysiaInteractMenu.Checkpoints;
using ElysiaInteractMenu.Discord;
using ElysiaInteractMenu.Features.ADN;
using ElysiaInteractMenu.Storage;
using Life;
using Life.CharacterSystem;
using Life.CheckpointSystem;
using Life.InventorySystem;
using Life.Network;
using Life.UI;
using Mirror;

namespace ElysiaInteractMenu
{
    public class EventManager : IEvents
    {
        protected DiscordWebhookSender GlobalSender;
        protected DiscordWebhookSender LifeSender;
        public Action<PlayerFine,Player,Player> OnPlayerSendFineEvent;
        public Action<Player> OnPlayerPayFineEvent;

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
            Nova.server.OnPlayerUseCommandEvent = onPlayerUseCommand;

            //Custom Events
            OnPlayerSendFineEvent += OnPlayerSendFine;
            OnPlayerPayFineEvent += OnPlayerPayFine;
        }

        

        public void onPlayerUseCommand(Player player, SChatCommand command)
        {
            if (command.fullCommandName.Equals("/serviceadmin"))
            {
                Task.Run(async () =>
                {
                    if (player.serviceAdmin)
                    {
                        await LifeSender.SendMessageAsync($"**{player.steamUsername}** est en service admin", embed: true);
                    }
                    else
                    {
                        await LifeSender.SendMessageAsync($"**{player.steamUsername}** n'est plus en service admin", embed: true);

                    }
                });
            } else if (command.fullCommandName.Equals("/ban"))
            {
                Task.Run(async () =>
                {
                    command.action += async (player1, strings) =>
                    {
                        await LifeSender.SendMessageAsync($"{player.steamUsername} a **banni** un joueur",embed:true);
                    };
                });
            } else if (command.fullCommandName.Equals("/give"))
            {
                Task.Run(async () =>
                {
                    command.action += async (player1, strings) =>
                    {
                        if (strings.Length == 1)
                        {
                            return;
                        }
                        Item item = Nova.man.item.GetItem(strings[1]);

                        if (strings.Length == 3)
                        {
                            await LifeSender.SendMessageAsync($"{player.steamUsername} s'est donné **{strings[2]}x{item.itemName}**",
                                embed: true);
                        }
                        else
                        {
                            await LifeSender.SendMessageAsync($"{player.steamUsername} s'est donné **{item.itemName}**",
                                embed: true);
                        }
                    };
                });
            }
        }

        public void OnPlayerPayFine(Player player)
        {
            Task.Run(async () =>
            {
                await LifeSender.SendMessageAsync($"**{player.steamUsername}** a **payé** une amende",embed:true);
            });
        }

        public void OnPlayerSendFine(PlayerFine fine, Player var1, Player var2)
        {
            Task.Run(async () =>
            {
                await LifeSender.SendMessageAsync($"**{var1.steamUsername}** a **envoyé** une amende à **{var2.steamUsername}** pour le motif **{fine.Reason}**",embed:true);
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
                    await GlobalSender.SendMessageAsync($"**{player.steamUsername}** a drop **{var3}x {var1}-{item.itemName}**",embed:true);

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
                ElysiaMain.instance.StorageManager.BraceletStorage.Save();

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
                if (var2.Equals("ADMIN_GIVE_MONEY"))
                {
                    await LifeSender.SendMessageAsync($"**{player.steamUsername}** a **reçu** {var1}$ de la part d'un admin",embed:true);

                }
                if (var1 < 0)
                {
                    await LifeSender.SendMessageAsync($"**{player.steamUsername}** a **acheté** {var1}$ ({var2})",embed:true);

                }
                else
                {
                    await LifeSender.SendMessageAsync($"**{player.steamUsername}** a **reçu** {var1}$ ({var2})",embed:true);

                } 
            });
        }

        public void OnPlayerKillPlayer(Player player, Player victim)
        {
            Task.Run(async () =>
            {
                await LifeSender.SendMessageAsync($"**{player.steamUsername}** a mis dans le coma **{victim.steamUsername}**",embed:true);
            });
        }

        public void OnPlayerDeath(Player player)
        {
            Task.Run(async () =>
            {
                await LifeSender.SendMessageAsync($"**{player.steamUsername}** est tombé dans le cas d'une cause inconnue",embed:true);
            });
        }

        public void OnPlayerDamage(Player player, Player victim, int var1)
        {
            Task.Run(async () =>
            {
                await LifeSender.SendMessageAsync($"**{player.steamUsername}** a **agressé** **{victim.steamUsername}** avec **{player.setup.interaction._equipedItem.itemName}**(-{var1}hp). Restant: {player.Health - var1}",embed:true);
            });
        }

        public void OnPlayerPickUpItem(Player player, int var1, int var2, int var3)
        {
            //player.SendText(var1 + "" + var2 + "" + var3);
            
            Task.Run(async () =>
            {
                if (ElysiaMain.instance.StorageManager.BannedItems.GetBannedItems().Contains(var1))
                {
                    Item item = Nova.man.item.GetItem(var1);
                    if(item == null)return;
                    await GlobalSender.SendMessageAsync($"**{player.steamUsername}** a recuperé **{var3}x {var1}-{item.itemName}**",embed:true);
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
            ElysiaMain.instance.Players.Add(new ElysiaPlayer(player));
            
            if (ElysiaMain.instance.PlayerDnaManager.getDna(player.character.Id) == null)
            {
                ElysiaMain.instance.PlayerDnaManager.playerDNAS.Add(new PlayerDNA()
                {
                    CharacterId = player.character.Id,
                    DnaCode = GenerateRandomCode(9),
                    IsFound = false
                });
            }
            Task.Run(async () =>
            {

                await GlobalSender.SendMessageAsync($"**{player.steamUsername}** ({player.conn.address}) a rejoint le serveur sous le personnage **{player.GetFullName()}**",embed:true);
            });
            ConfigStorage configStorage = ElysiaMain.instance.StorageManager.ConfigStorage;
            
            NCheckpoint armoryCheckpoint = new NCheckpoint(player.netId,configStorage.GetPolicePosition(),(
                checkpoint =>
                {
                    ElysiaPlayer elysiaPlayer = new ElysiaPlayer(player);
                    if (!elysiaPlayer.IsPoliceMan())
                    {
                        player.Notify("Police","Vous n'etes pas policier",NotificationManager.Type.Error);
                        
                        return;
                    }

                    UIPanel servicePanel = new UIPanel("Equipements",UIPanel.PanelType.Tab);

                    if (elysiaPlayer.LastTookEquipment + (60 * 1000L) < DateTime.Now.Millisecond)
                    {
                        servicePanel.AddTabLine("Recuperer équipement", panel =>
                        {
                            player.setup.inventory.AddItem(6, 1, "");
                            player.setup.inventory.AddItem(7, 48, "");
                            player.setup.inventory.AddItem(36, 1, "");

                            player.Notify("Police", "Vous etes en service!");
                        });

                        elysiaPlayer.LastTookEquipment = DateTime.Now.Millisecond;
                    }
                    else
                    {
                        player.Notify("Info","Vous pouvez recuperer un equipement toute les heures");
                    }

                    servicePanel.AddTabLine("Rendre l'équipement", panel =>
                    {
                        player.setup.inventory.RemoveItem(6,1,true);
                        player.setup.inventory.RemoveItem(7, 48,true);
                        player.setup.inventory.RemoveItem(36, 1,true);
                        
                        player.Notify("Police","Vous n'êtes plus en service!");

                    });

                    servicePanel.AddButton("Confirmer",panel => panel.SelectTab());
                    
                    player.ShowPanelUI(servicePanel);
                } ));
            
            player.CreateCheckpoint(armoryCheckpoint);
            player.SendText("created");
        }
        private string GenerateRandomCode(int length)
        {
            const string alphabet = "1234567890";
            StringBuilder codeBuilder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                int randomIndex = new Random().Next(alphabet.Length);
                char randomChar = alphabet[randomIndex];
                codeBuilder.Append(randomChar);
            }
            string randomCode = codeBuilder.ToString();

            while (ElysiaMain.instance.PlayerDnaManager.getDna(codeBuilder.ToString()) != null)
            {
                randomCode = GenerateRandomCode(length);
            }

            return randomCode;
        }

        public void OnPlayerBuyTerrain(Player player, int var1, int var2)
        {
            Task.Run(async () =>
            {
                await GlobalSender.SendMessageAsync($"**{player.steamUsername}** ({player.conn.address}) a acheté le terrain **{var1}** pour **{var2}$**",embed:true);
            });
        }

        public void OnPlayerDisconnect(NetworkConnection conn)
        {
            ElysiaMain.instance.Players.Remove(new ElysiaPlayer(GetPlayerFromConn(conn)));
            Task.Run(async () =>
            {
                await GlobalSender.SendMessageAsync($" ({conn.address}) a **quitté** le serveur",embed:true);
            });
        }

        public Player GetPlayerFromConn(NetworkConnection conn)
        {
            foreach (var p in Nova.server.Players)
            {
                if (p.netId.Equals(conn.identity.netId))
                {
                    return p;
                }
            }

            return null;
        }
    }
}
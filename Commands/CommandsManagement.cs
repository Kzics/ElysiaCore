using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElysiaInteractMenu.Discord;
using Life;
using Life.DB;
using Life.Network;
using Life.UI;
using Mirror;
using UnityEngine;

namespace ElysiaInteractMenu.Commands
{
    public class CommandsManagement
    {
        public ElysiaMain parent;

        private Dictionary<Player, Vector3> playersBring = new Dictionary<Player, Vector3>();
        public static string PlayerDontExist = "<color=red> Joueur introuvable";

        private DiscordWebhookSender Sender = new DiscordWebhookSender(ElysiaMain.instance.StorageManager.ConfigStorage.GetWebhookUrl("staff_webhook"),EmbedType.Staff);

        public void Initialize()
        {
            GetDiscord();
            GetSite();
            GetPlayerPosition();
            Bring();
            Return();
            TeleportTo();
            Teleport();
            TeleportAreas();
            RegisterArea();
            DeleteArea();
            GivePoints();
            RemovePoints();
            CancelPrison();
            FreezePlayer();
            UnFreezePlayer();
            FreezeArea();
            UnFreezeArea();
            BanIP();
            UnBanIP();
            Kick();
            Mute();
            Rename();
            Ping();
            MessageToPlayer();
            ClearInventory();
            Urgence();
            LastJoined();
            PingList();
        }

        private Player GetPlayerWithName(Player sender, string name)
        {

            foreach (Player player in Nova.server.Players)
            {
                if (player.steamUsername.Equals(name))
                {
                    return player;
                }
            }

            return null;
        }

        private void GetDiscord()
        {
            SChatCommand cmdGetDiscord = new SChatCommand("/discord", "Renvoie le discord", "/discord", (player, args) =>
            {
                player.SendText("Discord : ");
            });

            cmdGetDiscord.Register();
        }

        private void GetSite()
        {
            SChatCommand cmdGetSite = new SChatCommand("/site", "Renvoie le site", "/site", (player, args) =>
            {
                player.SendText("Site : ");
            });

            cmdGetSite.Register();
        }

        private void GetPlayerPosition()
        {
            SChatCommand cmdGetPlayerPosition = new SChatCommand("/pos", "Récupère la position de votre joueur", "/pos", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    player.SendText("Position actuelle : " + player.setup.transform.position);
                }

            });

            cmdGetPlayerPosition.Register();
        }

        private void Bring()
        {
            SChatCommand cmdBring = new SChatCommand("/bring", "Téléporte le joueur renseigné à toi", "/bring", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer != null)
                    {
                        playersBring.Add(argPlayer, argPlayer.setup.transform.position);
                        argPlayer.setup.TargetSetPosition(player.setup.transform.position);
                        
                        player.SendText("<color=green> Vous avez ramené un joueur");
                        argPlayer.SendText("<color=green> Vous avez été amené par un admin");
                        Task.Run(async () =>
                        {
                            await Sender.SendMessageAsync($"{player.steamUsername} a ramené à lui {argPlayer.steamUsername}",embed:true);
                        });
                    }
                    else
                    {
                        player.SendText("Téléportation impossible.");
                    }
                }
            });

            cmdBring.Register();
        }

        private void Return()
        {
            SChatCommand cmdReturn = new SChatCommand("/return", "Téléporte le joueur à sa position d'origine", "/return", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer != null)
                    {
                        argPlayer.setup.TargetSetPosition(playersBring[argPlayer]);
                        playersBring.Remove(argPlayer);
                        player.SendText("<color=green> Vous avez retourné un joueur");
                        argPlayer.SendText("<color=green> Vous avez été retourné par un admin");

                        Task.Run(async () =>
                        {
                            await Sender.SendMessageAsync($"{player.steamUsername} a retourné le joueur {argPlayer.steamUsername}",embed:true);
                        });
                    }
                    else
                    {
                        player.SendText("Téléportation impossible.");
                    }
                }
            });

            cmdReturn.Register();
        }

        private void LastJoined()
        {
            SChatCommand lastJoinedCmd = new SChatCommand("/lastjoined", "derniere connexion joueur", "/lastjoined",
                 (player, strings) =>
                 {
                     if (strings.Length != 1)
                     {
                         player.SendText("<color=red> Mauvais usage");
                         return;
                     }

                     Player targetPlayer = GetPlayerWithName(player,strings[0]);
                     if (targetPlayer == null)
                     {
                         player.SendText(PlayerDontExist);
                         return;
                     }
                     Task.Run(async () =>
                     {
                         Characters characterAsync = await LifeDB.db.Table<Characters>()
                             .Where(characters => characters.AccountId == player.account.id)
                             .FirstOrDefaultAsync();

                         long lastDisconnect = characterAsync.LastDisconnect;
                         DateTime lastDisconnectDate = DateTimeOffset.FromUnixTimeMilliseconds(lastDisconnect).DateTime;

                         string formattedDate = lastDisconnectDate.ToString("MM/dd/yyyy HH:mm:ss");
                         player.SendText($"Dernière connexion de {targetPlayer.steamUsername}: {formattedDate}");
                     });
                 });
            
            lastJoinedCmd.Register();
        }

        private void Urgence()
        {
            SChatCommand cmdUrgence = new SChatCommand("/urgence","Téléporte tout les admins","/urgence",
                (player, strings) =>
                {
                    if(!player.IsAdmin) return;

                    if (strings.Length > 2 || strings.Length == 0)
                    {
                        player.SendText("<color=green> Usage : /urgence <raison en un mot>");
                        return;
                    }

                    string reason = strings[0];

                    Vector3 senderPosition = player.setup.gameObject.transform.position;

                    List<Player> players = GetAdminPlayers();

                    foreach (Player p in players)
                    {
                        UIPanel confirmMenu = new UIPanel("Urgence",UIPanel.PanelType.Text);

                        confirmMenu.SetText($"Une urgence a été déclenché : {reason}");
                        confirmMenu.AddButton("Teleporter", panel =>
                        {
                            p.setup.TargetSetPosition(senderPosition);
                            p.ClosePanel(panel);
                        });
                        confirmMenu.AddButton("Décliner", panel =>
                        {
                            Task.Run(async () =>
                            {
                                await Sender.SendMessageAsync($"{p.steamUsername} a refusé l'urgence de {player.steamUsername}",embed:true);
                            });
                            p.ClosePanel(panel);
                        });
                        
                        p.ShowPanelUI(confirmMenu);
                    }
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"@here {player.steamUsername} vient d'utiliser la commande /urgence !",embed:true);
                    });
                });
            cmdUrgence.Register();
        }

        private List<Player> GetAdminPlayers()
        {
            List<Player> currentPlayers = Nova.server.GetAllInGamePlayers();
            List<Player> adminPlayers = new List<Player>();

            for (int i = 0; i < currentPlayers.Count; i++)
            {
                if (!currentPlayers[i].IsAdmin) continue;
                
                adminPlayers.Add(currentPlayers[i]);
            }

            return adminPlayers;
        }

        private void TeleportTo()
        {
            SChatCommand cmdTeleportTo = new SChatCommand("/tpto", "Téléporte le joueur à un autre joueur", "/tpto", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer1 = GetPlayerWithName(player, args[0]);
                    Player argPlayer2 = GetPlayerWithName(player, args[1]);

                    if (argPlayer1 != null && argPlayer2 != null)
                    {
                        argPlayer1.setup.TargetSetPosition(argPlayer2.setup.transform.position);
                        player.SendText("<color=green> Vous avez été téléporté à un joueur");

                        Task.Run(async () =>
                        {
                            await Sender.SendMessageAsync($"{player.steamUsername} à téléporté {argPlayer1.steamUsername} sur {argPlayer2.steamUsername}",embed:true);
                        });
                    }
                    else
                    {
                        player.SendText("Téléportation impossible.");
                    }
                }
            });

            cmdTeleportTo.Register();
        }

        private void Teleport()
        {
            SChatCommand cmdTeleport = new SChatCommand("/tp", "Téléporte le joueur à point précis", "/tp", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Vector3 position = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
                    player.setup.TargetSetPosition(position);
                    player.SendText("<color=green> Vous avez été téléporté à une position");

                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} s'est téléporté en {player.setup.transform.position}",embed:true);
                    });
                }
            });

            cmdTeleport.Register();
        }

        private void TeleportAreas()
        {
            SChatCommand cmdTeleportAreas = new SChatCommand("/tpareas", "Téléporte le joueur à point précis", "/tpareas", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    UIPanel adminAreasPanel = new UIPanel("Points de Téléportation", UIPanel.PanelType.TabPrice);
                    adminAreasPanel.AddButton("Sélectionner", (ui) => { adminAreasPanel.SelectTab(); });
                    adminAreasPanel.AddButton("Fermer", (ui) => { player.ClosePanel(ui); });

                    foreach (var area in ElysiaMain.instance.adminAreas)
                    {
                        adminAreasPanel.AddTabLine("Point : " + area.Key, (ui) => {

                            player.setup.TargetSetPosition(area.Value);
                            player.SendText("<color=green> Vous avez été téléporté dans une zone");

                        });
                    }

                    player.ShowPanelUI(adminAreasPanel);
                }
            });

            cmdTeleportAreas.Register();
        }

        private void RegisterArea()
        {
            SChatCommand cmdRegisterAreas = new SChatCommand("/registerarea", "Sauvegarde un point de téléportation", "/registerarea", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    ElysiaMain.instance.AddArea(args[0], player.setup.transform.position);

                    player.SendText("Point de téléportation ajouter avec succès.");
                }
            });

            cmdRegisterAreas.Register();
        }

        private void DeleteArea()
        {
            SChatCommand cmdDeleteArea = new SChatCommand("/deletearea", "Supprime un point de téléportation", "/deletearea", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    //parent.RemoveArea(args[0]);

                    player.SendText("<color=green> Point de téléportation supprimé avec succès.");
                }
            });

            cmdDeleteArea.Register();
        }

        private void GivePoints()
        {
            SChatCommand cmdGivePoints = new SChatCommand("/givepoints", "Donner des points sur un permis", "/givepoints", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer == null) { return; }

                    argPlayer.character.PermisPoints += int.Parse(args[1]);
                    argPlayer.character.Save();

                    player.SendText("Vous avez donné : " + args[1] + ".");
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} a donné {args[1]} points à {argPlayer.steamUsername}",embed:true);
                    });
                }
            });

            cmdGivePoints.Register();
        }

        private void RemovePoints()
        {
            SChatCommand cmdRemovePoints = new SChatCommand("/removepoints", "Retirer des points sur un permis", "/removepoints", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);


                    if (argPlayer == null) { return; }


                    argPlayer.character.PermisPoints -= int.Parse(args[1]);
                    argPlayer.character.Save();

                    player.SendText("Vous avez retiré : " + args[1] + ".");
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} a retiré {args[1]} points à {argPlayer.steamUsername}",embed:true);
                    });
                }
            });

            cmdRemovePoints.Register();
        }

        private void CancelPrison()
        {
            SChatCommand cmdCancelPrison = new SChatCommand("/cancelprison", "Annule la prison", "/cancelprison", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer == null) { return; }

                    argPlayer.character.PrisonTime = 0;

                    player.SendText("<color=green> Vous avez annulé la prison du joueur.");
                }
            });

            cmdCancelPrison.Register();
        }

        private void FreezePlayer()
        {
            SChatCommand cmdFreezePlayer = new SChatCommand("/freeze", "Freeze un joueur", "/freeze", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer == null) { return; }

                    player.setup.NetworkisFreezed = true;
                    argPlayer.setup.TargetExitVehicle();


                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} a freeze {argPlayer.steamUsername}",embed:true);
                    });

                    player.SendText("<color=green> Vous avez freeze un joueur.");
                    argPlayer.SendText("<color=green> Vous avez été freeze");
                }
            });

            cmdFreezePlayer.Register();
        }

        private void UnFreezePlayer()
        {
            SChatCommand cmdUnFreezePlayer = new SChatCommand("/unfreeze", "UnFreeze un joueur", "/unfreeze", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer == null) { return; }

                    player.setup.NetworkisFreezed = false;
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} a unfreeze {argPlayer.steamUsername}",embed:true);
                    });

                    player.SendText("<color=green> Vous avez unfreeze un joueur.");
                    argPlayer.SendText("<color=green> Vous avez été unfreeze");
                }
            });

            cmdUnFreezePlayer.Register();
        }

        private void FreezeArea()
        {
            SChatCommand cmdFreezeArea = new SChatCommand("/freezearea", "Freeze une zone", "/freezearea", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    foreach (Player argPlayer in Nova.server.Players)
                    {
                        if ((!argPlayer.IsAdmin || argPlayer.character.Level != 10) && Vector3.Distance(player.setup.transform.position, argPlayer.setup.transform.position) < 100)
                        {
                            player.setup.CmdAdminFreezePlayer(argPlayer.character.Id);
                            argPlayer.setup.TargetExitVehicle();
                        }
                    }

                    player.SendText("Vous avez freeze une zone autour de vous de 100m.");

                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} vient de freeze une zone aux coordonnées  {player.setup.transform.position}",embed:true);
                    });
                }
                else
                {
                    player.SendText("<color=red> Vous avez pas la permission");

                }
            });

            cmdFreezeArea.Register();
        }

        private void UnFreezeArea()
        {
            SChatCommand cmdUnFreezeArea = new SChatCommand("/unfreezearea", "DeFreeze une zone", "/unfreezearea", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    foreach (Player argPlayer in Nova.server.Players)
                    {
                        if (!argPlayer.IsAdmin && Vector3.Distance(player.setup.transform.position, argPlayer.setup.transform.position) < 115)
                        {
                            player.setup.NetworkisFreezed = false;
                        }
                    }

                    player.SendText("Vous avez unfreeze une zone autour de vous de 100m.");
                    
                    Vector3 playerPosition = player.setup.use.transform.position;
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} vient de unfreeze une zone aux coordonnées  x:{playerPosition.x},y:{playerPosition.y},z:{playerPosition.z}",embed:true);
                    });
                }
            });

            cmdUnFreezeArea.Register();
        }

        private void BanIP()
        {
            SChatCommand cmdBanIP = new SChatCommand("/banip", "BanIP un joueur", "/banip", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    //parent.AddBanIP(args[0]);

                    player.SendText("Vous avez BanIP : " + args[0]);
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} a ban-IP {args[0]}",embed:true);
                    });
                }
            });

            cmdBanIP.Register();
        }

        private void UnBanIP()
        {
            SChatCommand cmdUnBanIP = new SChatCommand("/unbanip", "UnBanIP un joueur", "/unbanip", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    //parent.RemoveBanIP(args[0]);

                    player.SendText("Vous avez UnBanIP : " + args[0]);
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} a unBan-IP {args[0]}",embed:true);
                    });
                }
            });

            cmdUnBanIP.Register();
        }

        private void Kick()
        {
            SChatCommand cmdKick = new SChatCommand("/kick", "Kick 15m un joueur", "/kick", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer == null)
                    {
                        player.SendText(PlayerDontExist);
                        return;
                    }

                    player.setup.CmdBanSubmit(argPlayer.character.Id, 900, 0, false, "Kick de 15m");

                    player.SendText("Vous avez Kick : " + args[0]);
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} a kick {args[0]}",embed:true);
                    });
                }
            });

            cmdKick.Register();
        }

        private void Mute()
        {
            SChatCommand cmdMute = new SChatCommand("/mute", "Mute un joueur", "/mute", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer == null)
                    {
                        player.SendText(PlayerDontExist);
                        return;
                    }

                    player.setup.CmdAdminMute(argPlayer.character.Id);

                    player.SendText("Vous avez mute un joueur.");
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} a mute {args[0]}",embed:true);
                    });
                }
            });

            cmdMute.Register();
        }

        private void Rename()
        {
            SChatCommand cmdRename = new SChatCommand("/rename", "Nettoie l'inventaire d'un joueur", "/rename", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer == null) { return; }

                    argPlayer.character.Firstname = args[1];
                    argPlayer.character.Lastname = args[2];
                    argPlayer.character.Save();

                    player.SendText("Vous avez rename le joueur.");
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} a rename {argPlayer.steamUsername} en {argPlayer.GetFullName()}",embed:true);
                    });
                }
            });

            cmdRename.Register();
        }
        
        private void Ping()
        {
            SChatCommand cmdPing = new SChatCommand("/ping", "Connaitre son ping", "/ping", (player, args) =>
            {

                if (args.Length != 1)
                {
                    player.SendText("<color=green> Vous avez " + Math.Round(NetworkTime.rtt * 1000) + "ms");
                    return;
                }

                Player targetPlayer = GetPlayerWithName(player,args[0]);
                if (targetPlayer == null)
                {
                    player.SendText("<color=red>Joueur inexistant...");
                    return;
                }
                player.SendText($"<color=green> Vous avez {Math.Round(NetworkTime.rtt * 1000)} ms");
            });

            cmdPing.Register();
        }

        private void PingList()
        {
            SChatCommand cmdPingList = new SChatCommand("/pinglist", "Connaitre son ping", "/pinglist", (player, args) =>
            {
                List<Player> connectedPlayers = new List<Player>(Nova.server.Players);

                connectedPlayers.Sort((a, b) =>(int) (NetworkTime.rtt * 1000 - NetworkTime.rtt * 1000));

                string message = "<color=green>Classement des meilleurs pings :\n";
                for (int i = 0; i < connectedPlayers.Count; i++)
                {
                    int ping =(int) NetworkTime.rtt * 1000;
                    message += $"{i + 1}. {connectedPlayers[i].account.username} - {ping} ms\n";
                }

                player.SendText(message);
            });

            cmdPingList.Register();

        }
        
        

        private void MessageToPlayer()
        {
            SChatCommand cmdMessage = new SChatCommand("/msg", "Connaitre son ping", "/ping", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer == null)
                    {
                        player.SendText("<color=red> Joueur introuvable");
                        return;
                    }

                    string message = null;
                    for (int i = 1; i < args.Length; i++)
                    {
                        message += " " + args[i];
                    }

                    argPlayer.setup.TargetShowCenterText("Message STAFF", message, 10f);
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} vient d'envoyer {message} à  {args[0]}",embed:true);
                    });
                }
            });

            cmdMessage.Register();
        }

        private void ClearInventory()
        {
            SChatCommand cmdClearInventory = new SChatCommand("/clearinventory", "Nettoie l'inventaire d'un joueur", "/clearinventory", (player, args) =>
            {
                if (player.IsAdmin || player.character.Level == 10)
                {
                    Player argPlayer = GetPlayerWithName(player, args[0]);

                    if (argPlayer == null) { return; }

                    argPlayer.setup.inventory.Clear();

                    player.SendText("Vous avez nettoyé l'inventaire du joueur.");
                    
                    
                    Task.Run(async () =>
                    {
                        await Sender.SendMessageAsync($"{player.steamUsername} vient de nettoyer l'inventaire de {args[0]}",embed:true);
                    });
                }
            });

            cmdClearInventory.Register();
        }
    }

}
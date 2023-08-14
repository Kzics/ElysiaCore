using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ElysiaInteractMenu.Commands;
using ElysiaInteractMenu.Discord;
using ElysiaInteractMenu.Manager;
using ElysiaInteractMenu.Menu.Fine;
using Life;
using Life.AreaSystem;
using Life.BizSystem;
using Life.CharacterSystem;
using Life.Network;
using Life.UI;
using Life.VehicleSystem;
using Socket.Newtonsoft.Json;
using Socket.Newtonsoft.Json.Linq;
using Socket.WebSocket4Net.System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElysiaInteractMenu
{
    public class ElysiaMain : Plugin
    {
        private ElysiaDB ElysiaDB { get; }
        public static ElysiaMain instance { get; set; }
        public InvoiceManager InvoiceManager { get; }
        public PlayerAlcoholManager PlayerAlcoholManager { get; }
        public VehicleInfoManager VehicleInfoManager { get; }
        public PlayerFineManager PlayerFineManager { get; set; }
        public StorageManager StorageManager { get; set; }
        public PlayerDNAManager PlayerDnaManager { get; set; }
        public EventManager EventManager { get; set; }
        public Dictionary<int, int> BizMoney = new Dictionary<int, int>();
        private CommandsManagement commandsManagement;
        public DiscordWebhookSender BizSender;
        public Dictionary<InteractableDoor,List<string>> doors = new Dictionary<InteractableDoor,List<string>>();
        public List<ElysiaPlayer> Players = new List<ElysiaPlayer>();
        
        public Dictionary<string, Vector3> adminAreas { private set; get; } = new Dictionary<string, Vector3>();


        
        public ElysiaMain(IGameAPI gameAPI) : base(gameAPI)
        {
            instance = this;
            ElysiaDB = new ElysiaDB();
            
            InvoiceManager = new InvoiceManager(ElysiaDB);
            PlayerAlcoholManager = new PlayerAlcoholManager();
            VehicleInfoManager = new VehicleInfoManager(ElysiaDB);
            PlayerDnaManager = new PlayerDNAManager(ElysiaDB);
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            StorageManager = new StorageManager();
            BizSender = new DiscordWebhookSender(StorageManager.ConfigStorage.GetWebhookUrl("biz_webhook"),EmbedType.Biz);

            commandsManagement = new CommandsManagement();
            commandsManagement.parent = this;
            commandsManagement.Initialize();

            EventManager = new EventManager();
            PlayerFineManager = new PlayerFineManager(ElysiaDB);
            LoadAreasFile();
            LoadCurrentBizBank();

            Nova.man.StartCoroutine(CalculateDistance());
            Nova.man.StartCoroutine(BizEvent());
            Task.Run(() =>
            {
                Nova.man.StartCoroutine(Perform());
                // NetworkServer.Spawn(CreateMenuCanvas(),player.conn);
            });


        }
        
        public override void OnPlayerEnterVehicle(Vehicle vehicle, int seatId, Player player)
        {
            Vehicle closest = player.GetClosestVehicle();
            Debug.Log("t");

            VehicleInfo vehicleInfo = VehicleInfoManager.GetVehicleInfo(closest.vehicleDbId);

            if (vehicleInfo == null)
            {
                VehicleInfoManager.AddVehicleInfo(player.character.Id,closest.vehicleDbId);
            }
            VehicleInfoManager.PlayersDriving.Add(player);


            
            List<PlayerFine> vehicleFines = PlayerFineManager.GetPlayerFines(player.character.Id);

            if (vehicleFines != null && vehicleFines.Count != 0)
            {
                FineListMenu fineListMenu = new FineListMenu("Rappel Amendes",player);
                player.ShowPanelUI(fineListMenu);
            }
        }
        
        public void LoadCurrentBizBank()
        {
            foreach (var biz in Nova.biz.bizs)
            {
                BizMoney.Add(biz.Id,biz.Bank);
            }
        }
        
        public IEnumerator CalculateDistance()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f);

                if (VehicleInfoManager.PlayersDriving.Count != 0)
                {

                    foreach (Player p in VehicleInfoManager.PlayersDriving)
                    {
                        CharacterDriver driver = p.GetVehicle();
                        VehicleInfo drivingVehicleInfo = VehicleInfoManager.GetVehicleInfo(driver.vehicle.vehicleDbId);
                        double currentVelocityKmPerHour = driver.vehicle.newController.net.currentVelocity.magnitude * 3.6f; 
                        
                        drivingVehicleInfo.Kilometer += currentVelocityKmPerHour * (5f / 1000f);                        
                    }
                }

            }
        }

        public IEnumerator BizEvent()
        {
            while (true)
            {
                yield return new WaitForSeconds(10f);

                foreach (var biz in Nova.biz.bizs)
                {
                    int result;

                    if (BizMoney.TryGetValue(biz.Id,out result))
                    {
                        if (biz.Bank != result)
                        {
                            int difference = biz.Bank - result;
                            if (difference > 0)
                            {
                                Task.Run(async () =>
                                {
                                    await BizSender.SendMessageAsync($"Un membre a ajouté **{difference}$** de l'entreprise {biz.BizName} - {biz.Id}",embed:true);
                                });
                            }
                            else
                            {
                                Task.Run(async () =>
                                {
                                    await BizSender.SendMessageAsync($"Un membre a retiré **{difference}$** del'entreprise {biz.BizName} - {biz.Id}",embed:true);
                                });
                            }

                            BizMoney[biz.Id] = biz.Bank;
                        }
                    }
                    else
                    {
                        BizMoney.Add(biz.Id,biz.Bank);
                    }

                }
            }
        }
        private GameObject CreateMenuCanvas()
        {
            // Créer un nouvel objet pour contenir le Canvas
            GameObject menuCanvas = new GameObject("MenuCanvas");
    
            // Ajouter un composant Canvas au nouvel objet
            Canvas canvas = menuCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    
            // Créer le panneau du menu
            GameObject menuPanel = new GameObject("MenuPanel");
            menuPanel.transform.SetParent(menuCanvas.transform);
    
            // Ajouter un composant Image pour le fond noir
            Image image = menuPanel.AddComponent<Image>();
            image.color = Color.black;
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.sizeDelta = Vector2.zero;

            return menuCanvas;
        }

        public  IEnumerator Perform()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                
                foreach (var player in Nova.server.GetAllInGamePlayers())
                {
                    FieldInfo hitField =
                        typeof(CharacterInteraction).GetField("hit", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (hitField != null)
                    {
                        RaycastHit hitValue = (RaycastHit)hitField.GetValue(player.setup.interaction);
                        if (hitValue.collider != null && hitValue.collider.tag != null)
                        {
                            if (hitValue.collider.tag.Equals("Interaction/Door"))
                            {
                                InteractableDoor interactableDoor = hitValue.collider.GetComponent<InteractableDoor>();
                                if (!(bool)(UnityEngine.Object)interactableDoor)
                                    interactableDoor = hitValue.collider.GetComponentInParent<InteractableDoor>();
                                if(Nova.ui.GetAction(0))
                                {
                                    AddDnaToDoor(player,interactableDoor);
                                }

                                ElysiaPlayer elysiaPlayer = new ElysiaPlayer(player);
                                
                                if (!elysiaPlayer.IsPoliceMan()) continue;


                                Nova.ui.SetAction(2, "Enfoncer", interactableDoor.isLocked);
                                Nova.ui.SetAction(3,"Verifier Empreintes",true);
                                
                                Nova.ui.ShowActions(true);
                                
                                if (Nova.ui.GetAction(2) && interactableDoor.isLocked && !elysiaPlayer.IsForceEnter)
                                {
                                    player.SendText("Vous enfoncez la porte");

                                    player.setup.NetworkisFreezed = true;

                                    Nova.man.StartCoroutine(BreakingDoor(player,interactableDoor));
                                    elysiaPlayer.IsForceEnter = true;

                                }

                                if (Nova.ui.GetAction(3))
                                {
                                    UIPanel dnaPanel = new UIPanel("Empreintes",UIPanel.PanelType.Tab);
                                    foreach (var dna in doors[interactableDoor])
                                    {
                                        if (PlayerDnaManager.IsFound(dna))
                                        {
                                            dnaPanel.AddTabLine($"Adn: {dna}", panel =>
                                            {
                                            });
                                        }
                                        else
                                        {
                                            dnaPanel.AddTabLine($"Adn: {dna}    INCONNU", panel =>
                                            {
                                            });
                                            player.Notify("Police","Vous avez découvert de nouveaux <color=green> ADN");
                                            
                                            PlayerDnaManager.FindDna(dna);
                                        }
                                        
                                    }

                                    dnaPanel.AddButton("Confirmer", pan => player.ClosePanel(pan));
                                    Nova.server.SendLocalText("Procède à un relevé d’ADN et l’analyse",3f,player.setup.gameObject.transform.position);
                                    
                                    player.ShowPanelUI(dnaPanel);
                                }

                            }/*else if (hitValue.collider.tag.Equals("Placeable"))
                            {
                                Nova.ui.SetAction(0,"Utiliser",true);
                                Nova.ui.ShowActions(true);

                                if (Nova.ui.GetAction(0))
                                {
                                    player.SendText("Cliqué");
                                }
                            }*/
                        }
                    }
                }
            }
        }


        public ElysiaPlayer GetPlayer(Player player) =>
            Players.Where(p => p.Player.netId == player.netId).FirstOrDefault();

        private IEnumerator BreakingDoor(Player player, InteractableDoor interactableDoor)
        {
            yield return new WaitForSeconds(4f);
            interactableDoor.SetLocalDoorState(true, false);
            interactableDoor.UpdateServerDoorState();
            Nova.server.SendLocalText("Porte enfoncé",2f,player.setup.gameObject.transform.position);
            AddDnaToDoor(player,interactableDoor);
            
            player.setup.NetworkisFreezed = false;
        }

        private void AddDnaToDoor(Player player, InteractableDoor interactableDoor)
        {
            string playerDna = PlayerDnaManager.getDna(player.character.Id).DnaCode;
            doors.TryGetValue(interactableDoor,out List<string> doorDnas);
            if (doorDnas == null)
            {
                doors.Add(interactableDoor,new List<string>(){playerDna});
            }
            else
            {
                if (doorDnas.Count < 3)
                {
                    doorDnas.Add(playerDna);
                }
                else
                {
                    doorDnas.Remove(doorDnas[0]);
                    doorDnas.Add(playerDna);
                }
            }

        }
        

        public override void OnPlayerInput(Player player, KeyCode keyCode, bool onUI)
        {
            if (keyCode == KeyCode.P && !onUI)
            {
                /*Task.Run(() =>
                {
                    //Nova.man.StartCoroutine(Perform(player));
                    // NetworkServer.Spawn(CreateMenuCanvas(),player.conn);
                });*/

                if (!player.HasBiz())
                {
                    NoBizMenu noBizMenu = new NoBizMenu(player);
                    
                    player.ShowPanelUI(noBizMenu);
                    return;
                }
                string activites = player.biz.Activities;
                JObject activitiesObject = JObject.Parse(activites);
                JArray jArray = activitiesObject["ids"] as JArray;
                
                if (jArray[0].Value<int>() == 0)
                {
                    player.ShowPanelUI(new CopsMenu("Interactions-Police", MenuType.Cops, player));
                }else if (jArray[0].Value<int>() == 2)
                {
                    player.ShowPanelUI(new MecanoMenu("Interactions-Mecano",MenuType.Mecano,player));
                }else if (jArray[0].Value<int>() == 10)
                {
                    player.ShowPanelUI(new PoundMenu("Interactions-Fourriere",player));
                }
                else
                {
                    player.ShowPanelUI(new NoBizMenu(player));
                }
                
            }
        }

        public override void OnPlayerConsumeAlcohol(Player player, int itemId, float alcoholValue)
        {
            PlayerAlcoholManager.AddAlcoholValue(player.character.Id,alcoholValue);
        }

        public override void OnPlayerExitVehicle(Vehicle vehicle, Player player)
        {
            if (VehicleInfoManager.PlayersDriving.Contains(player))
            {
                VehicleInfoManager.PlayersDriving.Remove(player);
            }
        }


        private void SaveAreaFile(string name)
        {
            string path = pluginsPath + "/ElysiaCore/Areas/" + name + ".json";

            if (!File.Exists(path))
            {
                FileStream stream = new FileStream(path, FileMode.CreateNew);
                stream.Close();

                string[] content = new string[]
                {
                    name,
                    adminAreas[name].x.ToString(),
                    adminAreas[name].y.ToString(),
                    adminAreas[name].z.ToString(),
                };

                File.WriteAllText(path, JsonConvert.SerializeObject(content));
            }
        }

        private void LoadAreasFile()
        {
            if (!Directory.Exists(pluginsPath + "/ElysiaCore/Areas"))
            {
                Directory.CreateDirectory(pluginsPath + "/ElysiaCore/Areas");
            }
            
            string[] files = Directory.GetFiles(pluginsPath + "/ElysiaCore/Areas");
            for (int i = 0; i < files.Length; i++)
            {
                string read = File.ReadAllText(files[i]);
                try
                {
                    string[] content = JsonConvert.DeserializeObject<string[]>(read);

                    adminAreas.Add(content[0], new Vector3(
                        float.Parse(content[1]),
                        float.Parse(content[2]),
                        float.Parse(content[3])));
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        }
        public void AddArea(string name, Vector3 position)
        {
            if (adminAreas.ContainsKey(name)) { return; }

            adminAreas.Add(name, position);
            SaveAreaFile(name);
        }
    }
}
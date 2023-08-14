using System.IO;
using ElysiaInteractMenu.Storage;

namespace ElysiaInteractMenu.Manager
{
    public class StorageManager
    {

        public BannedItems BannedItems { get; }
        public ConfigStorage ConfigStorage { get; }
        public BraceletStorage BraceletStorage { get; }
        
        public StorageManager()
        {
            BannedItems = new BannedItems("bannedItems.json");
            //BannedItems.Load();
            ConfigStorage = new ConfigStorage("config.json");
            BraceletStorage = new BraceletStorage("bracelets.json");


        }
        
    }
}
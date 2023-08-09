using System.IO;
using ElysiaInteractMenu.Storage;

namespace ElysiaInteractMenu.Manager
{
    public class StorageManager
    {

        public BannedItems BannedItems { get; }
        public ConfigStorage ConfigStorage { get; }
        
        public StorageManager()
        {
            BannedItems = new BannedItems(Path.Combine(ElysiaMain.instance.pluginsPath, "ElysiaCore/bannedItems.json"));
            BannedItems.Load();

            //ConfigStorage = new ConfigStorage(Path.Combine(ElysiaMain.instance.pluginsPath, "ElysiaCore/config.json"));

        }
        
    }
}
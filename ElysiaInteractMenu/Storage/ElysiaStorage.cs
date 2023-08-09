using System.Collections.Generic;
using System.IO;
using Socket.Newtonsoft.Json.Linq;
using SQLite;

namespace ElysiaInteractMenu.Storage
{
    public abstract class ElysiaStorage
    {
        protected JObject _jObject;
        protected string _filePath;
        
        public ElysiaStorage(string filePath) 
        {
            _filePath = filePath;
        }

        public JArray GetJArray(string key)
        {
            if (key == null) return null;

            return (JArray)_jObject[key];
        }

        public int GetInt(string key)
        {
            if (key == null) return -1;
            return (int)_jObject[key];
        }

        public string GetString(string key)
        {
            if (key == null) return null;
            
            return (string)_jObject[key];
        }

        public virtual void Load()
        {
            _jObject = JObject.Parse(File.ReadAllText(_filePath));

        }
        public abstract void Save();
    }

    public class JsonStorage : ElysiaStorage
    {
        private string FilePath { get; set; }
        
        public JsonStorage(string filePath) : base(filePath)
        {
            FilePath = filePath;
        }

        public override void Save()
        {
            throw new System.NotImplementedException();
        }
    }

    public class SQLStorage : ElysiaStorage
    {
        private SQLiteAsyncConnection db;
        public SQLStorage(string filePath) : base(filePath)
        {
            _filePath = filePath;
        }

        public override void Save()
        {
        }
    }

    public class BannedItems : JsonStorage
    {
        private List<int> _bannedItems = new List<int>();
        public BannedItems(string filePath) : base(filePath)
        {
            _filePath = filePath;
            string folderPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, new JObject(
                    new JProperty("0",36),
                    new JProperty("1",106),
                    new JProperty("1",107),
                    new JProperty("2",37),
                    new JProperty("3",58),
                    new JProperty("4",96),
                    new JProperty("5",102),
                    new JProperty("6",103),
                    new JProperty("7",104),
                    new JProperty("8",105),
                    new JProperty("9",1159),
                    new JProperty("10",1172),
                    new JProperty("11",1235),
                    new JProperty("12",1296),
                    new JProperty("13",1368),
                    new JProperty("14",1369),
                    new JProperty("15",1463),
                    new JProperty("16",1464),
                    new JProperty("17",1465)
                ).ToString());
            }
        }

        public override void Load()
        {
            _jObject = JObject.Parse(File.ReadAllText(_filePath));
        }

        public List<int> GetBannedItems()
        {
            if (_bannedItems.Count != 0) return _bannedItems;
            

            if (_jObject == null) return new List<int>();
            List<int> banned = new List<int>();
            
            foreach (var token in _jObject)
            {
                banned.Add((int)token.Value);
            }

            return banned;
        } 

        public override void Save()
        {
        }
    }


    public class ConfigStorage : JsonStorage
    {
        public ConfigStorage(string filePath) : base(filePath)
        {
            string folderPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath,new JObject(
                    new JProperty("global_webhook","URL_WEBHOOK"),
                    new JProperty("staff_webhook","URL_WEBHOOK"),
                    new JProperty("life_webhook","URL_WEBHOOK"),
                    new JProperty("biz_webhook","URL_WEBHOOK")
                    ).ToString());
            }
        }

        public override void Load()
        {
            _jObject = JObject.Parse(File.ReadAllText(_filePath));

        }


        public string GetWebhookUrl(string key)
        {
            return (string) _jObject[key];
        }
    }
}
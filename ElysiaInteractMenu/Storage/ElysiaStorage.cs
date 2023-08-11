using System.Collections.Generic;
using System.IO;
using Socket.Newtonsoft.Json.Linq;
using SQLite;

namespace ElysiaInteractMenu.Storage
{
    public abstract class ElysiaStorage
    {
        protected JObject _jObject;
        protected string FileName;
        
        protected ElysiaStorage(string fileName) 
        {
            FileName = fileName;
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
            if (File.Exists(FileName))
            {
                _jObject = JObject.Parse(File.ReadAllText(FileName));
            }
        }
        public abstract void Save();
    }

    public class JsonStorage : ElysiaStorage
    {
            
        protected JsonStorage(string fileName) : base(fileName)
        {
        }
        
        public override void Save()
        {
            if (_jObject != null)
            {
                File.WriteAllText(FileName, _jObject.ToString());
            }
        }
    }

    public class SQLStorage : ElysiaStorage
    {
        private SQLiteAsyncConnection db;
        public SQLStorage(string fileName) : base(fileName)
        {
            FileName = fileName;
        }

        public override void Save()
        {
        }
    }

    public class BannedItems : JsonStorage
    {
        private List<int> _bannedItems = new List<int>();
        public BannedItems(string fileName) : base(fileName)
        {
            if (!Directory.Exists(Path.Combine(ElysiaMain.instance.pluginsPath,"ElysiaCore")))
            {
                Directory.CreateDirectory(Path.Combine(ElysiaMain.instance.pluginsPath,"ElysiaCore"));
            }

            if (!File.Exists(Path.Combine(ElysiaMain.instance.pluginsPath, "ElysiaCore/" + fileName)))
            {
                File.WriteAllText(Path.Combine(ElysiaMain.instance.pluginsPath, "ElysiaCore/" + fileName), new JObject(
                    new JProperty("key0", 36),
                    new JProperty("key1", 106),
                    new JProperty("key2", 107),
                    new JProperty("key3", 37),
                    new JProperty("key4", 58),
                    new JProperty("key5", 96),
                    new JProperty("key6", 102),
                    new JProperty("key7", 103),
                    new JProperty("key8", 104),
                    new JProperty("key9", 105),
                    new JProperty("key10", 1159),
                    new JProperty("key11", 1172),
                    new JProperty("key12", 1235),
                    new JProperty("key13", 1296),
                    new JProperty("key14", 1368),
                    new JProperty("key15", 1369),
                    new JProperty("key16", 1463),
                    new JProperty("key17", 1464),
                    new JProperty("key18", 1465)
                ).ToString());            
            }
            Load();
        }
        

        public override void Load()
        {
            _jObject = JObject.Parse(File.ReadAllText(Path.Combine(ElysiaMain.instance.pluginsPath,"ElysiaCore/" + FileName)));
        }

        public List<int> GetBannedItems()
        {
            if (_bannedItems.Count != 0) return _bannedItems;
            

            if (_jObject == null) return new List<int>(){1,2};
            List<int> banned = new List<int>();
            

            foreach (var property in _jObject.Properties())
            {
                if (property.Value is JValue value && value.Type == JTokenType.Integer)
                {
                    banned.Add((int)value);
                }
            }

            return banned;
        } 
        
    }


    public class ConfigStorage : JsonStorage
    {
        public ConfigStorage(string fileName) : base(fileName)
        {

            if (!Directory.Exists(Path.Combine(ElysiaMain.instance.pluginsPath,"ElysiaCore")))
            {
                Directory.CreateDirectory(Path.Combine(ElysiaMain.instance.pluginsPath,"ElysiaCore"));
            }

            if (!File.Exists(Path.Combine(ElysiaMain.instance.pluginsPath,"ElysiaCore/" + fileName)))
            {
                var jsonObject = new JObject(
                    new JProperty("global_webhook", "URL_WEBHOOK"),
                    new JProperty("staff_webhook", "URL_WEBHOOK"),
                    new JProperty("life_webhook", "URL_WEBHOOK"),
                    new JProperty("biz_webhook", "URL_WEBHOOK")
                );

                var crimesList = new JObject(
                    new JProperty("Meurtre", 500),
                    new JProperty("Delit de fuite", 100)
                );

                jsonObject.Add("infractions", crimesList);

                File.WriteAllText(Path.Combine(ElysiaMain.instance.pluginsPath, "ElysiaCore/" + fileName), jsonObject.ToString());            }
            Load();
        }

        public override void Load()
        {
            _jObject = JObject.Parse(File.ReadAllText(Path.Combine(ElysiaMain.instance.pluginsPath,"ElysiaCore/" + FileName)));

        }


        public string GetWebhookUrl(string key)
        {
            return (string) _jObject[key];
        }

        public Dictionary<string, int> GetCrimes()
        {
            Dictionary<string, int> infractionDict = new Dictionary<string, int>();
            if (_jObject.TryGetValue("infractions", out JToken infractions))
            {
                foreach (var infraction in infractions)
                {
                    string infractionName = ((JProperty)infraction).Name;
                    int infractionValue = (int)((JProperty)infraction).Value;
                    infractionDict.Add(infractionName, infractionValue);
                }
            }

            return infractionDict;
        }
    }
}
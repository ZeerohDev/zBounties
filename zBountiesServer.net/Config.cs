using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace zBountiesServer
{
    public class Config
    {
        private static string dataFolder = Environment.CurrentDirectory.Replace("\\", "/") + "/resources/zBounties/data/";
        private static string configLocation = dataFolder + "config.cfg";
        private static string bountyDataLocation = dataFolder + "bounties.cfg";
        public static List<BountyData> BountyDatas;
        public static ConfigData ConfigValues;

        public static void Initialize()
        {
            try
            {
                Directory.CreateDirectory(dataFolder);
                if (File.Exists(configLocation))
                {
                    ConfigValues = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(configLocation));
                }
                else
                {
                    ConfigValues = new ConfigData
                    {
                        Enabled = true,
                        WantedLevel1Reward = 100,
                        WantedLevel2Reward = 200,
                        WantedLevel3Reward = 300,
                        WantedLevel4Reward = 400,
                        WantedLevel5Reward = 500
                    };
                    using (StreamWriter writer = new StreamWriter(configLocation))
                    {
                        writer.WriteLine(JsonConvert.SerializeObject(ConfigValues));
                    }
                }
                if (File.Exists(bountyDataLocation))
                {
                    BountyDatas = JsonConvert.DeserializeObject<List<BountyData>>(File.ReadAllText(bountyDataLocation));
                }
                else
                {
                    BountyDatas = new List<BountyData>();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public static void WriteBountyData(Player player, int wantedLevel)
        {
            bool foundPlayer = false;
            List<string> playerIds = player.Identifiers.ToList();
            foreach (var bountyData in BountyDatas)
            {
                if (playerIds[1] == bountyData.SteamID)
                {
                    bountyData.Name = player.Name;
                    bountyData.LastWantedLevel = wantedLevel;
                    foundPlayer = true;
                    break;
                }
            }
            if (!foundPlayer)
            {
                BountyData bountyData = new BountyData
                {
                    Name = player.Name,
                    SteamID = playerIds[1]
                };
                BountyDatas.Add(bountyData);
            }
            using (StreamWriter writer = new StreamWriter(bountyDataLocation))
            {
                writer.WriteLine(JsonConvert.SerializeObject(BountyDatas));
            }
        }

        public static void ResetBountyData(BountyData toRemove)
        {
            BountyDatas.Remove(toRemove);
            using (StreamWriter writer = new StreamWriter(bountyDataLocation))
            {
                writer.WriteLine(JsonConvert.SerializeObject(BountyDatas));
            }
        }

        public static int GetRewardForWantedLevel(int wantedLevel)
        {
            switch (wantedLevel)
            {
                case 1:
                    return ConfigValues.WantedLevel1Reward;
                case 2:
                    return ConfigValues.WantedLevel2Reward;
                case 3:
                    return ConfigValues.WantedLevel3Reward;
                case 4:
                    return ConfigValues.WantedLevel4Reward;
                case 5:
                    return ConfigValues.WantedLevel5Reward;
                default:
                    return 0;
            }
        }

        public class ConfigData
        {
            public bool Enabled { get; set; }
            public int WantedLevel1Reward { get; set; }
            public int WantedLevel2Reward { get; set; }
            public int WantedLevel3Reward { get; set; }
            public int WantedLevel4Reward { get; set; }
            public int WantedLevel5Reward { get; set; }
        }

        public class BountyData
        {
            public string Name { get; set; }
            public string SteamID { get; set; }
            public int LastWantedLevel { get; set; }
        }
    }
}

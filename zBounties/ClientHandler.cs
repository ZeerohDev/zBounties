using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace zBountiesClient
{
    public class ClientHandler : BaseScript
    {
        public static int CurrWantedLevel = 0;
        public static bool PlayerPoolNeedsUpdate = false,
                           HasBounty = false,
                           BountiesUpdating = false;

        private static bool firstSpawn = false;

        private static List<ClientBounty> bounties = new List<ClientBounty>();

        public static void UpdateBounties(List<dynamic> data)
        {
            BountiesUpdating = true;
            bounties = new List<ClientBounty>();
            foreach (var datas in data)
            {
                string s = datas as string;
                string[] intData = s.Split(',');
                bounties.Add(new ClientBounty(int.Parse(intData[0]), int.Parse(intData[1])));
            }
            BountiesUpdating = false;
        }

        public static ClientBounty GetBountyForPlayer(int serverId)
        {
            foreach (var cBounty in bounties)
            {
                if (cBounty.ServerID == serverId)
                {
                    return cBounty;
                }
            }
            return null;
        }

        public static void RemoveClientBounty(ClientBounty bounty)
        {
            foreach (var cB in bounties)
            {
                if (bounty == cB)
                {
                    bounties.Remove(cB);
                }
            }
        }

        public static void OnPlayerDeath()
        {
            if (HasBounty)
            {
                UpdateWantedLevel(0, false);
                TriggerServerEvent("zBounties:resetBounty");
            }
        }

        public static void OnPlayerKilled(int killerID)
        {
            if (HasBounty)
            {
                UpdateWantedLevel(0, false);
                TriggerServerEvent("zBounties:resetBounty");
            }
        }

        public static void OnPlayerSpawned()
        {
            if (!firstSpawn)
            {
                TriggerServerEvent("zBounties:requestBountyData");
                firstSpawn = true;
            }
        }

        public static void VerifyKill(int reward, bool resetBounty)
        {
            TriggerServerEvent("es_me:addMoney", reward);
            if (resetBounty)
            {
                UpdateWantedLevel(0, false);
                TriggerServerEvent("zBounties:resetBounty");
            }
        }

        public static void VerifyDeath()
        {
            UpdateWantedLevel(0, false);
            TriggerServerEvent("zBounties:resetBounty");
        }

        public static void UpdateWantedLevel(int wantedLevel, bool hasBounty)
        {
            HasBounty = hasBounty;
            CurrWantedLevel = wantedLevel;
            SetPlayerWantedLevel(Game.Player.Handle, wantedLevel, false);
        }

        public static void NotifyPlayer(string message)
        {
            Notify.Custom(message, true, true);
        }
    }
}

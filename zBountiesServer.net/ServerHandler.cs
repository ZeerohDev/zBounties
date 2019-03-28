using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace zBountiesServer
{
    public class ServerHandler : BaseScript
    {
        public static bool PlayerPoolNeedsUpdate = false;

        public static void InitializeOnlinePlayers()
        {
            PlayerPoolNeedsUpdate = true;
            foreach (Player p in Globals.OnlinePlayers)
            {
                if (p == null) continue;
                var bountyData = GetBountyForPlayer(p);
                if (bountyData == null) continue;
                bool hasBounty = bountyData.LastWantedLevel > 0;
                p.TriggerEvent("zBounties:updateWantedLevel", bountyData.LastWantedLevel, hasBounty);
                p.TriggerEvent("zBounties:Notify", "Resuming your current bounty of ~g~$" + Config.GetRewardForWantedLevel(bountyData.LastWantedLevel) + "~s~.");
            }
        }

        public static void SetBountyForPlayer([FromSource]Player player, int wantedLevel)
        {
            if (player == null) return;
            Config.WriteBountyData(player, wantedLevel);
            PlayerPoolNeedsUpdate = true;
            foreach (Player p in Globals.OnlinePlayers)
            {
                if (player != p)
                {
                    TriggerClientEvent(p, "zBounties:Notify", player.Name + " has a bounty of ~g~$" + Config.GetRewardForWantedLevel(wantedLevel) + "~s~!");
                }
            }
            UpdateClientBounties();
            player.TriggerEvent("zBounties:Notify", "You now have a bounty of ~g~$" + Config.GetRewardForWantedLevel(wantedLevel) + "~s~. Stay frosty!");
        }

        public static void UpdateClientBounties()
        {
            List<dynamic> data = new List<dynamic>();
            PlayerPoolNeedsUpdate = true;
            foreach (Player p in Globals.OnlinePlayers)
            {
                var bountyData = GetBountyForPlayer(p);
                if (bountyData != null)
                {
                    data.Add(p.Handle + "," + Config.GetRewardForWantedLevel(bountyData.LastWantedLevel).ToString());
                }
            }
            TriggerClientEvent("zBounties:updateBounties", data);
        }

        public static void ResetBounty([FromSource]Player player)
        {
            if (player == null) return;
            var bountyData = GetBountyForPlayer(player);
            if (bountyData != null)
            {
                Config.ResetBountyData(bountyData);
            }
            UpdateClientBounties();
            player.TriggerEvent("zBounties:Notify", "Your bounty has been ~h~reset~s~.");
        }

        public static void RequestBountyData([FromSource]Player player)
        {
            if (player == null) return;
            var bountyData = GetBountyForPlayer(player);
            if (bountyData == null) return;
            if (bountyData.LastWantedLevel > 0)
            {
                player.TriggerEvent("zBounties:updateWantedLevel", bountyData.LastWantedLevel, true);
                player.TriggerEvent("zBounties:Notify", "Resuming your current bounty of ~g~$" + Config.GetRewardForWantedLevel(bountyData.LastWantedLevel) + "~s~.");
            }
            else
            {
                ResetBounty(player);
            }
        }

        public static void NotifyAll([FromSource] Player player, string msg)
        {
            TriggerClientEvent("zBounties:Notify", msg);
        }

        private static bool PlayerHasBounty(Player player)
        {
            return GetBountyForPlayer(player) != null;
        }

        private static Config.BountyData GetBountyForPlayer(Player player)
        {
            foreach (var bounty in Config.BountyDatas)
            {
                if (bounty.SteamID == player.Identifiers.ToList()[1])
                {
                    return bounty;
                }
            }
            return null;
        }
    }
}

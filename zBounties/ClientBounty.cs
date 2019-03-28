using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zBountiesClient
{
    public class ClientBounty
    {
        public int ServerID { get; set; }
        public int RewardAmount { get; set; }

        public ClientBounty(int serverId, int rewardAmount)
        {
            ServerID = serverId;
            RewardAmount = rewardAmount;
        }
    }
}

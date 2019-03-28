using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace zBountiesServer
{
    public class MainServer : BaseScript
    {
        public MainServer()
        {
            if (GetCurrentResourceName() != "zBounties")
            {
                Exception invalidNameException = new Exception("[zBounties] Installation Error:\r\nThe name of the resource is invalid. " +
                    "Please change the folder name from '^3" + GetCurrentResourceName() + "^1' to ^2'zBounties' (case sensitive) and restart the resource.\r\n\r\n\r\n^7");
                try
                {
                    throw invalidNameException;
                }
                catch (Exception e)
                {
                    Debug.Write(e.Message);
                }
            }
            else
            {
                Config.Initialize();
                Debug.WriteLine("zBounties Started!");
                Tick += PopulatePlayerList;
                EventHandlers.Add("zBounties:setPlayerBounty", new Action<Player, int>(ServerHandler.SetBountyForPlayer));
                EventHandlers.Add("zBounties:requestBountyData", new Action<Player>(ServerHandler.RequestBountyData));
                EventHandlers.Add("zBounties:resetBounty", new Action<Player>(ServerHandler.ResetBounty));
                ServerHandler.InitializeOnlinePlayers();
            }
        }

        private async Task PopulatePlayerList()
        {
            if (ServerHandler.PlayerPoolNeedsUpdate)
            {
                Globals.OnlinePlayers = Players.ToList();
                ServerHandler.PlayerPoolNeedsUpdate = false;
            }
        }
    }
}

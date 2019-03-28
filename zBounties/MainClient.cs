using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace zBountiesClient
{
    public class MainClient : BaseScript
    {
        private bool firstTick = true;
        private List<int> deadPlayers = new List<int>();

        public MainClient()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
        }

        private void OnClientResourceStart(string resourceName)
        {
            EventHandlers.Add("zBounties:updateWantedLevel", new Action<int, bool>(ClientHandler.UpdateWantedLevel));
            EventHandlers.Add("zBounties:Notify", new Action<string>(ClientHandler.NotifyPlayer));
            EventHandlers.Add("zBounties:updateBounties", new Action<List<dynamic>>(ClientHandler.UpdateBounties));
            EventHandlers.Add("zBounties:verifyKill", new Action<int, bool>(ClientHandler.VerifyKill));
            EventHandlers["baseevents:onPlayerDied"] += new Action(ClientHandler.OnPlayerDeath);
            EventHandlers["baseevents:onPlayerKilled"] += new Action<int>(ClientHandler.OnPlayerKilled);
            EventHandlers["playerSpawned"] += new Action(ClientHandler.OnPlayerSpawned);
            Tick += OnTick;
            Tick += PopulatePlayerList;
            Tick += Test;
        }

        private string GetSafePlayerName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }
            return name.Replace("^", @"\^").Replace("~", @"\~").Replace("<", "«").Replace(">", "»");
        }

        private async Task OnTick()
        {
            if (firstTick)
            {
                ClearBrief();
                firstTick = false;
            }
            int wL = GetPlayerWantedLevel(Game.Player.Handle);
            if (ClientHandler.CurrWantedLevel < wL && Game.Player.IsAlive)
            {
                ClientHandler.CurrWantedLevel = wL;
                ClientHandler.HasBounty = true;
                TriggerServerEvent("zBounties:setPlayerBounty", wL);
            }
            if (wL == 0 && ClientHandler.CurrWantedLevel > wL)
            {
                ClientHandler.UpdateWantedLevel(0, false);
                TriggerServerEvent("zBounties:resetBounty");
            }
            await Delay(100);
        }

        private async Task PopulatePlayerList()
        {
            if (ClientHandler.PlayerPoolNeedsUpdate)
            {
                Globals.OnlinePlayers = Players.ToList();
                ClientHandler.PlayerPoolNeedsUpdate = false;
            }
            await Delay(100);
        }

        private async Task Test()
        {
            foreach (Player player in Players)
            {
                if (player != Game.Player)
                {
                    if (player.IsDead)
                    {
                        if (deadPlayers.Contains(player.Character.Handle)) { return; }
                        deadPlayers.Add(player.Character.Handle);
                        Entity ent = player.Character.GetKiller();
                        if (ent != null)
                        {
                            if (ent.Handle == Game.Player.Character.Handle)
                            {
                                Debug.WriteLine("You killed " + player.Name);
                                while (ClientHandler.BountiesUpdating) { await Delay(10); }
                                ClientBounty bounty = ClientHandler.GetBountyForPlayer(player.ServerId);
                                if (bounty != null)
                                {
                                    Debug.WriteLine("Bounty is not null for entity");
                                    TriggerServerEvent("es_me:addMoney", bounty.RewardAmount);
                                    Debug.WriteLine("Rewarded player for bounty: " + bounty.RewardAmount);
                                    ClientHandler.NotifyPlayer("You have killed ~p~" + GetSafePlayerName(player.Name) + " ~s~for a total bounty reward of ~g~$" + bounty.RewardAmount + "~s~.");
                                    TriggerServerEvent("zBounties:notifyAll", "~p~" + GetSafePlayerName(Game.Player.Name) + " ~s~has killed ~p~" + GetSafePlayerName(player.Name) + " ~s~for a total bounty reward of ~g~$" + bounty.RewardAmount + "~s~.");
                                    if (ClientHandler.CurrWantedLevel < 3)
                                    {
                                        Debug.WriteLine("Removed bounty for good behaviour (less than 3 stars)");
                                        ClientHandler.UpdateWantedLevel(0, false);
                                        TriggerServerEvent("zBounties:resetBounty");
                                    }
                                    while (ClientHandler.BountiesUpdating) { await Delay(10); }
                                    ClientHandler.RemoveClientBounty(bounty);
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        if (deadPlayers.Contains(player.Character.Handle))
                        {
                            deadPlayers.Remove(player.Character.Handle);
                        }
                    }
                }
            }
            await Delay(10);
        }

        private async Task MonitorPlayerKills()
        {
            int entHandle = 0;
            bool entExists = GetEntityPlayerIsFreeAimingAt(Game.Player.Handle, ref entHandle);
            if (entExists)
            {
                Debug.WriteLine("Entity Exists");
                if (IsEntityAPed(entHandle))
                {
                    Debug.WriteLine("Entity is a Ped");
                    foreach (Player player in Players)
                    {
                        if (player.Character.Handle == entHandle)
                        {
                            Debug.WriteLine("Entity is " + player.Name);
                            if (player.IsDead)
                            {
                                Debug.WriteLine("Player is dead");
                                if (deadPlayers.Contains(entHandle)) { return; }
                                deadPlayers.Add(entHandle);
                                Debug.WriteLine("Entity added to dead players list");
                                if (player.Character.GetKiller().Handle == Game.Player.Character.Handle)
                                {
                                    Debug.WriteLine("You killed " + player.Name);
                                    while (ClientHandler.BountiesUpdating) { await Delay(1); }
                                    ClientBounty bounty = ClientHandler.GetBountyForPlayer(player.ServerId);
                                    if (bounty != null)
                                    {
                                        Debug.WriteLine("Bounty is not null for entity");
                                        TriggerServerEvent("es_me:addMoney", bounty.RewardAmount);
                                        Debug.WriteLine("Rewarded player for bounty: " + bounty.RewardAmount);
                                        ClientHandler.NotifyPlayer("You have killed ~p~" + GetSafePlayerName(player.Name) + " ~s~for a total bounty reward of ~g~$" + bounty.RewardAmount + "~s~.");
                                        TriggerServerEvent("zBounties:notifyAll", "~p~" + GetSafePlayerName(Game.Player.Name) + " ~s~has killed ~p~" + GetSafePlayerName(player.Name) + " ~s~for a total bounty reward of ~g~$" + bounty.RewardAmount + "~s~.");
                                        if (ClientHandler.CurrWantedLevel < 3)
                                        {
                                            Debug.WriteLine("Removed bounty for good behaviour (less than 3 stars)");
                                            ClientHandler.UpdateWantedLevel(0, false);
                                            TriggerServerEvent("zBounties:resetBounty");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (deadPlayers.Contains(entHandle))
                                {
                                    deadPlayers.Remove(entHandle);
                                }
                            }
                        }
                    }

                }
            }
            await Delay(1);
        }
    }
}

using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Linq;

namespace SystemFree
{
    public sealed class SystemFree : IDalamudPlugin
    {
        public string Name => "System Free";

        private const string commandName = "/sysfree";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ChatGui ChatGui { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        public SystemFree(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ChatGui chatGui,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.ChatGui = chatGui;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            ChatGui.ChatMessage += OnChat;

            this.PluginUi = new PluginUI(this.Configuration);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open settings"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);
            this.ChatGui.ChatMessage -= this.OnChat;
        }

        private void OnChat(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!Configuration.Enabled)
            {
                return;
            }

            if (type is XivChatType.SystemMessage)
            {
                isHandled = filterMessage(message.TextValue);
            } else if (type is not XivChatType.Say 
                                and not XivChatType.Debug
                                and not XivChatType.Echo
                                and not XivChatType.Shout
                                and not XivChatType.Yell
                                and not XivChatType.TellIncoming
                                and not XivChatType.TellOutgoing
                                and not XivChatType.Party
                                and not XivChatType.CrossParty
                                and not XivChatType.Alliance
                                and not XivChatType.FreeCompany
                                and not XivChatType.NoviceNetwork
                                and not XivChatType.PvPTeam
                                and not XivChatType.Ls1
                                and not XivChatType.Ls2
                                and not XivChatType.Ls3
                                and not XivChatType.Ls4
                                and not XivChatType.Ls5
                                and not XivChatType.Ls6
                                and not XivChatType.Ls7
                                and not XivChatType.Ls8
                                and not XivChatType.CrossLinkShell1
                                and not XivChatType.CrossLinkShell2
                                and not XivChatType.CrossLinkShell3
                                and not XivChatType.CrossLinkShell4
                                and not XivChatType.CrossLinkShell5
                                and not XivChatType.CrossLinkShell6
                                and not XivChatType.CrossLinkShell7
                                and not XivChatType.CrossLinkShell8
                      )
            {
                if (Configuration.NuclearMode)
                {
                    //Nuclear mode enabled?
                    isHandled = true;
                } else
                {
                    // Otherwise try to be more careful
                    isHandled = handleSpecialMessages(message.TextValue);
                }
            }

            // You are now in the instanced area Location Instance. Blah blah blah.
            string normalizedText = message.TextValue.ToLower();
            string[] instancedArea = { "you", "are", "now", "in", "the", "instanced", "area" };
            if (Configuration.BetterInstanceMessage && instancedArea.All(normalizedText.Contains))
            {
                // Some reformatting magic
                int index = message.TextValue.IndexOf('.');
                string instanceNumber = message.TextValue.Substring(index - 1, 1);
                message = "You are now in instance: " + instanceNumber;
            }

        }

        private bool filterMessage(string input)
        {
            try
            {
                // Blacklist all messages by default
                string normalizedText = input.ToLower();

                // Whitelist specific phrases

                // You sense the presence of a powerful mark...
                string[] powerfulMark = { "you", "sense", "the", "presense", "of", "a", "powerful" };
                // Retainer completed a venture.
                string[] completedVenture = { "completed", "a", "venture" };
                // You received a player commendation
                string[] playerCommendation = { "you", "received", "a", "player", "commendation" };
                // You are now in the instanced area Location Instance. Blah blah blah.
                string[] instancedArea = { "you", "are", "now", "in", "the", "instanced", "area" };

                if (
                    (powerfulMark.All(normalizedText.Contains) && !Configuration.HideSRankHunt) || 
                    (completedVenture.All(normalizedText.Contains) && !Configuration.HideCompletedVenture) || 
                    (playerCommendation.All(normalizedText.Contains) && !Configuration.HideCommendations) || 
                    (instancedArea.All(normalizedText.Contains) && !Configuration.HideInstanceMessage)
                   )
                {
                    return false;
                }

                // We hit the end of our whitelist - block the message
                return true;
            }
            // If we somehow encounter an error - block the message
            catch (Exception)
            {
                return true;
            }
        }

        private bool handleSpecialMessages(string input)
        {
            try
            {
                // Whitelist all messages by default
                string normalizedText = input.ToLower();

                // Blacklist specific phrases

                // You spent xxx gil.
                string[] spentGil = { "you", "spent", "gil"};
                // You pay retainer 1-2 ventures
                string[] oneVenture = { "you", "pay", "a", "venture" };
                string[] twoVentures = { "you", "pay", "2", "ventures" };
                // You assign your retainer "Mission"
                string[] assignVenture = { "you", "assign", "your", "retainer" };
                // <mission> is now complete.
                string[] missionComplete = { "", "is", "now", "complete" };
                // The <item> is added to your inventory.
                string[] tendedCrop = { "this", "crop", "is", "doing", "well" };
                // The <item> is added to your inventory.
                string[] itemtoInventory = { "the", "is", "added", "to", "your", "inventory" };
                // The <Duty> has begun.
                string[] dutyHasBegun = { "has", "begun" };
                // The <Duty> has ended.
                string[] dutyHasEnded = { "has", "ended" };
                // Commencing duty with an unrestricted party. If level sync has not been enabled, enemies will not yield EXP or items and gear will not gain spiritbond.
                string[] unsyncedParty = { "commencing", "duty", "with", "an", "unrestricted", "party" };
                // You have entered a duty with the Unrestricted Party option enabled. Damage dealt, healing potency, and maximum HP are increased by 300 %.
                string[] enteredUnsyncedDuty = { "you", "have", "entered", "a", "duty", "with", "the", "unrestricted", "party", "option", "enabled" };
                // You used an <aetheryte ticket> (Remaining: [number])
                string[] aetheryteTicket = { "you", "used", "an", "remaining"};
                // Your retainer's gear was sufficient to earn a greater reward!
                string[] goodGear = { "your", "retainer's", "gear", "was", "sufficient", "to", "earn", "a", "greater", "reward" };
                // Gil earned from market sales has been entrusted to your retainer.
                string[] gilEarned = { "gil", "earned", "from", "market", "sales", "has", "been", "entrusted", "to", "your", "retainer" };
                // <Retainer> has reached maximum level.
                string[] maxLevelRetainer = { "has", "reached", "maximum", "level" };
                // You sell a <item> for <amount> gil
                string[] soldItem = { "you", "sell", "for", "gil" };
                // You plant seeds in the <number> bed, <number> patch
                string[] plantSeeds = { "you", "plant", "seeds", "patch" };
                // You cast a glamour. The <item> takes on the apperance of a <item>
                string[] castGlamour = { "you", "cast", "a", "glamour", "takes", "on", "the", "appearance", "of" };
                // You cast a glamour. The <item> takes on the apperance of a <item>
                string[] glamoursProjected = { "glamours", "projected", "from", "plate" };
                // You change to <class>
                string[] changeClass = { "you", "change", "to" };
                // Your level has been synced to <level>
                string[] levelSync = { "your", "level", "has", "been", "synced", "to" };
                // Your level is no longer synced.
                string[] levelSyncedNoLonger = { "your", "level", "is", "no", "longer", "synced" };
                // <Area> will be sealed off in 15 seconds!
                string[] areaSealedOff = { "will", "be", "sealed", "off", "in", "15", "seconds" };
                // <Area> is sealed off!
                string[] areaSealed = { "is", "sealed", "off" };
                // <Area> is sealed off!
                string[] areaNoLongerSealed = { "is", "no", "longer", "sealed" };
                // You obtain a <typeOf> key.
                string[] obtainedKey = { "you", "obtain", "a", "key" };

                // ARR Dungeon Text
                // You are doused with poison!
                string[] sastashaPoison = { "you", "are", "doused", "with", "poison" };
                // You hear something move in the distance.
                string[] sastashaCorrectButton = { "you", "hear", "something", "move", "in", "the", "distance" };
                // A hidden door opens!
                string[] sastashaWhyPressTheButtonAgain = { "you", "discover", "a", "switch", "but", "it", "does", "not", "appear", "to", "function", "at", "this", "time" };
                // A hidden door opens!
                string[] sastashaHiddenDoor = { "a", "hidden", "door", "opens" };
                // The shallowscale Reaver drops a captain's quarters key.
                // The shallowtail Reaver drops a Waverider Gate key.
                string[] sastashaReaverDropsKey = { "the", "reaver", "drops", "a", "key" };                
                string[] sastashaGateOpens = { "waverider", "gate", "opens" };


                if (
                    spentGil.All(normalizedText.Contains) ||
                    oneVenture.All(normalizedText.Contains) || 
                    twoVentures.All(normalizedText.Contains) || 
                    assignVenture.All(normalizedText.Contains) ||
                    missionComplete.All(normalizedText.Contains) ||
                    tendedCrop.All(normalizedText.Contains) ||
                    dutyHasBegun.All(normalizedText.Contains) ||
                    dutyHasEnded.All(normalizedText.Contains) ||
                    unsyncedParty.All(normalizedText.Contains) ||
                    enteredUnsyncedDuty.All(normalizedText.Contains) ||
                    aetheryteTicket.All(normalizedText.Contains) ||
                    goodGear.All(normalizedText.Contains) ||
                    gilEarned.All(normalizedText.Contains) ||
                    maxLevelRetainer.All(normalizedText.Contains) ||
                    soldItem.All(normalizedText.Contains) ||
                    plantSeeds.All(normalizedText.Contains) ||
                    castGlamour.All(normalizedText.Contains) ||
                    changeClass.All(normalizedText.Contains) ||
                    glamoursProjected.All(normalizedText.Contains) ||
                    levelSync.All(normalizedText.Contains) ||
                    levelSyncedNoLonger.All(normalizedText.Contains) ||
                    areaSealedOff.All(normalizedText.Contains) ||
                    areaSealed.All(normalizedText.Contains) ||
                    areaNoLongerSealed.All(normalizedText.Contains) ||
                    sastashaPoison.All(normalizedText.Contains) ||
                    sastashaCorrectButton.All(normalizedText.Contains) ||
                    sastashaHiddenDoor.All(normalizedText.Contains) ||
                    obtainedKey.All(normalizedText.Contains) ||
                    sastashaReaverDropsKey.All(normalizedText.Contains) ||
                    sastashaGateOpens.All(normalizedText.Contains) ||
                    sastashaWhyPressTheButtonAgain.All(normalizedText.Contains) ||
                    itemtoInventory.All(normalizedText.Contains)
                   )
                {
                    return true;
                }

                // We hit the end of our blacklist - allow the message
                return false;
            }
            // If we somehow encounter an error - allow the message
            catch (Exception)
            {
                return false;
            }
        }


        private void OnCommand(string command, string args)
        {
            this.PluginUi.Visible = true;
        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }
    }
}

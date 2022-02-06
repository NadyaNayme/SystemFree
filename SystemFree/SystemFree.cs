using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Linq;
using ChatTwo.Code;

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

        // Lifted below lines (27-39) from Anna's chat2

        private const ushort Clear7 = ~(~0 << 7);
        internal ushort Raw { get; }
        internal ChatType Type => (ChatType)(this.Raw & Clear7);

        private static ChatType FromCode(ushort code)
        {
            return (ChatType)(code & Clear7);
        }

        private static ChatType FromDalamud(XivChatType type)
        {
            return FromCode((ushort)type);
        }

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
            // Lifted below line from Anna's chat2
            var chatType = FromDalamud(type);

            if (!Configuration.Enabled)
            {
                return;
            }

            if (chatType is ChatType.System)
            {
                isHandled = filterMessage(message.TextValue);
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

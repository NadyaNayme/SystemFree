using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System;

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
            this.ChatGui.CheckMessageHandled -= this.OnChat;
        }

        // Helper functions in case I need them
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "";
        }

        private void OnChat(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!Configuration.Enabled)
            {
                return;
            }

            if (message.TextValue.ToLower().Contains("you are now in the instanced area"))
            {
                // Some reformatting magic to get the index before the period;
                int index = message.TextValue.IndexOf('.');
                string instance = message.TextValue.Substring(index - 1, 1);
                message = "You are now in instance: " + instance;
            }

            if (type is XivChatType.SystemMessage)
            {
                isHandled |= filterMessage(type, message.TextValue);
            }
        }

        private bool filterMessage(XivChatType type, string input)
        {
            try
            {
                // Blacklist all messages by default
                bool allowedMessage = false;
                // Whitelist specific phrases
                allowedMessage = input.ToLower().Contains("you sense the presence of a powerful mark");
                allowedMessage = input.ToLower().Contains("has completed a venture!");
                allowedMessage = input.ToLower().Contains("you received a player commendation!");
                allowedMessage = input.ToLower().Contains("you are now in the instanced area");
                return allowedMessage;
            }
            // If we somehow encounter an error - show the message
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

using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;

namespace SystemFree
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;

        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void Dispose()
        {
            
        }

        public void Draw()
        {
            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("SystemFree", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = true;
                }
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(600, 255), ImGuiCond.Always);
            if (ImGui.Begin("SystemFree Settings", ref this.settingsVisible,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                var enabled = this.configuration.Enabled;
                if (ImGui.Checkbox("Enable filtering.", ref enabled))
                {
                    this.configuration.Enabled = enabled;
                    this.configuration.Save();
                }
                var additionalBlacklist = this.configuration.Blacklist;
                if (ImGui.Checkbox("Enable the additional blacklist.", ref additionalBlacklist))
                {
                    this.configuration.Blacklist = additionalBlacklist;
                    this.configuration.Save();
                }
                ImGuiComponents.HelpMarker("Tries to prevent system messages that are somehow ignoring their system channel categorization in the client from appearing. This includes messages from Gardening, Retainers, Duties, Item Interactions, Level Syncing and many more miscellaneous messages. It is recommended that you leave this enabled.");
                var nuclear = this.configuration.NuclearMode;
                if (ImGui.Checkbox("Enable nuclear mode. WARNING: May block messages from Guildmasters enable at your own risk.", ref nuclear))
                {
                    this.configuration.NuclearMode = nuclear;
                    this.configuration.Save();
                }
                ImGuiComponents.HelpMarker("Tries to filter any and all text that did not originate from a player in an attempt to any system messages from ever appearing.");
                var betterInstanceMessage = this.configuration.BetterInstanceMessage;
                if (ImGui.Checkbox("Improve the Instance message text.", ref betterInstanceMessage))
                {
                    this.configuration.BetterInstanceMessage = betterInstanceMessage;
                    this.configuration.Save();
                }
                ImGuiComponents.HelpMarker("Changes the instance text to: You are now in instance: #");
                var instanceMessage = this.configuration.HideInstanceMessage;
                if (ImGui.Checkbox("Hide Instance message", ref instanceMessage))
                {
                    this.configuration.HideInstanceMessage = instanceMessage;
                    this.configuration.Save();
                }
                ImGuiComponents.HelpMarker("Removes the instanced area notification from the whitelist.");
                var sRankHunt = this.configuration.HideSRankHunt;
                if (ImGui.Checkbox("Hide S Rank Hunt", ref sRankHunt))
                {
                    this.configuration.HideSRankHunt = sRankHunt;
                    this.configuration.Save();
                }
                ImGuiComponents.HelpMarker("Removes the announcement that an S Rank Hunt has spawned in the zone notification from the whitelist.");
                var commendations = this.configuration.HideCommendations;
                if (ImGui.Checkbox("Hide Commendations", ref commendations))
                {
                    this.configuration.HideCommendations = commendations;
                    this.configuration.Save();
                }
                ImGuiComponents.HelpMarker("Removes the you have earned a commendation notification from the whitelist.");
                var completedVenture = this.configuration.HideCompletedVenture;
                if (ImGui.Checkbox("Hide Completed Venture", ref completedVenture))
                {
                    this.configuration.HideCompletedVenture = completedVenture;
                    this.configuration.Save();
                }
                ImGuiComponents.HelpMarker("Removes the completed venture notification from the whitelist.");
                if (ImGui.Button("Save and Close Config"))
                {
                    this.configuration.Save();
                    SettingsVisible = false;
                }
            }
            ImGui.End();
        }
    }
}
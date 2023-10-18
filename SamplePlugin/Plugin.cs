using Dalamud.Game.Command;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using SamplePlugin.Effects;
using Dalamud.Logging;

using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;

namespace SamplePlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Wine Finder";
        private const string CommandName = "/winefinder";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        private IFramework Framework { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("Wine Finder");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        private FoodMonitor FoodMonitorScript { get; init; }

        [PluginService]
        public static IChatGui Chat { get; private set; }

        [PluginService]
        public static IClientState ClientState { get; private set; }



        [PluginService]
        public static IPartyList PartyList { get; private set; }


        [PluginService]
        public static IObjectTable ObjectTable { get; private set; }



        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager, 
            [RequiredVersion("1.0")] IFramework framework)

        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.Framework = framework;
            //this.IFramework = framework;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "profile.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, goatImage);
            FoodMonitorScript = new FoodMonitor(PluginInterface, Chat, PartyList, ObjectTable, ClientState);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            Framework.Update += OnFrameworkUpdate;

            Chat.Print("Plugin turned on and chat is working");

           // this.FoodMonitor.Update();

        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            
            ConfigWindow.Dispose();
            MainWindow.Dispose();
            FoodMonitorScript.Dispose();

            this.CommandManager.RemoveHandler(CommandName);

            Framework.Update -= OnFrameworkUpdate;
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            MainWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }

        private void OnFrameworkUpdate(IFramework framework)
        {

            try
            {
                FoodMonitorScript.Update();
                //Chat.Print("Turn the plugin off and remove this update is working");
            }
            catch (Exception ex)
            {
                Chat.Print("stop you just dumped like 5GB of frame errors");
            }
        }


    }
}

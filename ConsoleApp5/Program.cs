using System;
using System.Linq;
using PluginSystem;

namespace CLI
{
    public class Program
    {
        private const string pluginsDirectoryPath = "plugins";

        static void Main(string[] args)
        {
            var pluginLoader = new PluginLoader(pluginsDirectoryPath);
            var plugins = pluginLoader.LoadPlugins().ToList();

            if (args.Length == 0 || (args.Length == 1 && args[0] == "help"))
            {
                Display(plugins);
                return;
            }

            string pluginName = args[0];
            string commandName = args.Length > 1 ? args[1] : "help";
            string[] methodArgs = args.Skip(2).ToArray();

            var plugin = plugins.FirstOrDefault(p => p.Name.Equals(pluginName, StringComparison.OrdinalIgnoreCase));

            if (plugin == null)
            {
                Console.WriteLine($"Plugin '{pluginName}' not found.");
                return;
            }

            if (commandName == "help" || methodArgs.Contains("help"))
            {
                pluginLoader.ShowPluginHelp(plugin);
                return;
            }

            var result = plugin.ExecuteCommand(commandName, methodArgs);
            if (!result.Silent)
                Console.WriteLine(result.Message);
        }

        static void Display(IEnumerable<IPlugin> plugins)
        {
            Console.WriteLine("Available Plugins:");
            foreach (var plugin in plugins)
            {
                Console.WriteLine($"- {plugin.Name}");
            }
            Console.WriteLine("Usage: <plugin> <command> [args]");
            Console.WriteLine("Use '<plugin> help' to get a list of available commands for a plugin.");
            Console.WriteLine("Use '<plugin> <command> help' for help on a specific command.");
        }
    }
}

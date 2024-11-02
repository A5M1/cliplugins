using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PluginSystem {
    public class PluginLoader {
        private readonly string _pluginsDirectory;

        public PluginLoader(string pluginsDirectory) {
            _pluginsDirectory = pluginsDirectory;
        }

        public IEnumerable<IPlugin> LoadPlugins() {
            if(!Directory.Exists(_pluginsDirectory))
                throw new DirectoryNotFoundException($"Plugins directory '{_pluginsDirectory}' not found.");

            foreach(var dllPath in Directory.EnumerateFiles(_pluginsDirectory, "*.dll")) {
                List<IPlugin> plugins = new List<IPlugin>();
                try {
                    var assembly = Assembly.LoadFrom(dllPath);
                    foreach(var type in assembly.GetTypes()) {
                        if(typeof(IPlugin).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface) {
                            var pluginInstance = Activator.CreateInstance(type) as IPlugin;
                            if(pluginInstance != null) {
                                plugins.Add(pluginInstance);
                            }
                        }
                    }
                } catch(Exception ex) {
                    Console.WriteLine($"Error loading plugin from '{dllPath}': {ex.Message}");
                }
                foreach(var plugin in plugins) {
                    yield return plugin;
                }
            }
        }

        public void ShowPluginHelp(IPlugin plugin) {
            Console.WriteLine($"Help for Plugin: {plugin.Name}");
            foreach(var command in plugin.GetCommands()) {
                Console.WriteLine($"Command: {command.CommandName}");
                Console.WriteLine($"Description: {command.Description}");
                Console.WriteLine("Parameters:");
                foreach(var param in command.Parameters) {
                    Console.WriteLine($"  {param.Name} ({param.PType.TypeName})");
                    if(param.PType.IsEnum) {
                        Console.WriteLine("  Possible Values:");
                        foreach(var value in param.PType.EnumValues)
                            Console.WriteLine($"    {value} ({(int)value})");
                    }
                }
            }
        }
    }
}

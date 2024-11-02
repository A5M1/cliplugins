using System.Collections.Generic;

namespace PluginSystem {
    public interface IPlugin {
        string Name {
            get;
        }
        PluginResult ExecuteCommand(string commandName, string[] args);
        IEnumerable<PluginCommandInfo> GetCommands();
    }
}

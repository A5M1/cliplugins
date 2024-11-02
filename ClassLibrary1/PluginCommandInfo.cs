using System.Collections.Generic;

namespace PluginSystem {
    public struct PluginCommandInfo {
        public string CommandName {
            get; set;
        }
        public string Description {
            get; set;
        }
        public IEnumerable<Parameter> Parameters {
            get; set;
        }
    }
}

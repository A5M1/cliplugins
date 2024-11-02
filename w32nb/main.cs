using System.Runtime.InteropServices;
using PluginSystem;
using System.Collections.Generic;

namespace w32nb {
    public class MessageBoxPlugin : IPlugin {
        public string Name => "w32";

        public PluginResult ExecuteCommand(string commandName, string[] args) {
            if(commandName.Equals("show", StringComparison.CurrentCultureIgnoreCase)) {
                string message = args.Length > 0 ? args[0] : "Default Message";
                string title = args.Length > 1 ? args[1] : "Default Title";
                int style = args.Length > 2 ? int.Parse(args[2]) : (int)MessageBoxOptions.MB_OK;

                int result = ShowMessageBox(message, title, style);
                return new PluginResult {
                    Success = true,
                    Silent = true,
                    Message = $"MessageBox returned: {result}"
                };
            }
            return new PluginResult { 
                Success = false, 
                Message = $"Unknown command: {commandName}" 
            };
        }

        public IEnumerable<PluginCommandInfo> GetCommands() {
            return new List<PluginCommandInfo> {
                new() {
                    CommandName = "show",
                    Description = "Displays a message box with specified message, title, and style.",
                    Parameters = [  
                        new Parameter { 
                            Name = "message", 
                            PType = new PType(typeof(string)) 
                        },
                        new Parameter { 
                            Name = "title", 
                            PType = new PType(typeof(string)) 
                        },
                        new Parameter { 
                            Name = "style", 
                            PType = new PType(typeof(int)) 
                        }
                    ]
                }
            };
        }

        private static int ShowMessageBox(string text, string caption, int options) {
            return MessageBox(nint.Zero, text, caption, options);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(nint hWnd, string text, string caption, int type);

        public enum MessageBoxOptions {
            MB_OK = 0x00000000,
            MB_OKCANCEL = 0x00000001,
            MB_YESNO = 0x00000004,
            MB_ICONEXCLAMATION = 0x00000030,
            MB_ICONINFORMATION = 0x00000040,
            MB_ICONQUESTION = 0x00000020,
            MB_ICONSTOP = 0x00000010
        }
    }
}

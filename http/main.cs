using System.Text;
using System.Runtime.InteropServices;
using PluginSystem;

namespace http {
    public class HttpPlugin : IPlugin {
        public string Name => "http";

        private enum HTTPMethod {
            GET, POST, PATCH
        }
        private enum AddressFamily {
            AF_INET = 2
        }

        public PluginResult ExecuteCommand(string commandName, string[] args) {
            int port = args.Length > 1 ? int.Parse(args[1]) : 80;
            string? body = args.Length > 2 ? args[2] : null;

            if(Enum.TryParse(commandName.ToUpper(), out HTTPMethod method))
                return ExecuteHttpRequest(method, args[0], port, body);

            return new PluginResult { Success = false, Message = $"Unknown command: {commandName}" };
        }

        public IEnumerable<PluginCommandInfo> GetCommands() {
            return
            [
                new PluginCommandInfo {
                    CommandName = "get",
                    Description = "Performs an HTTP GET request.",
                    Parameters = [
                        new Parameter {
                            Name = "url",
                            PType = new PType(typeof(string)) },
                        new Parameter {
                            Name = "port",
                            PType = new PType(typeof(int))
                        }
                    ]
                },
                new PluginCommandInfo {
                    CommandName = "post",
                    Description = "Performs an HTTP POST request.",
                    Parameters = [
                        new Parameter {
                            Name = "url",
                            PType = new PType(typeof(string)) },
                        new Parameter {
                            Name = "port",
                            PType = new PType(typeof(int)) },
                        new Parameter {
                            Name = "body",
                            PType = new PType(typeof(string))
                        }
                    ]
                },
                new PluginCommandInfo {
                    CommandName = "patch",
                    Description = "Performs an HTTP PATCH request.",
                    Parameters = [
                        new Parameter {
                            Name = "url",
                            PType = new PType(typeof(string)) },
                        new Parameter {
                            Name = "port",
                            PType = new PType(typeof(int)) },
                        new Parameter {
                            Name = "body",
                            PType = new PType(typeof(string))
                        }
                    ]
                }
            ];
        }

        private PluginResult ExecuteHttpRequest(HTTPMethod method, string url,
            int port = 80, string? body = null) {

            ArgumentNullException.ThrowIfNull(body);
            string host = ExtractHostFromUrl(url);

            if(InitializeWinsock() != 0)
                return new PluginResult {
                    Success = false, Message = "Failed to initialize Winsock."
                };

            nint sock = socket((int)AddressFamily.AF_INET, 1, 0);
            if(sock == nint.Zero)
                return new PluginResult { Success = false, Message = "Failed to create socket." };

            var address = ResolveHost(host);
            if(address == 0)
                return new PluginResult { Success = false, Message = $"Could not resolve host: {host}" };

            var serverAddr =
                new Sockaddrin {
                    sin_family = (short)AddressFamily.AF_INET,
                    sin_port = htons((ushort)port), sin_addr = address
                };

            if(connect(sock, ref serverAddr, Marshal.SizeOf(serverAddr)) != 0)
                return new PluginResult { Success = false, Message = "Failed to connect to server." };

            string request = $"{method} {url} HTTP/1.1\r\n" +
                $"Host: {host}\r\nConnection: close\r\n";
            if(!string.IsNullOrEmpty(body))
                request += $"Content-Length: {Encoding.ASCII.GetBytes(body).Length}\r\n\r\n" + body;
            request += "\r\n";

            byte[] requestBytes = Encoding.ASCII.GetBytes(request);
            send(sock, requestBytes, requestBytes.Length, 0);

            byte[] buffer = new byte[4096];
            int receivedBytes = recv(sock, buffer, buffer.Length, 0);

            closesocket(sock);
            CleanupWinsock();

            string response = Encoding.ASCII.GetString(buffer, 0, receivedBytes);
            return new PluginResult { Success = true, Message = response };
        }

        private static string ExtractHostFromUrl(string url) => new Uri(url).Host;

        private static int ResolveHost(string host) {
            nint hostEntryPtr = gethostbyname(host);
            if(hostEntryPtr == nint.Zero) return 0;

            var hostEntry = (Hostent)Marshal.PtrToStructure(hostEntryPtr, typeof(Hostent));
            nint addressPtr = Marshal.ReadIntPtr(hostEntry.h_addr_list);
            byte[] addressBytes = new byte[4];
            Marshal.Copy(addressPtr, addressBytes, 0, 4);
            return BitConverter.ToInt32(addressBytes, 0);
        }

        [DllImport("ws2_32.dll")]
        private static extern int WSAStartup(ushort wVersionRequested, out WsaData lpWSAData);

        [DllImport("ws2_32.dll")]
        private static extern int WSACleanup();

        [DllImport("ws2_32.dll")]
        private static extern nint socket(int af, int type, int protocol);

        [DllImport("ws2_32.dll")]
        private static extern int connect(nint s, ref Sockaddrin name, int namelen);

        [DllImport("ws2_32.dll")]
        private static extern int send(nint s, byte[] buf, int len, int flags);

        [DllImport("ws2_32.dll")]
        private static extern int recv(nint s, byte[] buf, int len, int flags);

        [DllImport("ws2_32.dll")]
        private static extern int closesocket(nint s);

        [DllImport("ws2_32.dll")]
        private static extern nint gethostbyname(string name);

        [DllImport("ws2_32.dll")]
        private static extern ushort htons(ushort hostshort);

        private static int InitializeWinsock() => WSAStartup(0x0202, out _);

        private static void CleanupWinsock() => WSACleanup();

        [StructLayout(LayoutKind.Sequential)]
        private struct Sockaddrin {
            public short sin_family;
            public ushort sin_port;
            public int sin_addr;
            public long sin_zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WsaData {
            public ushort wVersion;
            public ushort wHighVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
            public string szDescription;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
            public string szSystemStatus;
            public ushort iMaxSockets;
            public ushort iMaxUdpDg;
            public nint lpVendorInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Hostent {
            public nint h_name;
            public nint h_aliases;
            public short h_addrtype;
            public short h_length;
            public nint h_addr_list;
        }
    }
}

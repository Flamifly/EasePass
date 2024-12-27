using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace EasePass.Helper
{
    public static class RDPHelper
    {
        public static bool OpenRDPClient(string ip, string port, string username, string password, bool publicUse = true, bool multiMonitor = true, bool fullScreen = true, int width = 0, int height = 0)
        {
            Process Process = new Process()
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                }
            };
            if (!Process.Start())
                return false;

            using (StreamWriter streamWriter = Process.StandardInput)
            {
                if (streamWriter.BaseStream.CanWrite)
                {
                    streamWriter.WriteLine($"cmdkey /generic:\"{ip}\" /user:\"{username}\" /pass:\"{password}\"");
                    streamWriter.WriteLine($"mstsc " + GetArguments(ip, port, publicUse, multiMonitor, fullScreen, width, height));
                    streamWriter.WriteLine($"cmdkey /delete:TERMSRV/{ip}");
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static string GetArguments(string ip, string port, bool publicUse, bool multiMonitor, bool fullScreen = true, int width = 0, int height = 0)
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrWhiteSpace(ip))
                return string.Empty;

            sb.Append("/v:" + ip);
            
            if (!string.IsNullOrWhiteSpace(port))
            {
                sb.Append(":" + port);
            }

            if (publicUse)
            {
                sb.Append(" /public");
            }

            if (multiMonitor)
            {
                sb.Append(" /multimon");
            }

            if (fullScreen)
            {
                sb.Append(" /f");
            }
            else
            {
                sb.Append("/w:" + width);
                sb.Append("/h:" + height);
            }
            return sb.ToString();
        }
    }
}
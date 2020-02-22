using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using static System.Environment;

namespace Shared
{
    public static class StatReader
    {
        public static string ReadStat(string address, ushort port)
        {
            byte[] rawServerData = new byte[512];
            bool serverUp = false;
            string version = "Unknown";
            string motd = "";
            long currentPlayers = 0;
            long maximumPlayers = 0;
            long latency = long.MaxValue;
            bool hasFailed = false;
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                TcpClient tcpClient = new TcpClient {ReceiveTimeout = 5000};
                stopWatch.Start();
                tcpClient.Connect(address, port);
                stopWatch.Stop();
                latency = stopWatch.ElapsedMilliseconds;
                NetworkStream stream = tcpClient.GetStream();
                byte[] payload = {0xFE, 0x01};
                stream.Write(payload, 0, payload.Length);
                stream.Read(rawServerData, 0, 512);
                tcpClient.Close();
            }
            catch (Exception)
            {
                hasFailed = true;
            }
            if (!hasFailed)
                if (rawServerData.Length != 0)
                {
                    string[] serverData =
                        Encoding.Unicode.GetString(rawServerData).Split("\u0000\u0000\u0000".ToCharArray());
                    if (serverData.Length >= 6)
                    {
                        serverUp = true;
                        version = serverData[2];
                        motd = serverData[3];
                        currentPlayers = long.Parse(serverData[4]);
                        maximumPlayers = long.Parse(serverData[5]);
                    }
                }
            string output = $"{address}:{port} is {(serverUp ? $"up. (Latency: {latency})" : "down")}";
            if (!serverUp) return output;
            output += $"{NewLine}{version} Server{NewLine}{currentPlayers}/{maximumPlayers} players";
            output +=
                $"({(maximumPlayers == currentPlayers ? "full" : $"{maximumPlayers - currentPlayers} slots free")})";
            output += $"{NewLine}\"{motd}\"";
            return output;
        }
    }
}
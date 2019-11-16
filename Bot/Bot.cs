using System.Diagnostics;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Bot
{
    public class Bot
    {
        public DiscordClient Client { get; }
        public Bot(DiscordConfiguration cfg)
        {
            Client = new DiscordClient(cfg);
            Client.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;
        }
        public Task StartAsync() => Client.ConnectAsync();
        public Task StopAsync() => Client.DisconnectAsync();
        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e) => Debug.WriteLine($"[{e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")}] [{e.Application}] [{e.Level}] {e.Message}");
    }
}

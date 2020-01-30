using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using Newtonsoft.Json;

namespace Moozy
{
    public class Program
    {
        public DiscordClient Client { get; set; }

        public static void Main(string[] args)
        {
            if (!File.Exists("config.json"))
            {
                Console.Write("Token: ");
                File.WriteAllText("config.json",
                    $"{{\r\n  \"token\": \"{Console.ReadLine()}\",\r\n  \"prefix\": \";;\"\r\n}}");
            }

            Program prog = new Program();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }

        public async Task RunBotAsync()
        {
            string json = "";
            using (FileStream fs = File.OpenRead("config.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                json = await sr.ReadToEndAsync();
            }

            ConfigJson cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
            DiscordConfiguration cfg = new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };
            Client = new DiscordClient(cfg);
            Client.UseVoiceNext();
            Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {cfgjson.CommandPrefix},
                EnableDms = false
            }).RegisterCommands<Commands>();
            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", "Client is ready to process events.",
                DateTime.Now);
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "DiscHax",
                $"Your invite Link: https://discordapp.com/oauth2/authorize?client_id={e.Client.CurrentApplication.Id}&scope=bot&permissions=8",
                DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", $"Guild available: {e.Guild.Name}",
                DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot",
                $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }
    }
}
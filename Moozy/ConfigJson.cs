using Newtonsoft.Json;

namespace Moozy
{
    // this structure will hold data from config.json
    public struct ConfigJson
    {
        [JsonProperty("token")] public string Token { get; private set; }

        [JsonProperty("prefix")] public string CommandPrefix { get; private set; }
    }
}
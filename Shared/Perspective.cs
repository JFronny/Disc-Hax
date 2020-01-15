using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shared
{
    public class Perspective
    {
        private readonly HttpClient _httpclient;
        private readonly string _token;

        public Perspective(string token)
        {
            _token = token;
            _httpclient = new HttpClient();
        }

        public async Task<PerspectiveAnalysisResponse> RequestAnalysis(string message)
        {
            PerspectiveAnalysisRequest requestPayload = new PerspectiveAnalysisRequest
            {
                Comment = new PerspectiveComment
                {
                    Text = message
                }
            };
            StringContent content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8,
                "application/json");
            HttpResponseMessage response = await _httpclient.PostAsync(
                "https://" + $"commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key={_token}", content);
            return JsonConvert.DeserializeObject<PerspectiveAnalysisResponse>(
                await response.Content.ReadAsStringAsync());
        }
    }

    public class PerspectiveAnalysisRequest
    {
        [JsonProperty("comment")] public PerspectiveComment Comment = new PerspectiveComment();

        [JsonProperty("context")] public PerspectiveContext Context = new PerspectiveContext();

        [JsonProperty("doNotStore")] public bool DoNotStore = true;

        [JsonProperty("languages")] public List<string> Languages = new List<string> {"en"};

        [JsonProperty("requestedAttributes")] public Dictionary<string, PerspectiveAttributes> RequestedAttributes =
            new Dictionary<string, PerspectiveAttributes>
            {
                {"TOXICITY", new PerspectiveAttributes {ScoreTreshold = null, ScoreType = null}}
            };

        [JsonProperty("sessionId")] public string SessionId = "";

        [JsonProperty("clientToken")] public string Token = "";
    }

    public class PerspectiveComment
    {
        [JsonProperty("text")] public string Text = "";

        [JsonProperty("type")] public string Type = "PLAIN_TEXT";
    }

    public class PerspectiveContext
    {
        [JsonProperty("entries")] public List<PerspectiveContextEntry> Entries = new List<PerspectiveContextEntry>();
    }

    public class PerspectiveContextEntry
    {
        [JsonProperty("text")] public string Text = "";

        [JsonProperty("type")] public string Type = "PLAIN_TEXT";
    }

    public class PerspectiveAttributes
    {
        [JsonProperty("scoreTreshold", NullValueHandling = NullValueHandling.Ignore)]
        public float? ScoreTreshold = 0.0f;

        [JsonProperty("scoreType", NullValueHandling = NullValueHandling.Ignore)]
        public string ScoreType = "PROBABILITY";
    }

    public class PerspectiveAnalysisResponse
    {
        [JsonProperty("attributeScores")]
        public Dictionary<string, AttributeScore> AttributeScores = new Dictionary<string, AttributeScore>();
    }

    public class AttributeScore
    {
        [JsonProperty("spanScores")] private List<SpanScore> _spanScores = new List<SpanScore>();

        [JsonProperty("summaryScore")] public Score SummaryScore = new Score();
    }

    public class SpanScore
    {
        [JsonProperty("begin")] public int Begin;

        [JsonProperty("end")] public int End;

        [JsonProperty("score")] public Score Score;
    }

    public class Score
    {
        [JsonProperty("type")] public string Type = "";

        [JsonProperty("value")] public float Value;
    }
}
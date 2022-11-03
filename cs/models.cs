using Newtonsoft.Json;

namespace MVCConAzure.cs
{
    public class user
    {
        [JsonProperty(PropertyName = "id")]
        public string? id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; } = "";
        [JsonProperty(PropertyName = "lastname")]
        public string lastname { get; set; } = "";
        [JsonProperty(PropertyName = "age")]
        public int age { get; set; } = 0;
        [JsonProperty(PropertyName = "phone")]
        public string phone { get; set; } = "";
    }
}

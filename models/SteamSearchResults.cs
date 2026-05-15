using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SteamReqDesktopWPF.models {
    public class  SteamSearchResponse {
        [JsonPropertyName("items")]
        public List<SteamSearchResults> Items { get; set; } = new List<SteamSearchResults>();
    }
    public class SteamSearchResults {
        [JsonPropertyName("id")]
        public int AppID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("tiny_image")]
        public string ImageURL { get; set; } = "";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AppdateChecker
{
    public class JsonGithubRelease
    {
        public List<JsonGithubReleases> Releases { get; set; }
    }
    public class JsonGithubReleases
    {
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("tag_name")]
        public string TagName { get; set; } = "";
    }
}

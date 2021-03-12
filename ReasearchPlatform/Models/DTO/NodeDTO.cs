using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchPlatform.Models.DTO
{
    public class NodeDTO
    {
        public long place_id { get; set; }

        public string licence { get; set; }

        public List<string> boundingbox { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string display_name { get; set; }

        [JsonProperty("class")]
        public string _class { get; set; }

        public string type { get; set; }

        public float importance { get; set; }
    }
}

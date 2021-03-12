using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchPlatform.Models.DTO
{

    public class OverpassApiResult
    {
        public float version { get; set; }
        public string generator { get; set; }
        public Osm3s osm3s { get; set; }
        public List<Element> elements { get; set; }
    }

    public class Osm3s
    {
        public DateTime timestamp_osm_base { get; set; }
        public string copyright { get; set; }
    }

    public class Element
    {
        public string type { get; set; }
        public long id { get; set; }
        public float lat { get; set; }
        public float lon { get; set; }
        public Tags tags { get; set; }
    }

    public class Tags
    {
        public string name { get; set; }
        public string nameru { get; set; }
        public string old_namede { get; set; }
        public string place { get; set; }
        public string population { get; set; }
        public string postal_code { get; set; }
        public string sourcepopulation { get; set; }
        public string terytsimc { get; set; }
        public string wikidata { get; set; }
        public string wikipedia { get; set; }
    }

}

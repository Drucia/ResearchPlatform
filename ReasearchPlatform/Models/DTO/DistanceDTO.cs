using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchPlatform.Models.DTO
{

    public class DistanceDTO
    {
        public List<List<int>> distances { get; set; }
        public List<List<int>> times { get; set; }
        public List<List<double>> weights { get; set; }
        public Info info { get; set; }
    }

    public class Info
    {
        public string[] copyrights { get; set; }
    }

}

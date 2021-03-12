using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchPlatform.Models
{
    public class Input
    {
        public ObservableCollection<Job> Jobs { get; set; }
        public Node Base { get; set; }
        public List<Client> Clients { get; set; }
        public List<Distance> DistanceMatrix { get; set; }
        public List<Node> Nodes { get; set; }
    }
}

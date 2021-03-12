using System;
using System.Collections.Generic;

namespace ResearchPlatform.Models
{
    public class Job
    {
        public int ID { get; set; }
        public Node From { get; set; }
        public Node To { get; set; }
        public double Price { get; set; }
        public Tuple<int, int> Pickup { get; set; }
        public Tuple<int, int> Delivery { get; set; }
        public int LoadingTime { get; set; }
        public int ClientId { get; set; }
    }
}

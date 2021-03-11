using System;
using System.Collections.Generic;

namespace ResearchPlatform.Models
{
    public class Job
    {
        public string ID { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public double Price { get; set; }
        //public Tuple<int, int> Pickup { get; set; }
        //public Tuple<int, int> Delivery { get; set; }
        public int Pickup { get; set; }
        public int Delivery { get; set; }
        public string ClientId { get; set; }

        public static List<Job> GetSimpleList()
        {
            return new List<Job> { 
                //new Job(){ ID = "1", From="A", To="B", Price=250, Pickup = Tuple.Create(10, 40), Delivery = Tuple.Create(90, 120), Client = "1" }
                new Job(){ ID = "1", From="A", To="B", Price=250, Pickup = 10, Delivery = 90, ClientId = "1" }
            };
        }
    }
}

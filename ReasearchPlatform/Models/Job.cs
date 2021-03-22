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
        public int TypeOfLoading { get; set; }
        public int SeizureRisk { get; set; }
        public double ClientOpinion { get; set; }

        public Job()
        {
        }

        public Job(Job other)
        {
            ID = other.ID;
            From = other.From;
            To = other.To;
            Price = other.Price;
            Pickup = other.Pickup;
            Delivery = other.Delivery;
            LoadingTime = other.LoadingTime;
            ClientId = other.ClientId;
            TypeOfLoading = other.TypeOfLoading;
            SeizureRisk = other.SeizureRisk;
            ClientOpinion = other.ClientOpinion;
        }
    }
}

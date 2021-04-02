using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchPlatform.Models
{
    public class JobToProceed : Job
    {
        public double Utility { get; set; }
        public double Profit { get; internal set; }
        public double ComfortOfWork { get; internal set; }
        public double TimeOfExecution { get; internal set; }
        public double Reliability { get; internal set; }
        public int PossibilityOfNextJobs { get; internal set; }

        public JobToProceed()
        {}
        public JobToProceed(Job job) : base(job)
        {
            Utility = 0.0;
        }

        public override bool Equals(object obj)
        {
            return obj is JobToProceed proceed &&
                   ID == proceed.ID &&
                   EqualityComparer<Node>.Default.Equals(From, proceed.From) &&
                   EqualityComparer<Node>.Default.Equals(To, proceed.To) &&
                   Price == proceed.Price &&
                   EqualityComparer<Tuple<int, int>>.Default.Equals(Pickup, proceed.Pickup) &&
                   EqualityComparer<Tuple<int, int>>.Default.Equals(Delivery, proceed.Delivery) &&
                   LoadingTime == proceed.LoadingTime &&
                   ClientId == proceed.ClientId &&
                   TypeOfLoading == proceed.TypeOfLoading &&
                   SeizureRisk == proceed.SeizureRisk &&
                   ClientOpinion == proceed.ClientOpinion &&
                   Utility == proceed.Utility &&
                   Profit == proceed.Profit &&
                   ComfortOfWork == proceed.ComfortOfWork &&
                   TimeOfExecution == proceed.TimeOfExecution &&
                   Reliability == proceed.Reliability &&
                   PossibilityOfNextJobs == proceed.PossibilityOfNextJobs;
        }
    }
}

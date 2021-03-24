using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchPlatform.Models
{
    public class JobToProceed : Job
    {
        public bool IsChosen { get; set; }
        public double Utility { get; set; }
        public double Profit { get; internal set; }
        public double ComfortOfWork { get; internal set; }
        public double TimeOfExecution { get; internal set; }
        public double Reliability { get; internal set; }
        public int PossibilityOfNextJobs { get; internal set; }

        public JobToProceed(Job job) : base(job)
        {
            IsChosen = false;
            Utility = 0.0;
        }
    }
}

using System.Collections.Generic;

namespace ResearchPlatform.Models
{
    public class Result
    {
        public List<JobToProceed> Jobs { get; set; }
        public List<Break> Breaks { get; set; }
        public long Duration { get; set; }
        public int VisitedNodes { get; set; }
        public int AmountOfJobs { get; set; }
    }
}
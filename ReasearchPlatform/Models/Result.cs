using System;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform.Models
{
    public class Result
    {
        public List<JobToProceed> Jobs { get; set; }
        public List<Break> Breaks { get; set; }
        public long Duration { get; set; }
        public int VisitedNodes { get; set; }
        public int AmountOfJobs { get; set; }
        public long CriteriaDuration { get; set; }
        public double Value { get; set; }
        public int DrivenTime { get; set; }
        public List<double> Factors { get; set; }

        public Result(Result result)
        {
            Jobs = result.Jobs;
            Breaks = result.Breaks;
            Duration = result.Duration;
            VisitedNodes = result.VisitedNodes;
            AmountOfJobs = result.AmountOfJobs;
            CriteriaDuration = result.CriteriaDuration;
            Value = result.Value;
            DrivenTime = result.DrivenTime;
            Factors = result.Factors;
        }

        public Result()
        {
        }
    }

    public class FileResult : Result
    {
        public string InputFileName { get; set; }
        public string AlgorithmName { get; set; }

        public FileResult(Result result, string filename, string algorithmName) : base(result)
        {
            InputFileName = filename;
            AlgorithmName = algorithmName;
        }
    }
}
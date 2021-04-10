using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResearchPlatform.Algorithms
{
    public class ELECTREEBuilder : ICriteriaAlgorithmBuilder
    {
        private List<double> _weights;
        private List<JobToProceed> _jobs;

        public ELECTREEBuilder(List<double> weights, List<JobToProceed> jobsToProceed)
        {
            _weights = weights;
            _jobs = jobsToProceed;
        }

        public List<JobToProceed> GetJobsWithCalculatedUtility()
        {
            throw new NotImplementedException();
        }

        public List<double> GetWeights()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}

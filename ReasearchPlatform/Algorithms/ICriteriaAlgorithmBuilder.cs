using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchPlatform.Algorithms
{
    public interface ICriteriaAlgorithmBuilder
    {
        public void Run();
        public List<JobToProceed> GetJobsWithCalculatedUtility();
    }
}

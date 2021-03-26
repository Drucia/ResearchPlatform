using ResearchPlatform.Models;
using System.Collections.Generic;

namespace ResearchPlatform.Helpers
{
    public interface IBranchAndBoundHelper
    {
        public bool AreAllConstrintsSatisfied(JobToProceed currentJob, List<byte> x, List<byte> d, List<byte> z);
        public double CalculateValueOfGoalFunction(List<byte> x, List<byte> d, List<byte> z);
        bool AreAllConstraintsSatisfied(JobToProceed currentJob, List<JobToProceed> done, double workTime, double drivenTime);
    }
}
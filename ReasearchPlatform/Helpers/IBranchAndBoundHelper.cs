using ResearchPlatform.Models;
using System.Collections.Generic;

namespace ResearchPlatform.Helpers
{
    public interface IBranchAndBoundHelper
    {
        public static readonly int MAX_TIME_WITH_DRIVING = 270;
        public static readonly int MAX_TIME_WITH_WORKING = 780;
        public static readonly int MAX_TIME_WITH_WHOLE_DRIVING = 540;
        public static readonly int BREAK_TIME = 45;
        public double CalculateValueOfGoalFunction(List<JobToProceed> done, double workTime);
        public bool AreAllConstraintsSatisfied(Node currNode, JobToProceed currentJob, List<JobToProceed> done, 
            int workTime, int drivenTime, int wholeDrivenTime);
        public double GetMaxPossibleValue(List<JobToProceed> done, List<JobToProceed> restPossibleJobs, double currentValue, double workTime);
        public List<double> CalculateFactorsOfGoalFunction(List<JobToProceed> done, double workTime);
    }
}
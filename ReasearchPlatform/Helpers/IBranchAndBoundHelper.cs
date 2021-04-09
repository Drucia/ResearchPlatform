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
        public double CalculateValueOfGoalFunction(List<JobToProceed> done);
        public bool AreAllConstraintsSatisfied(Node currNode, JobToProceed currentJob, List<JobToProceed> done, 
            int workTime, int drivenTime, int wholeDrivenTime);
    }
}
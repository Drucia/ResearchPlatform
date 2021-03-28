using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchPlatform.Helpers
{
    public class BranchAndBoundHelper : IBranchAndBoundHelper
    {
        private DistancesManager _manager;

        public BranchAndBoundHelper(DistancesManager distancesManager)
        {
            _manager = distancesManager;
        }

        public bool AreAllConstraintsSatisfied(Node currNode, JobToProceed currentJob, List<JobToProceed> done, 
            int workTime, int drivenTime, int wholeDrivenTime)
        {
            return 
                wholeDrivenTime <= IBranchAndBoundHelper.MAX_TIME_WITH_WHOLE_DRIVING &&
                drivenTime <= IBranchAndBoundHelper.MAX_TIME_WITH_DRIVING &&
                workTime <= IBranchAndBoundHelper.MAX_TIME_WITH_WORKING &&
                CheckIfExecuteJobIsPossible(currNode, currentJob, workTime, drivenTime, wholeDrivenTime);
        }

        private bool CheckIfExecuteJobIsPossible(Node currNode, JobToProceed currentJob, 
            int workTime, int drivenTime, int wholeDrivenTime)
        {
            // simulate execute of job
            var timeToStart = _manager.GetDistanceBetween(currNode, currentJob.From).DurationInSeconds / 60;
            var timeFromStartToEnd = _manager.GetDistanceBetween(currentJob.From, currentJob.To).DurationInSeconds / 60;

            var breakTime = drivenTime + timeToStart >= IBranchAndBoundHelper.MAX_TIME_WITH_DRIVING
                ? IBranchAndBoundHelper.BREAK_TIME : 0;

            // if break then zero driven time
            drivenTime = breakTime > 0 ? 0 : drivenTime;

            // add break time if necessary in commuting node
            // add travel time from last node to start node
            drivenTime += timeToStart;
            wholeDrivenTime += timeToStart;
            workTime = Math.Max(workTime + breakTime + timeToStart, currentJob.Pickup.Item1);

            if (workTime > currentJob.Pickup.Item2)
                return false;

            // add loading time
            workTime += currentJob.LoadingTime;

            breakTime = drivenTime + timeFromStartToEnd >= IBranchAndBoundHelper.MAX_TIME_WITH_DRIVING
                ? IBranchAndBoundHelper.BREAK_TIME : 0;

            // if break then zero driven time
            drivenTime = breakTime > 0 ? 0 : drivenTime;

            // add break time if necessary in start node
            // add travel time from start to end node
            drivenTime += timeFromStartToEnd;
            wholeDrivenTime += timeFromStartToEnd;
            workTime = Math.Max(workTime + breakTime + timeFromStartToEnd, currentJob.Delivery.Item1);

            if (workTime > currentJob.Delivery.Item2)
                return false;

            // add loading time
            workTime += currentJob.LoadingTime;

            if (workTime > IBranchAndBoundHelper.MAX_TIME_WITH_WORKING || wholeDrivenTime > IBranchAndBoundHelper.MAX_TIME_WITH_WHOLE_DRIVING)
                return false;

            return true;
        }

        public bool AreAllConstrintsSatisfied(List<byte> x, List<byte> d, List<byte> z)
        {
            throw new NotImplementedException();
        }

        public double CalculateValueOfGoalFunction(List<byte> x, List<byte> d, List<byte> z)
        {
            throw new NotImplementedException();
        }
    }
}

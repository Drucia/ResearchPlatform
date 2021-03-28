using ResearchPlatform.Helpers;
using ResearchPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchPlatform.Algorithms
{
    public class BranchAndBound
    {
        // input
        private List<Node> _allNodes;
        private Node _base;
        private DistancesManager _distancesManager;
        private IBranchAndBoundHelper _helper;
        private List<JobToProceed> _jobs;

        // processing
        private List<JobToProceed> _jobsToProceed;
        private List<byte> _chosenEdges;
        private List<byte> _chosenBreaks;

        public BranchAndBound(Node startNode, List<Node> allNodes, List<Distance> distances,
            List<JobToProceed> jobs, IBranchAndBoundHelper helper)
        {
            _helper = helper;
            _allNodes = allNodes;
            _base = startNode;
            _distancesManager = new DistancesManager(distances);
            _jobs = jobs;
            _jobsToProceed = new List<JobToProceed>(jobs);
        }

        public List<JobToProceed> Run(SearchTreeAlgorithm searchTreeAlgorithm)
        {
            switch(searchTreeAlgorithm)
            {
                case SearchTreeAlgorithm.DFS: return RunWithDFS();
            }

            return new List<JobToProceed>();
        }

        public List<JobToProceed> RunWithDFS()
        {
            // sort jobs
            _jobsToProceed.Sort((left, right) => (int)((left.Utility - right.Utility) * 100));

            var currentNode = _base;
            
            return _jobs.Where(j => j.IsChosen).ToList();
        }

        public void DFSRec(Node currNode, JobToProceed currentJob, List<JobToProceed> done, 
            List<JobToProceed> all, List<List<JobToProceed>> allCheckedJobsPath, int workTime, int drivenTime, int wholeDrivenTime)
        {
            if (_helper.AreAllConstraintsSatisfied(currNode, currentJob, done, workTime, drivenTime, wholeDrivenTime))
            {
                int wT, dT, wholeDT;
                done.Add(currentJob);
                currNode = ExecuteJob(done, out wT, out dT, out wholeDT);

                var allPossible = GetPossibleJobsToDo(all, currNode, wT);
                foreach (var job in allPossible)
                {
                    DFSRec(currNode, job, done, all, allCheckedJobsPath, wT, dT, wholeDT);
                }

                // leaf
                if (allPossible.Count == 0)
                    allCheckedJobsPath.Add(new List<JobToProceed>(done));
            }
            else 
            {
                // cut tree
                allCheckedJobsPath.Add(new List<JobToProceed>(done));
            }

            done.RemoveAt(done.Count - 1);
        }

        private Node ExecuteJob(List<JobToProceed> done, out int wT, out int dT, out int wholeDT)
        {
            Tuple<List<int>, Node /* last node */> times =
                done.Aggregate(Tuple.Create(new List<int>(Enumerable.Repeat(0, 3)), done.First().To), (acc, job) => {

                    var curWorkTime = acc.Item1[0];
                    var curDrivenTime = acc.Item1[1];
                    var wholeDrivenTime = acc.Item1[2];

                    var lastNode = acc.Item2;

                    var timeToStart = GetTimeToGo(lastNode, job.From);
                    var timeFromStartToEnd = GetTimeToGo(job.From, job.To);

                    var breakTime = curDrivenTime + timeToStart >= IBranchAndBoundHelper.MAX_TIME_WITH_DRIVING 
                        ? IBranchAndBoundHelper.BREAK_TIME : 0;

                    // if break then zero driven time
                    curDrivenTime = breakTime > 0 ? 0 : curDrivenTime;

                    // add break time if necessary in commuting node
                    // add travel time from last node to start node
                    curDrivenTime += timeToStart;
                    wholeDrivenTime += timeToStart;
                    curWorkTime = Math.Max(curWorkTime + breakTime + timeToStart, job.Pickup.Item1);

                    // add loading time
                    curWorkTime += job.LoadingTime;

                    breakTime = curDrivenTime + timeFromStartToEnd >= IBranchAndBoundHelper.MAX_TIME_WITH_DRIVING 
                        ? IBranchAndBoundHelper.BREAK_TIME : 0;

                    // if break then zero driven time
                    curDrivenTime = breakTime > 0 ? 0 : curDrivenTime;

                    // add break time if necessary in start node
                    // add travel time from start to end node
                    curDrivenTime += timeFromStartToEnd;
                    wholeDrivenTime += timeFromStartToEnd;
                    curWorkTime = Math.Max(curWorkTime + breakTime + timeFromStartToEnd, job.Delivery.Item1);

                    // add loading time
                    curWorkTime += job.LoadingTime;

                    return Tuple.Create(acc.Item1, job.To);
                });

            wT = times.Item1[0];
            dT = times.Item1[1];
            wholeDT = times.Item1[2];

            return times.Item2;
        }

        private List<JobToProceed> GetPossibleJobsToDo(List<JobToProceed> jobs, Node currentNode, int workTime)
        {
            return jobs
                .Where(j => !j.IsChosen && IsPossibleToGoToJob(j, currentNode, workTime)).ToList();
        }

        private bool IsPossibleToGoToJob(JobToProceed job, Node currentNode, int workTime)
        {
            return job.Pickup.Item2 > (workTime + GetTimeToGo(currentNode, job.From)); 
        }

        private int GetTimeToGo(Node currentNode, Node nextNode)
        {
            return _distancesManager.GetDistanceBetween(currentNode, nextNode).DurationInSeconds / 60;
        }
    }
}

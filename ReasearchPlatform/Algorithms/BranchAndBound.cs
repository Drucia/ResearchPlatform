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
        private static readonly int MAX_TIME_WITH_DRIVING = 270;

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
        private double _workTime = 0;
        private double _drivenTime = 0;

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
            List<JobToProceed> all, List<List<JobToProceed>> allCheckedJobsPath)
        {
            int wT, dT;

            if (_helper.AreAllConstraintsSatisfied(currentJob, done, _workTime, _drivenTime))
            {
                done.Add(currentJob);
                currNode = ExecuteJob(done, out wT, out dT);

                var allPossible = GetPossibleJobsToDo(all, currNode);
                foreach (var job in allPossible)
                {
                    DFSRec(currNode, job, done, all, allCheckedJobsPath);
                }

                // leaf
                if (allPossible.Count == 0)
                    allCheckedJobsPath.Add(new List<JobToProceed>(done));
            }
            else 
            {
                allCheckedJobsPath.Add(new List<JobToProceed>(done));
            }

            done.RemoveAt(done.Count - 1);
        }

        private Node ExecuteJob(List<JobToProceed> done, out int wT, out int dT)
        {
            KeyValuePair<Tuple<int /*work time */, int /* driven time */>, Node /* last node */> times = done.Aggregate(KeyValuePair.Create(Pair.Create(0, 0), done.First().To), (acc, job) => {
                acc.Item1.Item1 += GetTimeToGo(acc.Item2, job.From);

            });
        }

        private List<JobToProceed> GetPossibleJobsToDo(List<JobToProceed> jobs, Node currentNode)
        {
            return jobs
                .Where(j => !j.IsChosen && IsPossibleToGoToJob(j, currentNode)).ToList();
        }

        private bool IsPossibleToGoToJob(JobToProceed job, Node currentNode)
        {
            return job.Pickup.Item2 > (_workTime + GetTimeToGo(currentNode, job.From)); 
        }

        private int GetTimeToGo(Node currentNode, Node nextNode)
        {
            return _distancesManager.GetDistanceBetween(currentNode, nextNode).DurationInSeconds / 60;
        }
    }
}

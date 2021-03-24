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
        private List<Distance> _distances;
        private IBranchAndBoundHelper _helper;

        // processing
        private Node _currentNode;
        private List<JobWithChoose> _jobs;
        private List<byte> _chosenEdges;
        private List<byte> _chosenBreaks;
        private double _workTime = 0;
        private double _drivenTime = 0;

        private Tuple<Double, List<Job>> _currentBest;

        public BranchAndBound(Node startNode, List<Node> allNodes, List<Distance> distances,
            List<JobWithChoose> jobs, IBranchAndBoundHelper helper)
        {
            _helper = helper;
            _allNodes = allNodes;
            _base = startNode;
            _distances = distances;
            _jobs = jobs;
            _currentNode = _base;
        }

        public List<JobWithChoose> Run(SearchTreeAlgorithm searchTreeAlgorithm)
        {
            switch(searchTreeAlgorithm)
            {
                case SearchTreeAlgorithm.DFS: return RunWithDFS();
            }

            return new List<JobWithChoose>();
        }

        public List<JobWithChoose> RunWithDFS()
        {
            
            // sorting Job on basis of utility
            
            var jobToCurrentlyGet = GetRestOfJobs(_jobs);
            return _jobs.Where(j => j.IsChosen).ToList();
        }

        private List<JobWithChoose> GetRestOfJobs(List<JobWithChoose> jobs)
        {
            return jobs
                .Where(j => !j.IsChosen && IsPossibleToGoToJob(j)).ToList();
        }

        private bool IsPossibleToGoToJob(JobWithChoose job)
        {
            return job.Pickup.Item2 > (_workTime + GetTimeToGo(_currentNode, job.From)); 
        }

        private double GetTimeToGo(Node currentNode, Node nextNode)
        {
            return _distances.Find(distance => distance.From.Equals(currentNode) && distance.To.Equals(nextNode) ||
                distance.From.Equals(nextNode) && distance.To.Equals(currentNode)).DurationInSeconds / 60;
        }
    }
}

using ResearchPlatform.Algorithms;
using ResearchPlatform.Helpers;
using ResearchPlatform.Models;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform
{
    public static class AlgorithmsManager
    {
        public static bool CheckMatrixConsistency(IEnumerable<IEnumerable<string>> matrix)
        {
            var ahp = new AHPBuilder(matrix, null);
            ahp.Run();
            return ahp.IsConsistent;
        }

        public static List<Dictionary<SearchTreeAlgorithm, Result>> RunWith(Configuration configuration, Models.Input input)
        {
            var algorithmsMatrix = configuration.AlgorithmsMatrix;
            var distanceManager = new DistancesManager(input.DistanceMatrix);
            var branchAndBoundHelper = new BranchAndBoundHelper(distanceManager, configuration.GoalFunctionWeights, configuration);
            var jobsToProceed = input.Jobs.Select(j => new JobToProceed(j)).ToList();

            CriteriaCalculator.CalculateCriteria(
                jobsToProceed, 
                configuration, 
                distanceManager, 
                input.Base,
                input.Clients);

            // removes jobs with -1 id - no distances to do this job
            jobsToProceed = jobsToProceed.Where(job => job.ID != -1).ToList();

            // by default for promethee and electre weight from ahp
            var ahp = new AHPBuilder(configuration.ComparisionMatrix, null);
            ahp.Run();
            var ahpWeights = ahp.GetWeights();

            var algorithmsToRun = new List<Task>
            {
                new Task(
                    new AHPBuilder(configuration.ComparisionMatrix, jobsToProceed),
                    branchAndBoundHelper,
                    input,
                    new List<bool>(algorithmsMatrix[(int)MultiCriteriaAlgorithm.AHP]),
                    jobsToProceed,
                    distanceManager),

                new Task(
                    new PROMETHEEBuilder(ahpWeights, jobsToProceed),
                    branchAndBoundHelper,
                    input,
                    new List<bool>(algorithmsMatrix[(int)MultiCriteriaAlgorithm.PROMETHEE]),
                    jobsToProceed,
                    distanceManager),

                //new Task(
                //    new ELECTREEBuilder(ahpWeights, jobsToProceed),
                //    branchAndBoundHelper,
                //    input,
                //    new List<bool>(algorithmsMatrix[(int)MultiCriteriaAlgorithm.ELECTRE]),
                //    jobsToProceed,
                //    distanceManager),

                new Task(
                    new OwnWeightsBuilder(configuration.CriteriaWeights.Select(w => w / 100.0).ToList(), jobsToProceed),
                    branchAndBoundHelper,
                    input,
                    new List<bool>(algorithmsMatrix[(int)MultiCriteriaAlgorithm.OwnWeights]),
                    jobsToProceed,
                    distanceManager),
            };

            algorithmsToRun.ForEach(alg => alg.Run());

            return algorithmsToRun.Select(alg => alg.GetResults()).ToList();
        }
    }
}

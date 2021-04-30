using ResearchPlatform.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ResearchPlatform.Models
{
    public class Configuration
    {
        private static readonly int SUM_OF_WEIGHTS = 100;

        public ObservableCollection<ObservableCollection<string>> ComparisionMatrix { get; set; } = new ObservableCollection<ObservableCollection<string>> {
            new ObservableCollection<string>{ "1", "2", "1/6", "1/4", "1/2"},
            new ObservableCollection<string>{ "1/2", "1", "1/5", "1/3", "1/2"},
            new ObservableCollection<string>{ "6", "5", "1", "5", "5"},
            new ObservableCollection<string>{ "4", "3", "1/5", "1", "2" },
            new ObservableCollection<string>{ "2", "2", "1/5", "1/2", "1" }
        };

        public ObservableCollection<int> CriteriaWeights { get; set; } = new ObservableCollection<int> {
            5, // Criteria.ComfortOfWork 
            10, // Criteria.DrivingTime
            60, // Criteria.Profit
            15, // Criteria.CompletedJobs
            10 // Criteria.CustomerReliability
        };

        public ObservableCollection<int> GoalFunctionWeights { get; set; } = new ObservableCollection<int>
        {
            35,
            60,
            5
        };

        public ObservableCollection<ObservableCollection<bool>> AlgorithmsMatrix { get; set; } = new ObservableCollection<ObservableCollection<bool>> {
            new ObservableCollection<bool>{true, false, false},
            new ObservableCollection<bool>{false, false, false},
            new ObservableCollection<bool>{false, false, false},
            new ObservableCollection<bool>{false, false, false},
        };

        public bool IsAlgorithmsMatrixValid()
        {
            return AlgorithmsMatrix.Any(row => row.Any(col => col));
        }

        public static List<string> PossibleComparisionValues { get; } = new List<string>{
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "1/2", "1/3", "1/4", "1/5", "1/6", "1/7", "1/8", "1/9"
        };

        public static List<string> PossibleTarpaulinTypes { get; } = new List<string>{
            TarpaulinTypes.TILT, TarpaulinTypes.CURTAIN, TarpaulinTypes.WAGON, TarpaulinTypes.CHEST, TarpaulinTypes.TROLLEY, TarpaulinTypes.COOLING_BODIES
        };

        public int TrackHeight { get; set; } = 200;
        public int TrackWidth { get; set; } = 800;
        public int TrackDepth { get; set; } = 200;
        public int MaxLoadCapacity { get; set; } = 2000;
        public double AvgFuelConsumption { get; set; } = 5.0;
        public double FuelCost { get; set; } = 4.0;
        public double CostOfMaintain { get; set; } = 0.5;
        public string Postcode { get; set; } = "58-400";

        public double TypeOfLoadingMultipler { get; set; } = 0.4;
        public double RiskMultipler { get; set; } = 0.6;
        public static Configuration Create => new Configuration();

        public void CopyTo(Configuration originalConfiguration)
        {
            originalConfiguration.ComparisionMatrix = ComparisionMatrix;
            originalConfiguration.CriteriaWeights = CriteriaWeights;
            originalConfiguration.TrackHeight = TrackHeight;
            originalConfiguration.TrackWidth = TrackWidth;
            originalConfiguration.TrackDepth = TrackDepth;
            originalConfiguration.MaxLoadCapacity = MaxLoadCapacity;
            originalConfiguration.AvgFuelConsumption = AvgFuelConsumption;
            originalConfiguration.FuelCost = FuelCost;
            originalConfiguration.CostOfMaintain = CostOfMaintain;
        }

        public bool IsValid()
        {
            return AreCriteriaWeightValid() && AreGoalFunctionWeightValid();
        }

        public bool AreCriteriaWeightValid()
        {
            return CriteriaWeights.Sum() == SUM_OF_WEIGHTS;
        }

        public bool AreGoalFunctionWeightValid()
        {
            return GoalFunctionWeights.Sum() == SUM_OF_WEIGHTS;
        }

        public void fillMatrix()
        {
            var converter = new MatrixItemConverter();

            for (int row = 0; row < ComparisionMatrix.Count; row++)
            {
                var rowCol = ComparisionMatrix[row];
                for (int col = 0; col < rowCol.Count; col++)
                {
                    if (row == col)
                        break;

                    ComparisionMatrix[row][col] = converter.Convert(ComparisionMatrix[col][row], null, null, null) as string;
                }

            }
        }
    }
}

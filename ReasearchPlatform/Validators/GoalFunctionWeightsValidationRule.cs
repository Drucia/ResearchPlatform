using ResearchPlatform.Models;
using ResearchPlatform.ViewModels;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ResearchPlatform.Validators
{
    public class GoalFunctionWeightsValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = value as BindingGroup;
            if (bindingGroup.Items.Count > 1)
            {
                object item = bindingGroup.Items[0];
                MainWindowViewModel viewModel =
                  item as MainWindowViewModel;
                if (viewModel != null && viewModel.Configuration != null &&
                  !viewModel.Configuration.AreGoalFunctionWeightValid())
                    return new ValidationResult(false,
                      Messages.WEIGHTS_VALIDATION_MSG);
            }

            return ValidationResult.ValidResult;
        }
    }
}

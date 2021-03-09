using ResearchPlatform.Models;
using ResearchPlatform.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ResearchPlatform.Validators
{
    public class AlgorithmsChosenMatrixValidationRule : ValidationRule
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
                  !viewModel.Configuration.IsAlgorithmsMatrixValid())
                    return new ValidationResult(false,
                      Messages.ALGORITHMS_MATRIX_VALIDATION_MSG);
            }

            return ValidationResult.ValidResult;
        }
    }
}

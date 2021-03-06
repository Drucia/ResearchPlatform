using ResearchPlatform.Models;
using ResearchPlatform.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace ResearchPlatform.Validators
{
    public class WeightsValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = value as BindingGroup;
            if (bindingGroup.Items.Count > 1)
            {
                object item = bindingGroup.Items[0];
                SettingsDialogViewModel viewModel =
                  item as SettingsDialogViewModel;
                if (viewModel != null && viewModel.Configuration != null &&
                  !viewModel.Configuration.IsValid())
                    return new ValidationResult(false,
                      Messages.WEIGHTS_VALIDATION_MSG);
            }

            return ValidationResult.ValidResult;
        }
    }
}

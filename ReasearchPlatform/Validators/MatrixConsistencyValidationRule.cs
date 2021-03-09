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
    public class MatrixConsistencyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = value as BindingGroup;
            if (bindingGroup.Items.Count > 1)
            {
                object item = bindingGroup.Items[0];
                SettingsDialogViewModel viewModel =
                  item as SettingsDialogViewModel;
                if (viewModel != null)
                {
                    var conf = viewModel.Configuration;
                    conf.fillMatrix();
                    var isConsistent = AlgorithmsManager.GetInstance().CheckMatrixConsistency(conf.ComparisionMatrix);
                    if (!isConsistent)
                        return new ValidationResult(false,
                          Messages.MATRIX_ERROR_MSG);
                }
            }

            return ValidationResult.ValidResult;
        }
    }
}

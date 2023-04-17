using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Opus.Common.Validation
{
    public class EmptyFieldRule : ValidationRule
    {
        public EmptyFieldRule() { }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string content)
            {
                if (string.IsNullOrEmpty(content))
                    return new ValidationResult(false, Resources.Validation.General.FieldEmpty);

                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, Resources.Validation.General.FieldEmpty);
        }
    }
}

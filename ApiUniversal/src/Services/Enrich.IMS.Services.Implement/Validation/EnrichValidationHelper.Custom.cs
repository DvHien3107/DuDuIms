using FluentValidation.Results;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Validation
{
    public partial class EnrichValidationHelper
    {
        #region ObligedFieldRules
      

        private bool IsNullValue(object value, Type valType)
        {
            if (value == null)
                return true;

            if (value is string str && string.IsNullOrWhiteSpace(str))
                return true;

            if (value is ICollection collection && collection.Count == 0)
                return true;

            if (value is IEnumerable list && !list.Cast<object>().Any())
                return true;           

            return false;
        }

        private bool IsGreaterThanZero(object value, Type valType)
        {
            if (IsNullValue(value, valType))
                return false;

            if (value is decimal dc && dc <= 0)
                return false;

            if (value is int it && it <= 0)
                return false;

            if (value is float fl && fl <= 0)
                return false;

            if (value is double db && db <= 0)
                return false;
            return true;
        }

        #endregion
    }
}

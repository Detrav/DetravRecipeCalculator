using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    public static class ExpressionUtils
    {
        public static double GetValue(string? expression, int tier, double defaultValue)
        {
            if (String.IsNullOrEmpty(expression))
                return defaultValue;

            // todo;
            return tier * defaultValue;
        }
    }
}

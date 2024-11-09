using DetravRecipeCalculator.ViewModels;
using org.matheval;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetravRecipeCalculator.Utils
{
    public static class ExpressionUtils
    {

        public static Dictionary<string, double> Split(string? variables)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            if (variables != null)
            {
                foreach (var kv in variables.Split(' '))
                {
                    var arr2 = kv.Split('=');

                    if (arr2.Length == 2)
                    {
                        double.TryParse(arr2[1], out var value);
                        result[arr2[0]] = value;
                    }
                    else
                    {
                        result[kv] = 0;
                    }
                }
            }

            return result;
        }

        public static double GetValue(string? valueExpression, Dictionary<string, double> values)
        {
            if (!String.IsNullOrWhiteSpace(valueExpression))
            {
                try
                {
                    Expression expression = new Expression(valueExpression);

                    foreach (var kv in values)
                    {
                        expression.Bind(kv.Key, kv.Value);
                    }

                    var result = expression.Eval();

                    if (result is IConvertible convertible)
                    {
                        var result2 = convertible.ToDouble(CultureInfo.InvariantCulture);
                        return result2;
                    }
                }
                catch
                {

                }
            }
            return 0;
        }
    }
}
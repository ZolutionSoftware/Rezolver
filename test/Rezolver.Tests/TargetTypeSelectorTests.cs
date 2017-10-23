using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
    public partial class TargetTypeSelectorTests
    {
        private void LogTypes(Type[] types, string header)
        {
            Output.WriteLine(header);
            Output.WriteLine("================");

            int counter = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var type in types)
            {
                type.CSharpLikeTypeName(sb);
                Output.WriteLine("{0} => {1}", counter++, sb.ToString());
                sb.Clear();
            }
            Output.WriteLine("");
        }

        private void LogActual(Type[] result)
        {
            LogTypes(result, "Actual");
        }

        private void LogExpectedOrder(Type[] expected)
        {
            LogTypes(expected, "Expected (Specified Relative Order)");
        }

        private void LogOthers(Type[] anyExpected)
        {
            if (anyExpected != null)
                LogTypes(anyExpected, "Expected (Order Not Specified)");
        }
    }
}

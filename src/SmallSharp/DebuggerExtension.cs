using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SmallSharp
{
    static class DebuggerExtension
    {
        //[Conditional("DEBUG")]
        public static void CheckDebugger(this GeneratorExecutionContext context, string generatorName = nameof(SmallSharp))
            => context.AnalyzerConfigOptions.CheckDebugger(generatorName);

        public static void CheckDebugger(this AnalyzerConfigOptionsProvider provider, string generatorName = nameof(SmallSharp))
        {
            if (provider.GlobalOptions.TryGetValue("build_property.DebugSourceGenerators", out var debugValue) &&
                bool.TryParse(debugValue, out var shouldDebug) &&
                shouldDebug)
            {
                Debugger.Launch();
            }
            else if (provider.GlobalOptions.TryGetValue("build_property.Debug" + generatorName, out debugValue) &&
                bool.TryParse(debugValue, out shouldDebug) &&
                shouldDebug)
            {
                Debugger.Launch();
            }
        }
    }
}

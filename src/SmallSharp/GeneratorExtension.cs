﻿using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace SmallSharp
{
    static class GeneratorExtensions
    {
        //[Conditional("DEBUG")]
        public static void CheckDebugger(this GeneratorExecutionContext context, string generatorName = nameof(SmallSharp))
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.DebugSourceGenerators", out var debugValue) &&
                bool.TryParse(debugValue, out var shouldDebug) &&
                shouldDebug)
            {
                Debugger.Launch();
            }
            else if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.Debug" + generatorName, out debugValue) &&
                bool.TryParse(debugValue, out shouldDebug) &&
                shouldDebug)
            {
                Debugger.Launch();
            }
        }
    }
}

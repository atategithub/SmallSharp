using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SmallSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OpenStartupFileAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "SCS001",
            nameof(OpenStartupFileAnalyzer),
            nameof(OpenStartupFileAnalyzer),
            "Build",
            DiagnosticSeverity.Hidden, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

            context.RegisterCompilationStartAction(c =>
            {
                if (c.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.ActiveDebugProfile", out var activeProfile) &&
                    c.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var projectDirectory))
                {
                    var filePath = Path.Combine(projectDirectory, activeProfile);
                    EnsureOpened(c.Options, filePath);
                }
            });

            context.RegisterAdditionalFileAction(a => 
            {
                a.Options.AnalyzerConfigOptionsProvider.CheckDebugger();

                if (a.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.ActiveDebugProfile", out var activeProfile) &&
                    a.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var projectDirectory))
                {
                    var filePath = Path.Combine(projectDirectory, activeProfile);
                    EnsureOpened(a.Options, filePath);
                }
            });

            context.RegisterSymbolStartAction(c =>
            {
                var location = c.Symbol.Locations.FirstOrDefault(f => f.IsInSource);
                var filePath = location?.SourceTree?.FilePath;
                if (filePath != null)
                    EnsureOpened(c.Options, filePath);

            }, SymbolKind.NamedType);
        }

        void EnsureOpened(AnalyzerOptions options, string filePath)
        {
            options.AnalyzerConfigOptionsProvider.CheckDebugger();

            try
            {
                var workspace = options.AsDynamicReflection().Services.Workspace;
                IEnumerable<object> ids = workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath);
                var docId = ids.FirstOrDefault();
                if (docId == null)
                    return;

                var isopen = ((IEnumerable<object>)workspace.GetOpenDocumentIds(null)).Any(id => Equals(id, docId));
                if (!isopen)
                    workspace.OpenDocument(docId, true);
            }
            catch
            {
                // NOT SUPPORTED
            }
        }
    }
}

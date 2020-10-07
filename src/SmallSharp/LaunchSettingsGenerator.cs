using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Dynamic;

namespace SmallSharp
{
    [Generator]
    public class LaunchSettingsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.CheckDebugger();

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ActiveDebugProfile", out var activeProfile) &&
                context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var projectDirectory))
            {
                var filePath = Path.Combine(projectDirectory, activeProfile);
                EnsureOpened(context.AnalyzerConfigOptions, filePath);
            }

            var documents = from additional in context.AdditionalFiles
                            let options = context.AnalyzerConfigOptions.GetOptions(additional)
                            let compile = options.TryGetValue("build_metadata.AdditionalFiles.SourceItemType", out var itemType) && itemType == "Compile"
                            where compile
                            select additional.Path;

            var settings = new JObject(
                new JProperty("profiles", new JObject(
                    documents.OrderBy(path => Path.GetFileName(path)).Select(path => new JProperty(Path.GetFileName(path), new JObject(
                        new JProperty("commandName", "Project")
                    )))
                ))
            );

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var directory))
            {
                Directory.CreateDirectory(Path.Combine(directory, "Properties"));
                var filePath = Path.Combine(directory, "Properties", "launchSettings.json");
                var json = settings.ToString(Formatting.Indented);

                // Only write if different content.
                if (File.Exists(filePath) &&
                    File.ReadAllText(filePath) == json)
                    return;

                File.WriteAllText(filePath, json);
            }


        }

        void EnsureOpened(AnalyzerConfigOptionsProvider options, string filePath)
        {
            options.CheckDebugger();

            try
            {
                var workspace = options.AsDynamicReflection()._projectState.LanguageServices.WorkspaceServices.Workspace;
                IEnumerable<object> ids = workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath);
                var docId = ids.FirstOrDefault();
                if (docId == null)
                    return;

                var isopen = ((IEnumerable<object>)workspace.GetOpenDocumentIds(null).target).Any(id => Equals(id, docId));
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

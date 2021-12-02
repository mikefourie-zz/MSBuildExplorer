//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildHelper.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace MSBuildExplorer
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using MSBuildExplorer.DataModel;

    internal static class MSBuildHelper
    {
        internal static MSBuildFile GetFile(FileInfo file)
        {
            using (ProjectCollection loadedProjects = new ProjectCollection(ToolsetDefinitionLocations.Default))
            {
                var v15path = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\bin";
                var current = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin";
                loadedProjects.AddToolset(new Toolset("15.0", v15path, loadedProjects, v15path));
                loadedProjects.AddToolset(new Toolset("Current", current, loadedProjects, current));

                Debug.WriteLine(loadedProjects.DefaultToolsVersion);
                var tool14 = loadedProjects.GetToolset("14.0");
                Project currentProject = loadedProjects.GetLoadedProjects(file.FullName).FirstOrDefault();

                if (currentProject != null)
                {
                    loadedProjects.UnloadProject(currentProject);
                }

                loadedProjects.LoadProject(file.FullName);
                currentProject = loadedProjects.GetLoadedProjects(file.FullName).FirstOrDefault();

                MSBuildFile m = new MSBuildFile(currentProject);
                if (currentProject.Targets != null)
                {
                    SortedDictionary<string, ProjectTargetInstance> sortedTargets = new SortedDictionary<string, ProjectTargetInstance>(currentProject.Targets);
                    foreach (var t in sortedTargets)
                    {
                        ProjectTargetInstance pti = t.Value;
                        m.Targets.Add(new MSBuildTarget(pti, currentProject, m));
                    }
                }

                foreach (var us in currentProject.Xml.UsingTasks)
                {
                    m.Usings.Add(new MSBuildUsing(currentProject, us));
                }

                foreach (ResolvedImport import in currentProject.Imports)
                {
                    m.Imports.Add(new MSBuildImport(import));
                }

                foreach (var property in currentProject.AllEvaluatedProperties)
                {
                    m.Properties.Add(new MSBuildProperties(property));
                }

                if (currentProject.AllEvaluatedItems != null)
                {
                    foreach (var item in currentProject.AllEvaluatedItems)
                    {
                        m.Items.Add(new MSBuildItems(item.ItemType, item.UnevaluatedInclude, item.EvaluatedInclude, item.IsImported));
                    }
                }

                return m;
            }
        }
    }
}

//---------------------------------------------------------------------------------------------------------------------------
// <copyright file="MSBuildHelper.cs">(c) Mike Fourie. All other rights reserved.</copyright>
//---------------------------------------------------------------------------------------------------------------------------
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
            using (ProjectCollection loadedProjects = new ProjectCollection())
            {
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

                foreach (var property in currentProject.Properties)
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

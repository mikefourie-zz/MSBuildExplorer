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
	using System;

	internal static class MSBuildHelper
    {
        internal static MSBuildFile GetFile(FileInfo file, String propertyName = null, String propertyValue = null)
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
                if (!String.IsNullOrEmpty(propertyName))
                {
                    currentProject.SetGlobalProperty(propertyName, propertyValue);
                    currentProject.ReevaluateIfNecessary();
                }

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

                foreach (ProjectProperty property in currentProject.Properties)
                {
                    m.Properties.Add(new MSBuildProperties(property));
                }

                var cp = currentProject.ConditionedProperties;
                var configs = cp["Configuration"];
                var platforms = cp["Platform"];
                foreach (String config in configs)
                {
                    m.GlobalConfigurations.Add(config);
                }
                foreach (String platform in platforms)
                {
                    m.GlobalPlatforms.Add(platform);
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

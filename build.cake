#tool "nuget:?package=Fixie"
#addin "nuget:?package=Cake.Watch"

var solution = "MSBuildExplorer.sln";

Action<string, string> build = (config, target) => {
    DotNetBuild(solution, settings => {
        settings.SetConfiguration("Debug")
            .WithTarget("Build");
    });
};

Task("Build-Debug")
    .Does(() => {
        build("Debug", "Build");
    });

Task("Build-Release")
    .Does(() => {
        build("Release", "Build");
    });

var target = Argument("target", "default");
RunTarget(target);
#tool "nuget:?package=Fixie"
#addin "nuget:?package=Cake.Watch"

var user = EnvironmentVariable("ghu");
var pass = EnvironmentVariable("ghp");
var solution = "MSBuildExplorer.sln";

Action<string, string> build = (config, target) => {
    DotNetBuild(solution, settings => {
        settings.SetConfiguration(config)
            .WithTarget(target);
    });
};

Func<string> getVersion = () => {
    var asm = ParseAssemblyInfo("MSBuildExplorer/Properties/AssemblyInfo.cs");
    var version = asm.AssemblyVersion;
    return version;
};

Task("Create-Github-Release")
    .IsDependentOn("Zip")
    .Does(() => {
        var package = new System.IO.DirectoryInfo("./").GetFiles("*.zip").FirstOrDefault();
        var version = getVersion();
        var tag = string.Format("v{0}", version);
        var args = string.Format("tag -a {0} -m \"{0}\"", tag);
        var owner = "wk-j";
        var repo = "msbuild-explorer";

        StartProcess("git", new ProcessSettings {
            Arguments = args
        });

        StartProcess("git", new ProcessSettings {
            Arguments = string.Format("push https://{0}:{1}@github.com/wk-j/{2}.git {3}", user, pass, repo, tag)
        });

        GitReleaseManagerCreate(user, pass, owner , repo, new GitReleaseManagerCreateSettings {
            Name              = tag,
            InputFilePath = "RELEASE.md",
            Prerelease        = false,
            TargetCommitish   = "master",
        });
        GitReleaseManagerAddAssets(user, pass, owner, repo, tag, package.FullName);
        GitReleaseManagerPublish(user, pass, owner , repo, tag);
    });

Task("Zip")
    .IsDependentOn("Build-Release")
    .Does(() => {
            DeleteFiles("*.zip");
            var version = getVersion();
            var target = String.Format("MSBuildExplorer-{0}.zip", version);
            DeleteFiles("./MSBuildExplorer/bin/Release/*.xml");
            DeleteFiles("./MSBuildExplorer/bin/Release/*.lastcodeanalysissucceeded");
            DeleteFiles("./MSBuildExplorer/bin/Release/*.application");
            DeleteFiles("./MSBuildExplorer/bin/Release/*.manifest");
            DeleteDirectory("./MSBuildExplorer/bin/Release/app.publish", true);
            Zip("./MSBuildExplorer/bin/Release", target);
    });

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
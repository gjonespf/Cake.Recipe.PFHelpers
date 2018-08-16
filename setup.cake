#load "nuget:https://nuget.powerfarming.co.nz/api/odata?package=Cake.Recipe.PF&version=0.2.1"
#load setup.selfbootstrap.cake

//#define CustomGitVersionTool
//private const string GitVersionTool = "#tool nuget:?package=GitVersion.CommandLine.DotNetCore&version=4.0.0-netstandard0001";

var target = "Default";

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            title: "Cake.Recipe.PFHelpers",
                            repositoryOwner: "gjones@powerfarming.co.nz",
                            repositoryName: "Cake.Recipe.PFHelpers",
                            nuspecFilePath: "Cake.Recipe.PFHelpers/Cake.Recipe.PFHelpers.nuspec",
                            shouldPostToMicrosoftTeams: true,
                            shouldRunGitVersion: true
                            );

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

Task("Init")
    .IsDependentOn("PFInit")
    .IsDependentOn("Generate-Version-File-PF")
	.Does(() => {
		Information("Init");
    });

BuildParameters.Tasks.CleanTask
    .IsDependentOn("Generate-Version-File-PF")
    .Does(() => {
    });

// BuildParameters.Tasks.RestoreTask.Task.Actions.Clear();
// BuildParameters.Tasks.RestoreTask
// 	//.IsDependentOn("ATask")
//     .Does(() => {
//     });

//BuildParameters.Tasks.PackageTask.Task.Actions.Clear();
BuildParameters.Tasks.PackageTask
	.IsDependentOn("Package-GenerateReleaseVersion")
    .Does(() => {
	});

// BuildParameters.Tasks.BuildTask.Task.Actions.Clear();
// BuildParameters.Tasks.BuildTask
// 	.Does(() => {
//         Information("TASK: Build");
// 	});

Task("Publish")
	.IsDependentOn("Publish-Artifacts")
	.IsDependentOn("Publish-LocalNugetCache")
	.IsDependentOn("Publish-LocalNuget")
	.IsDependentOn("PublishNotify")
	.Does(() => {
        Information("TASK: Publish");
	});

Task("PublishNotify")
	.IsDependentOn("PublishNotify-NotifyTeams")
	.Does(() => {
        Information("TASK: PublishNotify");
	});

// Simplified...
Build.RunVanilla();
//Build.RunNuGet();
//RunTarget(target);

Teardown(context =>
{
    // Executed AFTER the last task.
});

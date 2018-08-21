

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

BuildParameters.IsDotNetCoreBuild = false;
BuildParameters.IsNuGetBuild = true;

BuildParameters.Tasks.DefaultTask
    .IsDependentOn("Build");

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

BuildParameters.Tasks.PackageTask
	.IsDependentOn("Generate-Version-File-PF")
	.IsDependentOn("Package-GenerateReleaseVersion")
	.IsDependentOn("Create-NuGet-Package")
    .Does(() => {
	});

 BuildParameters.Tasks.BuildTask
     .IsDependentOn("Init");

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

    
Teardown(context =>
{
    // Executed AFTER the last task.
});


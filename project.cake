

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./",
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

// Setup(context =>
// {
// });

// TODO: Should be baked in
// TODO: Should be run from PFInit / how PFInit is run?
Task("ConfigureFromProjectParametersFile")
.WithCriteria<ProjectProperties>((context, data) => !string.IsNullOrEmpty(data.ProjectName))
.Does<ProjectProperties>(data => {
    Information("Setting properties based on ProjectProperties file: ", data.ProjectName);
});

Task("Init")
    .IsDependentOn("ConfigureFromProjectParametersFile")
    .IsDependentOn("PFInit")
    .IsDependentOn("Create-SolutionInfoVersion")
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
    .IsDependentOn("Init")
    .IsDependentOn("Generate-AssemblyInfo")
    .Does(() => {
	});

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

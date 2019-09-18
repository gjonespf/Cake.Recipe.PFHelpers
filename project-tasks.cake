// Temporary testing funcs
#load "project-tasks-pfhelpers.cake"

// TODO: Should be done in setup.cake pulling from properties.json? Or same here?
BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./",
                            title: "Cake.Recipe.PFHelpers",
                            repositoryOwner: "gjones@powerfarming.co.nz",
                            repositoryName: "Cake.Recipe.PFHelpers",
                            //nuspecFilePath: "Cake.Recipe.PFHelpers/Cake.Recipe.PFHelpers.nuspec",
                            shouldPostToMicrosoftTeams: true,
                            shouldRunGitVersion: true
                            );
// TODO: Pull these in via detection too or in properties.json
// BuildParameters.IsDotNetCoreBuild = false;
// BuildParameters.IsNuGetBuild = true;

BuildParameters.Tasks.DefaultTask
    .IsDependentOn("Build");


Task("Publish")
	.IsDependentOn("Publish-Artifacts")
	.IsDependentOn("Publish-LocalNugetCache")
	.IsDependentOn("Publish-LocalNuget")
	.IsDependentOn("Publish-LocalPublicNuget")
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


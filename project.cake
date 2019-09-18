Task("Init")
    .IsDependentOn("ConfigureFromProjectParametersFile")
    .IsDependentOn("PFInit")
    .IsDependentOn("Create-SolutionInfoVersion")
    .IsDependentOn("Generate-Version-File-PF")
    .Does<ProjectProperties>(props => {
		Information("Init");
    });

BuildParameters.Tasks.CleanTask
    // .IsDependentOn("Generate-Version-File-PF")
    .Does(() => {
    });

 BuildParameters.Tasks.BuildTask
    .IsDependentOn("Init")
    .IsDependentOn("Generate-AssemblyInfo")
    .Does(() => {
	});
    
BuildParameters.Tasks.PackageTask
	.IsDependentOn("Generate-Version-File-PF")
	.IsDependentOn("Package-GenerateReleaseVersion")
	.IsDependentOn("Create-NuGet-Packages")
    .Does(() => {
	});


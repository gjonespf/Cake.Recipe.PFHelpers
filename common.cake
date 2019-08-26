// Candidates for loading into PFHelpers

// TODO: Should be baked in
// TODO: Should be run from PFInit / how PFInit is run?
Task("ConfigureFromProjectParametersFile")
    .IsDependentOn("ConfigureProjectProperties")
    .WithCriteria<ProjectProperties>((context, data) => !string.IsNullOrEmpty(data.ProjectName))
    .Does<ProjectProperties>(data => {
        Information("Setting properties based on ProjectProperties file: ", data.ProjectName);
});



Setup<ProjectProperties>(setupContext => 
{
    try {
        Verbose("ProjectProperties - Setup");
        return LoadProjectProperties(null);
    } catch(Exception ex) {
        Error("ProjectProperties - Exception while setting up ProjectProperties: " +ex.Dump());
        return null;
    }
});

Setup<CustomBuildVersion>(context => 
{
    try {
        Verbose("CustomBuildVersion - Setup");
        return GenerateCustomBuildVersion(context);
    } catch(Exception ex) {
        Error("CustomBuildVersion - Exception while setting up CustomBuildVersion: " +ex.Dump());
        return null;
    }
});


Setup<PFCustomBuildParams>(context => 
{
    Verbose("PFCustomBuildParams - Setup");
    try {
        ProjectProperties projProps = context.Data.Get<ProjectProperties>();
        CustomBuildVersion buildVersion = context.Data.Get<CustomBuildVersion>();
        return GeneratePFCustomBuildParams(projProps, buildVersion);
    } catch(Exception ex) {
        Error("PFCustomBuildParams - Exception while setting up PFCustomBuildParams: " +ex.Dump());
        return null;
    }
});

Task("ConfigureProjectProperties")
    .Does<ProjectProperties>(props => {

        // Load from project.json file if available
});

Task("ConfigureCustomBuildVersion")
    .Does<CustomBuildVersion>((context, vers) => {
        Verbose("ConfigureCustomBuildVersion");
    });

Task("ConfigureCustomBuildParameters")
    .Does<PFCustomBuildParams>((context, vers) => {
        Verbose("ConfigureCustomBuildParameters");
    });    

Task("PFInit")
    .IsDependentOn("ConfigureProjectProperties")
    .IsDependentOn("ConfigureCustomBuildVersion")
    .IsDependentOn("ConfigureCustomBuildParameters")
    .Does<PFCustomBuildParams>((context, parms) => {
    });

// TODO: Update build params based on this info
Task("ConfigureFromProjectParametersFile")
    .IsDependentOn("ConfigureProjectProperties")
    .WithCriteria<ProjectProperties>((context, data) => !string.IsNullOrEmpty(data.ProjectName))
    .Does<ProjectProperties>(data => {
        Information("Setting properties based on ProjectProperties file: ", data.ProjectName);
});

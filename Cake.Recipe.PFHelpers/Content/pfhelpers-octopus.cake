
// TODO: RequireAddin and env vars override
Task("Publish-LocalOctopus")
    .Does(() => {
        var SourceUrl = EnvironmentVariable("OctoServerPushUrl");
        var ApiKey = EnvironmentVariable("OCTOAPIKEY");
        var DestinationName = "Local Octopus";
        var keyExists = !string.IsNullOrEmpty(ApiKey)?"PRESENT":"ABSENT";
        Information($"Publishing to {DestinationName} with source: {SourceUrl} and key: {keyExists}");

        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");

        if(string.IsNullOrEmpty(SourceUrl) || string.IsNullOrEmpty(ApiKey)) {
            throw new ApplicationException("Environmental variables 'OctoServerPushUrl' and 'OCTOAPIKEY' must be set to use this");
        }

        foreach(var nupkgFile in nupkgFiles)
        {
            // Push the package.
            NuGetPush(nupkgFile, new NuGetPushSettings {
                Source = SourceUrl,
                ApiKey = ApiKey
            });
        }
    });

Task("Publish-CloudOctopus")
    .Does(() => {
        var SourceServer = EnvironmentVariable("OCTOCLOUDSERVER");
        var ApiKey = EnvironmentVariable("OCTOCLOUDAPIKEY");
        var DestinationName = "Octopus Cloud";
        var keyExists = !string.IsNullOrEmpty(ApiKey)?"PRESENT":"ABSENT";
        Information($"Publishing to {DestinationName} with source: {SourceServer} and key: {keyExists}");

        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");

        if(string.IsNullOrEmpty(SourceServer) || string.IsNullOrEmpty(ApiKey)) {
            throw new ApplicationException("Environmental variables 'OCTOCLOUDSERVER' and 'OCTOCLOUDAPIKEY' must be set to use this");
        }

        foreach(var nupkgFile in nupkgFiles)
        {
            // Push the package.
            NuGetPush(nupkgFile, new NuGetPushSettings {
                Source = SourceServer+"/nuget/packages/",
                ApiKey = ApiKey
            });
        }
    });



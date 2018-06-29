
// TODO: Repurpose, as we're going to use the Cake.Recipe default ./BuildArtifacts path instead...
Task("Publish-Artifacts")
    .IsDependentOn("PFInit")
    .Does(() => {
        //var sourceArtifactPath = MakeAbsolute(Directory("./BuildArtifacts/"));
        if(!string.IsNullOrEmpty(BuildArtifactPath)) {
            Information("Copying artifacts to build artifact path: "+BuildArtifactPath);
            EnsureDirectoryExists(BuildArtifactPath);

            // var nupkgs = GetFiles(sourceArtifactPath+"/**/*.nupkg");
            // foreach(var filePath in nupkgs)
            // {
            //     CopyFile(filePath, BuildArtifactPath+"/"+filePath.GetFilename());
            // }
        } else {
            Error("No artifact path set!");
        }
    });

Task("Publish-Local")
    .IsDependentOn("Publish-Artifacts")
    .Does(() => {
        // Copy packaging files (nupkg) to local dirs if env set
    });

// PF Publishing
Task("Publish-LocalNugetCache")
    .Does(() => {
        var cacheDir = EnvironmentVariable("LOCAL_NUGET_CACHE");
        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");

        if(!string.IsNullOrEmpty(cacheDir))
        {
            EnsureDirectoryExists(cacheDir);

            foreach(var nupkgFile in nupkgFiles)
            {
                CopyFile(nupkgFile, cacheDir+"/"+nupkgFile.GetFilename());
            }
        } else {
            Warning("Publish-LocalNugetCache called but no LOCAL_NUGET_CACHE set, files will not be copied");
        }
    });

// TODO: RequireAddin and env vars override
Task("Publish-LocalNuget")
    .Does(() => {
        var SourceUrl = EnvironmentVariable("LocalNugetServerUrl");
        var ApiKey = EnvironmentVariable("LocalNugetApiKey");
        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");
        var DestinationName = "Local Nuget";
        var keyExists = !string.IsNullOrEmpty(ApiKey)?"PRESENT":"ABSENT";
        Information($"Publishing to {DestinationName} with source: {SourceUrl} and key: {keyExists}");

        if(string.IsNullOrEmpty(SourceUrl) || string.IsNullOrEmpty(ApiKey)) {
            throw new ApplicationException("Environmental variables 'LocalNugetServerUrl' and 'LocalNugetApiKey' must be set to use this");
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
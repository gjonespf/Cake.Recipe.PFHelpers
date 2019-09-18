Task("BuildPackage")
    .IsDependentOn("Build")
    .IsDependentOn("Package")
    .Does(() =>
{
});

Task("BuildPackagePublish")
    .IsDependentOn("Build")
    .IsDependentOn("Package")
    .IsDependentOn("Publish")
    .Does(() => {
	});

// TODO: Repurpose, as we're going to use the Cake.Recipe default ./BuildArtifacts path instead...
Task("Publish-Artifacts")
    .IsDependentOn("PFInit")
    .Does<PFCustomBuildParams>((context, data) => {
        var BuildArtifactPath = data != null ? data.BuildArtifactPath : null;

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

// TODO: Split to nuget
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
                ApiKey = ApiKey,
                ArgumentCustomization = (args) => {
                    return args;
                }
            });
        }
    })
    .OnError(exception =>
    {
        // Let's not abort the build completely if the push fails, as it may be a conflict
        // TODO: Identify conflict?
    });

Task("Publish-LocalPublicNuget")
    .Does(() => {
        var SourceUrl = EnvironmentVariable("ProjectLocalPublicNugetServerUrl");
        var ApiKey = EnvironmentVariable("LocalPublicNugetApiKey");
        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");
        var DestinationName = "Local Public Nuget";
        var keyExists = !string.IsNullOrEmpty(ApiKey)?"PRESENT":"ABSENT";
        Information($"Publishing to {DestinationName} with source: {SourceUrl} and key: {keyExists}");

        if(string.IsNullOrEmpty(SourceUrl) || string.IsNullOrEmpty(ApiKey)) {
            throw new ApplicationException("Environmental variables 'ProjectLocalPublicNugetServerUrl' and 'LocalPublicNugetApiKey' must be set to use this");
        }

        foreach(var nupkgFile in nupkgFiles)
        {
            // Push the package.
            NuGetPush(nupkgFile, new NuGetPushSettings {
                Source = SourceUrl,
                ApiKey = ApiKey,
                ArgumentCustomization = (args) => {
                    return args;
                }
            });
        }
    })
    .OnError(exception =>
    {
        // Let's not abort the build completely if the push fails, as it may be a conflict
        // TODO: Identify conflict?
    });

Task("Publish-GitHubNuget")
    .Does(() => {
        var SourceUrl = EnvironmentVariable("ProjectNugetGitHubPackageFeed");
        var ApiUser = EnvironmentVariable("GITHUB_USERNAME");
        var ApiKey = EnvironmentVariable("GITHUB_API_TOKEN");
        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");
        var DestinationName = "GitHub Nuget";
        var keyExists = !string.IsNullOrEmpty(ApiKey)?"PRESENT":"ABSENT";
        Information($"Publishing to {DestinationName} with source: {SourceUrl} and key: {keyExists}");

        if(string.IsNullOrEmpty(SourceUrl) || string.IsNullOrEmpty(ApiKey)) {
            throw new ApplicationException("Environmental variables 'ProjectNugetGitHubPackageFeed', 'GITHUB_USERNAME' and 'GITHUB_API_TOKEN' must be set to use this");
        }

        foreach(var nupkgFile in nupkgFiles)
        {
            // Push the package.
            NuGetPush(nupkgFile, new NuGetPushSettings {
                Source = SourceUrl,
                ApiKey = ApiKey,
                ArgumentCustomization = (args) => {
                    return args;
                }
            });
        }
    })
    .OnError(exception =>
    {
        // Let's not abort the build completely if the push fails, as it may be a conflict
        // TODO: Identify conflict?
    });
    
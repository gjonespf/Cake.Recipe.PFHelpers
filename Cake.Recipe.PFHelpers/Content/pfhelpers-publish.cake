

Task("Publish-Artifacts")
    .IsDependentOn("PFInit")
    .Does(() => {
        var sourceArtifactPath = MakeAbsolute(Directory("./BuildArtifacts/"));
        if(!string.IsNullOrEmpty(BuildArtifactPath)) {
            Information("Copying artifacts to build artifact path: "+BuildArtifactPath);
            EnsureDirectoryExists(BuildArtifactPath);

            var nupkgs = GetFiles(sourceArtifactPath+"/**/*.nupkg");
            foreach(var filePath in nupkgs)
            {
                CopyFile(filePath, BuildArtifactPath+"/"+filePath.GetFilename());
            }
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
// TODO: RequireAddin and env vars
Task("Publish-LocalNuget")
    .Does(() => {
        var SourceUrl = EnvironmentVariable("LocalNugetServerUrl");
        var ApiKey = EnvironmentVariable("LocalNugetApiKey");
        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");

        foreach(var nupkgFile in nupkgFiles)
        {
            // Push the package.
            NuGetPush(nupkgFile, new NuGetPushSettings {
                Source = SourceUrl,
                ApiKey = ApiKey
            });
        }
    });

Task("Publish-LocalOctopus")
    .Does(() => {
        var SourceUrl = EnvironmentVariable("OCTOSERVER");
        var ApiKey = EnvironmentVariable("OCTOAPIKEY");

        var nupkgFiles = GetFiles(BuildParameters.Paths.Directories.NuGetPackages + "/**/*.nupkg");

            foreach(var nupkgFile in nupkgFiles)
            {
                // Push the package.
                NuGetPush(nupkgFile, new NuGetPushSettings {
                    Source = SourceUrl,
                    ApiKey = ApiKey
                });
            }
    });
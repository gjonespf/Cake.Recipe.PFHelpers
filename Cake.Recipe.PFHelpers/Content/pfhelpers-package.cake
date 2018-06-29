
Task("Package-GenerateReleaseVersion")
    .IsDependentOn("PFInit")
    .IsDependentOn("Generate-Version-File-PF")
    .Does(() => {
        var rel = GenerateReleaseVersion();
        if(rel != null) {
            SaveReleaseVersion(rel);
        }
        var versionFilePath = $"./{ReleaseVersionFileName}";
        if(BuildArtifactPath != null) {
            Information("Copying versioning to build artifact path: "+BuildArtifactPath);
            EnsureDirectoryExists(BuildArtifactPath);
            CopyFile(versionFilePath, BuildArtifactPath+$"/{ReleaseVersionFileName}");
        } else {
            Error("No artifact path set!");
        }
    });


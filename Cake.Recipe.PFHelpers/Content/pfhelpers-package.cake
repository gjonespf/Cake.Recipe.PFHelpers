
Task("Package-GenerateReleaseVersion")
    .IsDependentOn("PFInit")
    .IsDependentOn("Generate-Version-File-PF")
    .Does<PFCustomBuildParams>((context, data) => {
        var rel = GenerateReleaseVersion(data);
        var BuildArtifactPath = data != null ? data.BuildArtifactPath : null;
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


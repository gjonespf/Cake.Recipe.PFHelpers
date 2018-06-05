
Task("Package-GenerateReleaseVersion")
    .IsDependentOn("PFInit")
    .IsDependentOn("Generate-Version-File-PF")
    .Does(() => {
        var rel = GenerateReleaseVersion();
        if(rel != null) {
            SaveReleaseVersion(rel);
        }
    });


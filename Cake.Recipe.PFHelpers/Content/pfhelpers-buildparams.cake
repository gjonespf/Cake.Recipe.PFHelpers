


public class PFCustomBuildParams
{
    public string BuildArtifactPath;
    public string BuildNumber;
    public CustomBuildVersion PFBuildVersion;
    public ProjectProperties ProjectProps;
}

public PFCustomBuildParams GeneratePFCustomBuildParams(ProjectProperties props, CustomBuildVersion vers)
{
    var ret = new PFCustomBuildParams();
    var buildNum = EnvironmentVariable("BUILD_NUMBER");
    if(string.IsNullOrEmpty(buildNum) ||
        buildNum.StartsWith("HASH-")) {
        // Use current commit
        var lastCommit = GitLogTip(".");
        var commitHash = lastCommit.Sha;
        buildNum = "HASH-"+commitHash;
    }

    var artifactPath = MakeAbsolute(Directory("./BuildArtifacts/")).FullPath;
    Information("Artifact path set to: "+artifactPath);
    ret.BuildArtifactPath = artifactPath;
    ret.BuildNumber = buildNum;

    if(props != null) {
        ret.ProjectProps = props;
    } else {
        ret.ProjectProps = LoadProjectProperties(MakeAbsolute(Directory(".")));
    }
    ret.PFBuildVersion = vers;

    EnsureDirectoryExists(ret.BuildArtifactPath);

    return ret;
}

Setup<PFCustomBuildParams>(context => 
{
    Verbose("Setup - PFCustomBuildParams");
    ProjectProperties projProps = context.Data.Get<ProjectProperties>();
    CustomBuildVersion buildVersion = context.Data.Get<CustomBuildVersion>();
    return GeneratePFCustomBuildParams(projProps, buildVersion);
});

Task("PFInit")
    .Does<PFCustomBuildParams>((context, parms) => {
        // if(parms.BuildArtifactPath != null) {
        //     Information("Copying versioning to build artifact path: "+parms.BuildArtifactPath);
        //     EnsureDirectoryExists(parms.BuildArtifactPath);
        //     CopyFile(versionFilePath, parms.BuildArtifactPath+$"/{BuildVersionFileName}");
        // } else {
        //     Error("No artifact path set!");
        // }
    });

Task("PFInit-Clean")
    .Does<PFCustomBuildParams>((context, parms) => {
        var BuildArtifactPath = parms.BuildArtifactPath;

        if(!string.IsNullOrEmpty(BuildArtifactPath)) {
            if (DirectoryExists(BuildArtifactPath))
            {
                // Param to force clean, not sure it's currently needed
                //ForceDeleteDirectory(BuildArtifactPath);
            }
            EnsureDirectoryExists(BuildArtifactPath);
        }
    });
    
Task("Purge")
    .IsDependentOn("Clean")
    .Does(() =>
{
    GitClean(".");

    ForceDeleteDirectory("./.git/gitversion_cache/");
    ForceDeleteDirectory("./BuildArtifacts/");
    if(FileExists("./tools/packages.config.md5sum")) {
        DeleteFiles("./tools/packages.config.md5sum");
    }
    if(FileExists("./gitversion.properties")) {
        DeleteFiles("./gitversion.properties");
    }
    DeleteFiles("./*version.json");

    // rm -Recurse tools/*
    // Note many if not all of these will be locked...
    var directories = GetDirectories("./tools/*");
    foreach(var directory in directories)
    {
        Information("Purging Directory: {0}", directory);
        try
        {
            ForceDeleteDirectory(directory.FullPath);
        }
        catch (System.Exception)
        {
            Information("Exception Purging Directory: {0}", directory);
        }
    }
});

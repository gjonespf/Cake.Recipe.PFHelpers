//#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease
// #addin "nuget:https://www.nuget.org/api/v2?package=Newtonsoft.Json"
// using Newtonsoft.Json;
// #addin nuget:?package=Cake.Git&version=0.17.0

#load pfhelpers-addins.cake
#load pfhelpers-projprops.cake
#load pfhelpers-toolfuncs.cake
#load pfhelpers-package.cake
#load pfhelpers-publish.cake
#load pfhelpers-release.cake
#load pfhelpers-versioning.cake
#load pfhelpers-npm.cake
#load pfhelpers-teams.cake
#load pfhelpers-docker.cake

public static ProjectProperties ProjectProps;

// TASKS
var initDone = 
            !string.IsNullOrEmpty("BUILD_NUMBER") 
            && BuildArtifactPath != null 
            && BuildNumber != null
            && ProjectProps != null;
Task("PFInit")
    .WithCriteria(!initDone)
    .Does(() => {
        // Ensure build number & output directory for artifacts
        var buildNum = EnvironmentVariable("BUILD_NUMBER");
        if(string.IsNullOrEmpty(buildNum) ||
            buildNum.StartsWith("HASH-")) {
            // Use current commit
            var lastCommit = GitLogTip(".");
            var commitHash = lastCommit.Sha;
            buildNum = "HASH-"+commitHash;
        }

        var artifactPath = MakeAbsolute(Directory("./BuildArtifacts/"+buildNum)).FullPath;
        Information("Artifact path set to: "+artifactPath);
        BuildArtifactPath = artifactPath;
        BuildNumber = buildNum;
        EnsureDirectoryExists(BuildArtifactPath);
        ProjectProps = LoadProjectProperties(MakeAbsolute(Directory(".")));
    });

Task("PFInit-Clean")
    .Does(() => {
        if(!string.IsNullOrEmpty(BuildArtifactPath)) {
            if (DirectoryExists(BuildArtifactPath))
            {
                // Param to force clean, not sure it's currently needed
                //ForceDeleteDirectory(BuildArtifactPath);
            }
            EnsureDirectoryExists(BuildArtifactPath);
        }
    });

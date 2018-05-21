#addin "nuget:https://www.nuget.org/api/v2?package=Newtonsoft.Json"
using Newtonsoft.Json;
#addin nuget:?package=Cake.Git&version=0.17.0

public static string BuildArtifactPath;
public static string BuildNumber;
public static CustomBuildVersion PFBuildVersion;

// VERSIONING
public class CustomBuildVersion
{
    public string Version { get; set; }
    public string SemVersion { get; set; }
    public string MajorMinorPatch { get; set; }
    public string Major { get; set; }
    public string Minor { get; set; }
    public string Patch { get; set; }
    public string Milestone { get; set; }
    public string InformationalVersion { get; set; }
    public string FullSemVersion { get; set; }
    public string BranchName { get; set; }
    public string CommitHash { get; set; }
    public string CommitDate { get; set; }
}

Task("Generate-Version-File-PF")
    .Does(() => {
        var props = ReadDictionaryFile("./gitversion.properties");
        var versionFilePath = "./BuildVersion.json";

        var vers = new CustomBuildVersion() {
            Version = BuildParameters.Version.Version,
            SemVersion = BuildParameters.Version.SemVersion,
            MajorMinorPatch = props["GitVersion_MajorMinorPatch"],
            Major = props["GitVersion_Major"],
            Minor = props["GitVersion_Minor"],
            Patch = props["GitVersion_Patch"],
            Milestone = BuildParameters.Version.Milestone,
            InformationalVersion = BuildParameters.Version.InformationalVersion,
            FullSemVersion = BuildParameters.Version.FullSemVersion,
            BranchName = props["GitVersion_BranchName"],
            CommitHash = props["GitVersion_Sha"],
            CommitDate = props["GitVersion_CommitDate"],
        };
        PFBuildVersion = vers;
        var versionData = JsonConvert.SerializeObject(vers, Formatting.Indented);

        System.IO.File.WriteAllText(
            versionFilePath,
            versionData
            );

        if(BuildArtifactPath != null) {
            Information("Copying versioning to build artifact path: "+BuildArtifactPath);
            EnsureDirectoryExists(BuildArtifactPath);
            CopyFile(versionFilePath, BuildArtifactPath+"/BuildVersion.json");
        } else {
            Error("No artifact path set!");
        }
    });

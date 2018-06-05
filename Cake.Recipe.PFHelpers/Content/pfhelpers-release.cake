
public class ReleaseVersion
{
    public string PackagePath { get; set; }
    public string PackageName { get; set; }
    public string PackageRepo { get; set; }
    public string PackageUrl { get; set; }
    public string Version { get; set; }
    public string SemVersion { get; set; }
    public string BranchName { get; set; }
    public string CommitHash { get; set; }
    public string CommitDate { get; set; }
}

public static string ReleaseVersionFileName = "ReleaseVersion.json";

public void SaveReleaseVersion(ReleaseVersion relVer)
{        
    var versionFilePath = $"./{ReleaseVersionFileName}";
    if(relVer != null) {
        var versionData = JsonConvert.SerializeObject(relVer, Formatting.Indented);

        System.IO.File.WriteAllText(
            versionFilePath,
            versionData
            );
    } else {
        throw new ApplicationException("Tried to write null release version information");
    }
}

public ReleaseVersion LoadReleaseVersion()
{
    ReleaseVersion relVer = null;
    var versionFilePath = $"./{ReleaseVersionFileName}";
    if(FileExists(versionFilePath)) {
        var jsonData = String.Join(System.Environment.NewLine, System.IO.File.ReadAllLines(versionFilePath));
        relVer = JsonConvert.DeserializeObject<ReleaseVersion>(jsonData);
    } else {
        // Log warning how?
    }
    return relVer;
}

public ReleaseVersion GenerateReleaseVersion()
{
    var relVer = new ReleaseVersion() {
            PackagePath = "UNKNOWN",
            PackageName = "UNKNOWN",
            PackageRepo = "UNKNOWN",
            Version = BuildParameters.Version.Version,
            SemVersion = BuildParameters.Version.SemVersion,
            BranchName = PFBuildVersion.BranchName,
            CommitHash = PFBuildVersion.CommitHash,
            CommitDate = PFBuildVersion.CommitDate,
        };
    var props = LoadProjectProperties();
    // Grab defaults from props?
    if(props != null)
    {
            relVer.PackageName = props.ProjectName;
            relVer.PackageRepo = props.DefaultRemote;
    }
    return  relVer;
}
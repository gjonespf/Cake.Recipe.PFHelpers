#addin nuget:?package=Newtonsoft.Json&version=12.0.2

using Newtonsoft.Json;

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

public ReleaseVersion GenerateReleaseVersion(PFCustomBuildParams parms)
{
    var relVer = new ReleaseVersion() {
            PackagePath = "UNKNOWN",
            PackageName = "UNKNOWN",
            PackageRepo = "UNKNOWN",
            Version = BuildParameters.Version.Version,
            SemVersion = BuildParameters.Version.SemVersion
        };
    if(parms != null && parms.PFBuildVersion != null)
    {
        relVer.BranchName = parms.PFBuildVersion.BranchName;
        relVer.CommitHash = parms.PFBuildVersion.CommitHash;
        relVer.CommitDate = parms.PFBuildVersion.CommitDate;
    }
    var props = LoadProjectProperties();
    // Grab defaults from props?
    if(props != null)
    {
            relVer.PackageName = props.ProjectName;
            relVer.PackageRepo = props.DefaultRemote;
    }
    return  relVer;
}

Setup<ReleaseVersion>(context => 
{
    try {
        Verbose("ReleaseVersion - Setup");
        PFCustomBuildParams parms = context.Data.Get<PFCustomBuildParams>();
        return GenerateReleaseVersion(parms);
    } catch(Exception ex) {
        Error("ReleaseVersion - Exception while setting up ReleaseVersion: " +ex.Dump());
        return null;
    }
});
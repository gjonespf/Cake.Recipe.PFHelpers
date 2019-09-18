public class ProjectProperties
{
    public string ProjectName { get; set; }   
    public string ProjectCodeName { get; set; }   
    public string ProjectDescription { get; set; }
    public string ProjectUrl { get; set; }
    public string SourceControlUrl { get; set; }
    public string ProjectVersioning { get; set; }

    public string ProjectLocalPublicNugetServerUrl { get; set; }
    public string ProjectNugetGithubPackageFeed { get; set; }

    // TODO: Rework to be more generic and non docker focussed
    public string DockerDefaultUser { get;set; }
    public string DockerDefaultRemote { get;set; }
    public string DockerDefaultLocal { get;set; }
    public string DockerDefaultTag { get;set; }

    // TODO: Probably split optionals out into a generic property bag
    public string TeamsWebHook { get;set; }
}

public class DockerDetails
{
    public string ImageName { get; set; }
    public string ImageDescription { get; set; }
    public string ImageUrl { get; set; }
    public string GitUrl { get; set; }

    public string BuildId { get;set; }
    public string[] BuildArguments { get; set; }

    public string[] LocalTags { get; set; }
    public string DefaultLocal { get;set; }
    public string[] RemoteTags { get; set; }
    public string DefaultRemote { get;set; }
}

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
    public string BuildId { get; set; }
    public string BuildUrl { get; set; }
}

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



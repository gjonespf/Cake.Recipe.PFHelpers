
// TODO: Need a way of pulling versioning details into this somehow...
// Setup<DockerDetails>(context =>
// {
//     // FIXME
//     var dockerDetails = GetDockerDetails(null);
//     return dockerDetails;
// });

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
    public string DefaultRepo { get;set; }
}

public DockerDetails GetDockerDetails(CustomBuildVersion buildVersion)
{
    DockerDetails ret = new DockerDetails();

    var repoDir = DirectoryPath.FromString(".");
    var buildNumber = "UNKNOWN";
    var semVer = "UNKNOWN";

    ret.ImageName = "UNKNOWN";
    ret.ImageDescription = "UNKNOWN";
    ret.ImageUrl = "UNKNOWN";

    if(buildVersion == null) {
        throw new ApplicationException("PFBuildVersion is missing in GetDockerDetails");
    }

    ProjectProperties props = LoadProjectProperties();
    if(props == null) {
        throw new ApplicationException("Error loading project properties file, does it exist?");
    }

    if(!string.IsNullOrEmpty(buildVersion.BuildNumber)) {
        buildNumber = buildVersion.BuildNumber;
    }
    if(!string.IsNullOrEmpty(buildVersion.SemVersion)) {
        semVer = buildVersion.SemVersion;
    }

    var tip = GitLogTip(repoDir);
    var currentBranch = GitBranchCurrent(repoDir);
    var buildDate = DateTime.Now;

    // Update DockerDetails
    if(!string.IsNullOrEmpty(props.ProjectCodeName)) {
        ret.ImageName = props.ProjectCodeName;
    }
    if(!string.IsNullOrEmpty(props.ProjectName)) {
        //ret.ImageName = props.ProjectName;
    }
    if(!string.IsNullOrEmpty(props.ProjectDescription)) {
        ret.ImageDescription = props.ProjectDescription;
    }
    if(!string.IsNullOrEmpty(props.ProjectUrl)) {
        ret.ImageUrl = props.ProjectUrl;
    }
    ret.GitUrl = currentBranch.Remotes.First().Url;
    
    // Cache build args
    var httpProxy = EnvironmentVariable("http_proxy");
    if(!string.IsNullOrEmpty(httpProxy)) {
        Information("Using HTTP_PROXY: "+httpProxy);
    }
    var httpsProxy = EnvironmentVariable("https_proxy");
    if(!string.IsNullOrEmpty(httpsProxy)) {
        Information("Using HTTPS_PROXY: "+httpsProxy);
    }
    var noProxy = EnvironmentVariable("no_proxy");
    if(!string.IsNullOrEmpty(noProxy)) {
        Information("Using NO_PROXY: "+noProxy);
    }

    // Update DockerDetails.BuildArguments
    var buildArgs = new string[] {
        "COMMIT_ID="+tip.Sha,
        "GIT_COMMIT="+tip.Sha,
        "GIT_BRANCH="+currentBranch.FriendlyName,
        "GIT_COMMITDATE="+GetRFCDate(tip.Committer.When.DateTime),
        "BUILD_NUMBER="+buildNumber,
        "GIT_URL="+ret.GitUrl,
        "BUILD_DATE="+GetRFCDate(buildDate),
        "BUILD_VERSION="+semVer,

        "IMAGE_NAME="+ret.ImageName,
        "IMAGE_DESC="+ret.ImageDescription,
        "IMAGE_URL="+ret.ImageUrl,

        "HTTP_PROXY="+httpProxy,
        "HTTPS_PROXY="+httpsProxy,
        "NO_PROXY="+noProxy
    };

    ret.BuildArguments = buildArgs;

    // Tags
    var cleanBranchName = currentBranch.FriendlyName.Replace("/", "-").Replace("\\", "-");

    ret.LocalTags =  new string[] { 
                    (props.DefaultUser + "/" + ret.ImageName+":"+semVer), 
                    (props.DefaultUser + "/" + ret.ImageName+":"+cleanBranchName),
    };
    ret.DefaultLocal = (props.DefaultUser + "/" + ret.ImageName+":"+semVer);

    ret.RemoteTags =  new string[] { 
                    props.DefaultRemote + "/" + props.DefaultUser + "/" + (ret.ImageName+":"+semVer), 
                    props.DefaultRemote + "/" + props.DefaultUser + "/" + (ret.ImageName+":"+cleanBranchName),
    };
    ret.DefaultRemote = props.DefaultRemote + "/" + props.DefaultUser + "/" + (ret.ImageName+":"+semVer);

    return ret;
}

private string Information(DockerDetails deets)
{
    StringBuilder b = new StringBuilder();

    b.AppendLine("ImageName: "+deets.ImageName);

    b.AppendLine("Args:");
    foreach(var arg in deets.BuildArguments)
    {
        b.AppendLine(arg);
    }

    return b.ToString();
}

Task("Build-Docker")
    .IsDependentOn("PFInit")
	.WithCriteria<CustomBuildVersion>((context, data) => data != null)
    .Does<CustomBuildVersion>(pfbuild =>
    {
        var dockerDetails = GetDockerDetails(pfbuild);
        Information("Docker build with args:");
        Information(dockerDetails);

        var dockerFilePath = File("./Dockerfile");
        var dockerFileDir = Directory(".");
        if(FileExists(dockerFilePath)) {
            var buildSettings = new DockerImageBuildSettings() { 
                BuildArg = dockerDetails.BuildArguments,
                Tag = dockerDetails.LocalTags.ToArray(),
            };
            Verbose("Using dockerfile: "+dockerFilePath);
            Verbose("Image: "+dockerDetails.ImageName);
            Verbose(dockerDetails.BuildArguments);
            DockerBuild(buildSettings, dockerFileDir);
        } else {
            Error("Docker file not found! Used: "+ dockerFilePath);
        }
    });

Task("Package-Docker")
    .IsDependentOn("PFInit")
    .IsDependentOn("Generate-Version-File-PF")
	.WithCriteria<CustomBuildVersion>((context, data) => data != null)
    .Does<CustomBuildVersion>(pfbuild =>
    {
        Information("Docker build with args:");
        var dockerDetails = GetDockerDetails(pfbuild);
        Information(dockerDetails);
        var sourceTag = dockerDetails.LocalTags.First();
        // Simply apply remote tags
         foreach(var tagRef in dockerDetails.RemoteTags)
         {
             Information("Packaging tag: "+tagRef);
             DockerTag(sourceTag, tagRef);
         }
    });

Task("Publish-PFDocker")
    .IsDependentOn("PFInit")
    .IsDependentOn("Generate-Version-File-PF")
	.WithCriteria<CustomBuildVersion>((context, data) => data != null)
    .Does<CustomBuildVersion>(pfbuild =>
    {
        var dockerDetails = GetDockerDetails(pfbuild);
        foreach(var tagRef in dockerDetails.RemoteTags)
        {
            var dockerPushSettings = new DockerImagePushSettings() {
            };
            Information("Pushing tag: "+tagRef);
            DockerPush(dockerPushSettings, tagRef);
        }
    });

Task("Publish-PFDockerReleaseInformation")
    .WithCriteria<CustomBuildVersion>((context, data) => data != null)
    .Does<CustomBuildVersion>(pfbuild => //make sure you use the right type parameter here
    {
        if(pfbuild == null) {
            throw new ApplicationException("PFBuildVersion param is null");
        }

        var relVer = new ReleaseVersion() {
            PackagePath = "UNKNOWN",
            PackageName = "UNKNOWN",
            PackageRepo = "UNKNOWN",
            Version = BuildParameters.Version.Version,
            SemVersion = BuildParameters.Version.SemVersion,
            BranchName = pfbuild.BranchName,
            CommitHash = pfbuild.CommitHash,
            CommitDate = pfbuild.CommitDate,
        };
        var props = LoadProjectProperties();
        //var propertiesFilePath = "./properties.json";
        //if(FileExists(propertiesFilePath)) {
        if(props != null) {
            // TODO: Rename props?
            relVer.PackageName = props.ProjectName;
            relVer.PackageRepo = props.DefaultRemote;
            relVer.PackagePath = $"docker://{props.DefaultRemote}/{props.DefaultUser}/{props.ProjectCodeName}:{relVer.SemVersion}";
        } else {
            throw new ApplicationException("properties.json file is missing or empty");
        }

        SaveReleaseVersion(relVer);

        if(BuildArtifactPath != null) {
            Information("Copying versioning to build artifact path: "+BuildArtifactPath);
            EnsureDirectoryExists(BuildArtifactPath);
            CopyFile("./ReleaseVersion.json", BuildArtifactPath+"/ReleaseVersion.json");
        } else {
            Error("No artifact path set!");
        }
	});


public DockerDetails GetDockerDetails(PFCustomBuildParams parms)
{
    DockerDetails ret = new DockerDetails();
    if(parms == null) {
        throw new ApplicationException("Null parms GetDockerDetails");
    }
    var BuildNumber = parms.BuildNumber;
    var props = parms.ProjectProps;
    var PFBuildVersion = parms.PFBuildVersion;

    var repoDir = DirectoryPath.FromString(".");
    var buildNumber = "UNKNOWN";
    var semVer = "UNKNOWN";

    // Defaults
    ret.ImageName = "UNKNOWN";
    ret.ImageDescription = "UNKNOWN";
    ret.ImageUrl = "UNKNOWN";

    // if(PFBuildVersion == null) {
    //     throw new ApplicationException("GetDockerDetails - PFBuildVersion is missing");
    // }

    if(props == null) {
        throw new ApplicationException("Error loading project properties file, does it exist?");
    }

    if(!string.IsNullOrEmpty(BuildNumber)) {
        buildNumber = BuildNumber;
    }
    if(PFBuildVersion != null && !string.IsNullOrEmpty(PFBuildVersion.SemVersion)) {
        semVer = PFBuildVersion.SemVersion;
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
        "IMAGE_URL="+ret.ImageUrl
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

Setup<DockerDetails>(context => 
{
    DockerDetails ret = null;
    try {
        Verbose("DockerDetails - Setup");
        if(context != null && context.Data != null)
        {
            var parms = context.Data.Get<PFCustomBuildParams>();
            if(parms == null) {
                Warning("DockerDetails - Couldn't get PFCustomBuildParams");
            } else {
                ret = GetDockerDetails(parms);
            }
        } else {
            Warning("DockerDetails - Couldn't get PFCustomBuildParams - context was null");
        }
    } catch(Exception ex) {
        Error("DockerDetails - Exception while setting up DockerDetails: " +ex.Dump());
    }
    return ret;
});

Task("ConfigureDockerDetails")
    .Does<DockerDetails>((context, vers) => {
    });

Task("Build-Docker")
    .Does<DockerDetails>((context, dockerDetails) => {

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
    .Does<DockerDetails>((context, dockerDetails) => {

        Information("Docker build with args:");
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
    //.IsDependentOn("PFInit")
    .IsDependentOn("Generate-Version-File-PF")
    .Does<DockerDetails>((context, dockerDetails) => {
        //var dockerDetails = GetDockerDetails(data);
        foreach(var tagRef in dockerDetails.RemoteTags)
        {
            var dockerPushSettings = new DockerImagePushSettings() {
            };
            Information("Pushing tag: "+tagRef);
            DockerPush(dockerPushSettings, tagRef);
        }
    });

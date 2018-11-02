public static ProjectProperties ProjectProps;

// TASKS
var initDone = 
            !string.IsNullOrEmpty("BUILD_NUMBER") 
            && BuildArtifactPath != null 
            && BuildNumber != null
            && ProjectProps != null;
Task("PFInit")
    .WithCriteria(!initDone)
    .IsDependentOn("Create-SolutionInfoVersion")
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

        var artifactPath = MakeAbsolute(Directory("./BuildArtifacts/")).FullPath;
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
    
public class ProjectProperties
{
    public string ProjectName { get; set; }   
    public string ProjectCodeName { get; set; }   
    public string ProjectDescription { get; set; }
    public string ProjectUrl { get; set; }
    public string SourceControlUrl { get; set; }

    // TODO: Rework to be more generic and non docker focussed
    public string DefaultUser { get;set; }
    public string DefaultRemote { get;set; }
    public string DefaultLocal { get;set; }
    public string DefaultTag { get;set; }

    // TODO: Probably split optionals out into a generic property bag
    public string TeamsWebHook { get;set; }
}

public static string ProjectPropertiesFileName = "properties.json";

public ProjectProperties LoadProjectProperties(DirectoryPath rootPath = null)
{
    if(rootPath == null)
        rootPath = Directory(".");
        
    ProjectProperties props = null;
    var propertiesFilePath = $"{rootPath.FullPath}/{ProjectPropertiesFileName}";
    if(FileExists(propertiesFilePath)) {
        var jsonData = String.Join(System.Environment.NewLine, System.IO.File.ReadAllLines(propertiesFilePath));
        props = JsonConvert.DeserializeObject<ProjectProperties>(jsonData);
    } else {
        Console.WriteLine($"Couldn't find properties file (using: '{propertiesFilePath}')");
    }
    return props;
}


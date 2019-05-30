
public static string BuildArtifactPath;
public static string BuildNumber;
public static CustomBuildVersion PFBuildVersion;

public static string BuildVersionFileName = "BuildVersion.json";
public static string GitVersionPropertiesFileName = "gitversion.properties";

Setup<CustomBuildVersion>(context =>
{
    var buildVersion = GetPFBuildVersion();
    return buildVersion;
});

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
    public string BuildId { get; set; }
    public string BuildNumber { get; set; }
    public string BuildUrl { get; set; }
}

public void SaveBuildVersion(CustomBuildVersion buildVer)
{        
    var versionFilePath = $"./{BuildVersionFileName}";
    if(buildVer != null) {
        var versionData = JsonConvert.SerializeObject(buildVer, Formatting.Indented);

        System.IO.File.WriteAllText(
            versionFilePath,
            versionData
            );
    } else {
        throw new ApplicationException("Tried to write null build version information");
    }
}

// TODO: Remove/replace with <PFBuildVersion> 
public CustomBuildVersion GetPFBuildVersion() 
{
    CustomBuildVersion PFBuildVersion;
    var vers = new CustomBuildVersion() {
        Version = BuildParameters.Version.Version,
        SemVersion = BuildParameters.Version.SemVersion,
        MajorMinorPatch = "0.0.0",
        Major = "0",
        Minor = "0",
        Patch = "0",
        Milestone = BuildParameters.Version.Milestone,
        InformationalVersion = BuildParameters.Version.InformationalVersion,
        FullSemVersion = BuildParameters.Version.FullSemVersion,
        BranchName = "UNKNOWN",
        CommitHash = "UNKNOWN",
        CommitDate = "UNKNOWN",
        BuildId = EnvironmentVariable("BUILD_NUMBER"),
        BuildUrl = EnvironmentVariable("BUILD_URL"),
    };
    // Pull from this if it exists
    if(FileExists($"./{GitVersionPropertiesFileName}")) {
        var props = ReadDictionaryFile($"./{GitVersionPropertiesFileName}");
        vers.MajorMinorPatch = props["GitVersion_MajorMinorPatch"];
        vers.Major = props["GitVersion_Major"];
        vers.Minor = props["GitVersion_Minor"];
        vers.Patch = props["GitVersion_Patch"];
        vers.BranchName = props["GitVersion_BranchName"];
        vers.CommitHash = props["GitVersion_Sha"];
        vers.CommitDate = props["GitVersion_CommitDate"];
    }
    var versionFilePath = $"./{BuildVersionFileName}";
    PFBuildVersion = vers;
    SaveBuildVersion(vers);

    if(BuildArtifactPath != null) {
        Information("Copying versioning to build artifact path: "+BuildArtifactPath);
        EnsureDirectoryExists(BuildArtifactPath);
        CopyFile(versionFilePath, BuildArtifactPath+$"/{BuildVersionFileName}");
    } else {
        Warning("No artifact path set, will not copy version to artifact path");
    }
    return (PFBuildVersion);
}

Task("Generate-Version-File-PF")
    // Sets up the artifact directory/build numbers
    .IsDependentOn("PFInit")    
    .Does(() => {
        PFBuildVersion = GetPFBuildVersion();
    });

public DirectoryPath GetVersioningBaseDirectory()
{
    var baseDir = MakeAbsolute(new DirectoryPath("."));
    var solnDir = MakeAbsolute(new DirectoryPath("./"+BuildParameters.SolutionDirectoryPath));
    var sourceDir = MakeAbsolute(new DirectoryPath("./"+BuildParameters.SourceDirectoryPath)); 

    if(DirectoryExists(sourceDir)) {
        var reason = "Source";
        baseDir = sourceDir;
        Information("Using versioning base directory of: "+baseDir+" ("+reason+")");
    } else if(DirectoryExists(solnDir)) {
        var reason = "Solution";
        baseDir = solnDir;
        Information("Using versioning base directory of: "+baseDir+" ("+reason+")");
    } else {
        var reason = "BaseDir";
        Information("Using versioning base directory of: "+baseDir+" ("+reason+")");
    }

    return baseDir;
}

// TODO: This doesn't seem to be created early enough for first builds to work, need to look into this
//.WithCriteria(BuildParameters.SourceDirectoryPath != null)
// TODO: Unsure why, but BuildParameters.SourceDirectoryPath seems to be null here, possibly due to being too early in process
Task("Create-SolutionInfoVersion")
	.Does(() => {
        var baseDir = GetVersioningBaseDirectory();
        if(BuildParameters.SolutionFilePath != null) {
            Information("Solution file path: "+BuildParameters.SolutionFilePath);
        } else {
            Information("Solution file path is null: "+BuildParameters.SolutionFilePath);
        }
        if(BuildParameters.SourceDirectoryPath != null) {
            Information("Source directory path: "+BuildParameters.SourceDirectoryPath);
        } else {
            Information("Source directory path is null: "+BuildParameters.SourceDirectoryPath);
        }

        if(baseDir != null && DirectoryExists(baseDir)) {
            Information("Checking versioning on solution path: "+baseDir);
            var solutionFilePath = MakeAbsolute(new FilePath(baseDir + "/SolutionInfo.cs"));
            if(!FileExists(solutionFilePath)) {
                Information("Creating missing SolutionInfo file: "+solutionFilePath);
                System.IO.File.WriteAllText(solutionFilePath.FullPath, "");

                // Need to regenerate versioning so that the SolutionInfo file is populated
                BuildParameters.SetBuildVersion(
                    BuildVersion.CalculatingSemanticVersion(
                        context: Context
                    )
                );
            }
        } else {
            Warning("Base directory was null?");
        }
    });

// TODO: Should have task to generate AssemblyTemplate from AssemblyInfo files...
// Basically need a task to set this scheme up from e.g. a new project in VS

Task("Generate-AssemblyInfo")
	.Does(() => {
		Information("Generate-AssemblyInfo started");
        var baseDir = GetVersioningBaseDirectory();

        // Read in solutioninfo
        var slnInfo = GetFiles(baseDir + "/SolutionInfo.cs").FirstOrDefault();
        if(slnInfo == null) {
            Error("No solution info file could be found");
            return;
        }
        var slnData = System.IO.File.ReadAllLines(slnInfo.FullPath);
        //Debug(slnData);

        // Find template files
        var templateFiles = GetFiles("./**/AssemblyTemplate.cs");
        foreach(var file in templateFiles)
        {
            Information("Generating assemblyinfo from template: "+file);
            // Read AssemblyTemplate
            var templateData = System.IO.File.ReadAllLines(file.FullPath);

            // Replace template with Solutioninfo items
            var assemblyData = new StringBuilder();
            foreach(var line in templateData)
            {
                var templateToken = "(\"TEMPLATE\")";
                if(line.Contains(templateToken)) {
                    var attr = line.Substring(0, Math.Min(line.Length, line.IndexOf(templateToken)));
                    var replLine = slnData.Where(p => p.StartsWith(attr)).FirstOrDefault();
                    Debug("Replacing: "+attr+" in template "+file+" with "+replLine);
                    assemblyData.AppendLine(replLine);
                } else {
                    assemblyData.AppendLine(line);
                }
            }

            // Save AssemblyInfo
            var assemblyDataLines = assemblyData.ToString().Split(
                new[] { System.Environment.NewLine },
                StringSplitOptions.None
            );
            var assemblyInfoPath = new FilePath(file.GetDirectory() + "/AssemblyInfo.cs");
            System.IO.File.WriteAllLines(assemblyInfoPath.FullPath, assemblyDataLines);
        }
    });

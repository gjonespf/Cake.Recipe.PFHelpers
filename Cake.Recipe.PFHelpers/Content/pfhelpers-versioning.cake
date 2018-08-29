
public static string BuildArtifactPath;
public static string BuildNumber;
public static CustomBuildVersion PFBuildVersion;

public static string BuildVersionFileName = "BuildVersion.json";
public static string GitVersionPropertiesFileName = "gitversion.properties";

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

Task("Generate-Version-File-PF")
    // Sets up the artifact directory/build numbers
    .IsDependentOn("PFInit")    
    .Does(() => {
    var props = ReadDictionaryFile($"./{GitVersionPropertiesFileName}");
    var versionFilePath = $"./{BuildVersionFileName}";

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
            BuildId = EnvironmentVariable("BUILD_NUMBER"),
            BuildUrl = EnvironmentVariable("BUILD_URL"),
        };
        PFBuildVersion = vers;
        SaveBuildVersion(vers);

        if(BuildArtifactPath != null) {
            Information("Copying versioning to build artifact path: "+BuildArtifactPath);
            EnsureDirectoryExists(BuildArtifactPath);
            CopyFile(versionFilePath, BuildArtifactPath+$"/{BuildVersionFileName}");
        } else {
            Error("No artifact path set!");
        }
    });

Task("Create-SolutionInfoVersion")
	.Does(() => {
        var solutionFilePath = MakeAbsolute(new FilePath(BuildParameters.SourceDirectoryPath + "/SolutionInfo.cs"));
        if(!FileExists(solutionFilePath)) {
            Information("Creating missing SolutionInfo file: "+solutionFilePath);
            System.IO.File.WriteAllText(solutionFilePath.FullPath, "");
        }
    });

// TODO: Should have task to generate AssemblyTemplate from AssemblyInfo files...
// Basically need a task to set this scheme up from e.g. a new project in VS

Task("Generate-AssemblyInfo")
	.Does(() => {
		Information("Generate-AssemblyInfo started");

        // Read in solutioninfo
        var slnInfo = GetFiles(BuildParameters.SourceDirectoryPath + "/SolutionInfo.cs").FirstOrDefault();
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

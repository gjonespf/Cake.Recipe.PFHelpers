#addin Newtonsoft.Json&version=12.0.2
using Newtonsoft.Json;

public static string BuildVersionFileName = "BuildVersion.json";
public static string GitVersionPropertiesFileName = "gitversion.properties";

public CustomBuildVersion GenerateCustomBuildVersion(ISetupContext context)
{
    Information("GenerateCustomBuildVersion");
    var versionFilePath = $"./{BuildVersionFileName}";

    // Try reading props if exists
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

    // Fall back on env vars if exists
    SaveBuildVersion(vers);

    return vers;
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

// Task("ConfigureCustomBuildVersion")
//     .Does<CustomBuildVersion>((context, vers) => {
//         Verbose("ConfigureCustomBuildVersion");
//     });

Task("Generate-Version-File-PF")
    .IsDependentOn("PFInit")
    .Does<PFCustomBuildParams>((context, parms) => {
        if(parms.BuildArtifactPath != null) {
            var versionFilePath = $"./{BuildVersionFileName}";
            Information("Copying versioning to build artifact path: "+parms.BuildArtifactPath);
            EnsureDirectoryExists(parms.BuildArtifactPath);
            CopyFile(versionFilePath, parms.BuildArtifactPath+$"/{BuildVersionFileName}");
        } else {
            Error("No artifact path set!");
        }
    });

Task("Create-SolutionInfoVersion")
    .IsDependentOn("PFInit")
    .Does(() => {
        // var solutionFilePath = MakeAbsolute(File("./SolutionInfo.cs"));
        // if(BuildParameters.SourceDirectoryPath != null && !string.IsNullOrEmpty(BuildParameters.SourceDirectoryPath)) {
        //     solutionFilePath = MakeAbsolute(File(string.Format("{0}/SolutionInfo.cs", BuildParameters.SourceDirectoryPath);
        // }
        var solutionFilePath = BuildParameters.SourceDirectoryPath.CombineWithFilePath("SolutionInfo.cs");
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

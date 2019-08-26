#r Newtonsoft.Json
using Newtonsoft.Json;

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
        
    // TODO: Some defaults from what we know?
    ProjectProperties props = new ProjectProperties(){

    };
    var propertiesFilePath = $"{rootPath.FullPath}/{ProjectPropertiesFileName}";
    if(FileExists(propertiesFilePath)) {
        var jsonData = String.Join(System.Environment.NewLine, System.IO.File.ReadAllLines(propertiesFilePath));
        props = JsonConvert.DeserializeObject<ProjectProperties>(jsonData);
    } else {
        Console.WriteLine($"Couldn't find properties file (using: '{propertiesFilePath}')");
    }
    return props;
}

// Setup<ProjectProperties>(setupContext => 
// {
//     try {
//         Verbose("ProjectProperties - Setup");
//         return LoadProjectProperties(null);
//     } catch(Exception ex) {
//         Error("ProjectProperties - Exception while setting up ProjectProperties: " +ex.Dump());
//         return null;
//     }
// });


// Task("ConfigureProjectProperties")
//     .Does<ProjectProperties>(props => {

//         // Load from project.json file if available
// });


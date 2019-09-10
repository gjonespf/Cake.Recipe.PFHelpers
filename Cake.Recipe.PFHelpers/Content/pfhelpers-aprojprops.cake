#addin Newtonsoft.Json&version=12.0.2
using Newtonsoft.Json;

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

// Now in 0setup
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


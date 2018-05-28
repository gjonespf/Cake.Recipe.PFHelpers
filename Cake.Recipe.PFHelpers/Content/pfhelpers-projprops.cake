
// Original Docker focussed
// public class ProjectProperties
// {
//     public string ImageName { get; set; }
//     public string ImageDescription { get; set; }
//     public string ImageUrl { get; set; }
//     public string GitUrl { get; set; }

//     public string DefaultUser { get;set; }
//     public string DefaultRemote { get;set; }
//     public string DefaultLocal { get;set; }
//     public string DefaultTag { get;set; }
// }

public class ProjectProperties
{
    public string ProjectName { get; set; }   
    public string ProjectCodeName { get; set; }   
    public string ProjectDescription { get; set; }
    public string ProjectUrl { get; set; }
    public string SourceControlUrl { get; set; }

    public string DefaultUser { get;set; }
    public string DefaultRemote { get;set; }
    public string DefaultLocal { get;set; }
    public string DefaultTag { get;set; }

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


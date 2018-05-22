//#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease
// #addin "nuget:https://www.nuget.org/api/v2?package=Newtonsoft.Json"
// using Newtonsoft.Json;
// #addin nuget:?package=Cake.Git&version=0.17.0

#load pfhelpers-addins.cake
#load pfhelpers-publish.cake
#load pfhelpers-versioning.cake
#load pfhelpers-npm.cake
#load pfhelpers-docker.cake

// TOOLS
public static void ForceDeleteDirectory(string path)
{
    var directory = new System.IO.DirectoryInfo(path) { Attributes = FileAttributes.Normal };

    foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
    {
        info.Attributes = FileAttributes.Normal;
    }

    directory.Delete(true);
}


public static IDictionary<string, string> ReadDictionaryFile(string fileName)
{
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    foreach (string line in System.IO.File.ReadAllLines(fileName))
    {
        if ((!string.IsNullOrEmpty(line)) &&
            (!line.StartsWith(";")) &&
            (!line.StartsWith("#")) &&
            (!line.StartsWith("'")) &&
            (line.Contains('=')))
        {
            int index = line.IndexOf('=');
            string key = line.Substring(0, index).Trim();
            string value = line.Substring(index + 1).Trim();

            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value.Substring(1, value.Length - 2);
            }
            dictionary.Add(key, value);
        }
    }

    return dictionary;
}

// TASKS
Task("PFInit")
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

        var artifactPath = MakeAbsolute(Directory("./.buildenv/"+buildNum)).FullPath;
        Information("Artifact path set to: "+artifactPath);
        BuildArtifactPath = artifactPath;
        BuildNumber = buildNum;
        EnsureDirectoryExists(BuildArtifactPath);
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

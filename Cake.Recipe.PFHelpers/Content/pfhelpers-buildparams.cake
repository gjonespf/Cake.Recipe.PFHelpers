

Task("Purge")
    .IsDependentOn("Clean")
    .Does(() =>
{
    GitClean(".");

    ForceDeleteDirectory("./.git/gitversion_cache/");
    ForceDeleteDirectory("./BuildArtifacts/");
    DeleteFiles("./tools/packages.config.md5sum");
    DeleteFiles("./gitversion.properties");
    DeleteFiles("./*version.json");

    // rm -Recurse tools/*
    // Note many if not all of these will be locked...
    var directories = GetDirectories("./tools/*");
    foreach(var directory in directories)
    {
        Information("Purging Directory: {0}", directory);
        try
        {
            ForceDeleteDirectory(directory.FullPath);
        }
        catch (System.Exception)
        {
            Information("Exception Purging Directory: {0}", directory);
        }
    }
});

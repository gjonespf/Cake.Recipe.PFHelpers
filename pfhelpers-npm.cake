

#addin "nuget:https://www.nuget.org/api/v2?package=Newtonsoft.Json"
using Newtonsoft.Json;

// NPM
Task("NPM-Restore")
    .Does(() => {
		Information("npm run prepare");
        var runPrepSettings = new NpmRunScriptSettings
        {
            ScriptName = "prepare",
        };
        NpmRunScript(runPrepSettings);
	}).Does(() => {
		Information("npm run restore");
        var runPrepSettings = new NpmRunScriptSettings
        {
            ScriptName = "restore",
        };
        NpmRunScript(runPrepSettings);
    });

Task("NPM-Build")
    .Does(() => {
        Information("npm run build-release");
        var runPrepSettings = new NpmRunScriptSettings
        {
            ScriptName = "build-release",
            //WorkingDirectory = BuildParameters.D
        };
        NpmRunScript(runPrepSettings);
    });    

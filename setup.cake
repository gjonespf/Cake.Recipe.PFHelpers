//#load "nuget:https://nuget.powerfarming.co.nz/api/odata/?package=Cake.Recipe.PF&version=0.3.4-alpha0027"
#load "nuget:https://nuget.powerfarming.co.nz/api/odata/?package=Cake.Recipe.PF&version=0.3.4-alpha0042"

#load setup.selfbootstrap.cake

var buildDefaultsFile = "./properties.json";

Environment.SetVariableNames();

// TODO: Load buildDefaultsFile as defaults, override with project.cake
#load project.cake

BuildParameters.PrintParameters(Context);
ToolSettings.SetToolSettings(context: Context);

RunTarget(BuildParameters.Target);
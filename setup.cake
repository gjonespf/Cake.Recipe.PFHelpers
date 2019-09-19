//#load "nuget:https://nuget.powerfarming.co.nz/api/odata/?package=Cake.Recipe.PF&version=0.3.4-alpha0027"
//#load "nuget:https://nuget.powerfarming.co.nz/api/odata?package=Cake.Recipe.PF&version=0.3.4-alpha0048"
#load "nuget:http://nuget-public.devinf.powerfarming.co.nz/api/v2?package=Cake.Recipe.PF&version=0.3.4-alpha0054"

#load setup.selfbootstrap.cake

var buildDefaultsFile = "./properties.json";

Environment.SetVariableNames();

// TODO: Load buildDefaultsFile as defaults, override with project.cake
#load project-tasks.cake
#load project.cake

BuildParameters.PrintParameters(Context);
ToolSettings.SetToolSettings(context: Context);

RunTarget(BuildParameters.Target);
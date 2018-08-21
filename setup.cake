#load "nuget:https://nuget.powerfarming.co.nz/api/odata/?package=Cake.Recipe.PF&version=0.3.1"
#load setup.selfbootstrap.cake

Environment.SetVariableNames();

#load project.cake

BuildParameters.PrintParameters(Context);
ToolSettings.SetToolSettings(context: Context);
RunTarget(BuildParameters.Target);
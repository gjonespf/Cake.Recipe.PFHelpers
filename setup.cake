#load "nuget:https://nuget.powerfarming.co.nz/api/odata?package=Cake.Recipe.PF&version=0.2.1"
#load setup.selfbootstrap.cake

var target = "Default";
Environment.SetVariableNames();

#load project.cake

BuildParameters.PrintParameters(Context);
ToolSettings.SetToolSettings(context: Context);

// Simplified...
Build.RunVanilla();

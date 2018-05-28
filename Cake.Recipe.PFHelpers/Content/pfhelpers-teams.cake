
var notifyTeamsHook = ProjectProps != null ? ProjectProps.TeamsWebHook : null;

Task("PublishNotify-NotifyTeams")
    .IsDependentOn("PFInit")    
    //.WithCriteria(!string.IsNullOrEmpty(notifyTeamsHook))
	.Does(() => {
        if(ProjectProps == null) {
            Information("Null props");
            return;
        }
        Information("TASK: PublishNotify Teams");
        var teamsWebhookUrl = ProjectProps.TeamsWebHook;
        var releaseVersion = LoadReleaseVersion();
        var messageCard = new MicrosoftTeamsMessageCard {
            summary = "Cake posted message using Cake.MicrosoftTeams",
            title = $"New packages for project {ProjectProps.ProjectName} published ({releaseVersion.SemVersion})",
            sections = new []{
                new MicrosoftTeamsMessageSection{
                    activityTitle = $"Cake PublishNotify for project {ProjectProps.ProjectName}",
                    activitySubtitle = "using Cake.MicrosoftTeams",
                    activityText = "The Cake Power Farming CICD process published a new package from updated source code",
                    activityImage = "https://raw.githubusercontent.com/cake-build/graphics/master/png/cake-small.png",
                    facts = new [] {
                        new MicrosoftTeamsMessageFacts { name ="ProjectName", value = ProjectProps.ProjectName },
                        new MicrosoftTeamsMessageFacts { name ="ProjectCodeName", value = ProjectProps.ProjectCodeName },
                        new MicrosoftTeamsMessageFacts { name ="ProjectDescription", value = ProjectProps.ProjectDescription },
                        new MicrosoftTeamsMessageFacts { name ="ProjectUrl", value = ProjectProps.ProjectUrl },
                        new MicrosoftTeamsMessageFacts { name ="PackageName", value = releaseVersion.PackageName },
                        new MicrosoftTeamsMessageFacts { name ="PackageRepo", value = releaseVersion.PackageRepo },
                        new MicrosoftTeamsMessageFacts { name ="PackageVersion", value = releaseVersion.SemVersion },
                        new MicrosoftTeamsMessageFacts { name ="PackageUrl", value = releaseVersion.PackageUrl },
                    },
                }
            },
            potentialAction = new [] {
                    new MicrosoftTeamsMessagePotentialAction {
                        name = "View build in Jenkins",
                        target = new []{"https://www.example.com"}
                    }
            }
        };

        System.Net.HttpStatusCode result = MicrosoftTeamsPostMessage(messageCard,
        new MicrosoftTeamsSettings {
            IncomingWebhookUrl = teamsWebhookUrl
        });
        Information("Result: "+result.ToString());

	});
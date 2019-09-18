#!/usr/bin/pwsh

#===============
# Functions
#===============
function Get-GitCurrentBranchVSTS {
    $currentBranch = ""
    if($env:Build_SourceBranchName) {
        Write-Host "Get-GitCurrentBranchVSTS - Got VSTS Source Branch: $($env:Build_SourceBranchName)"
        $currentBranch = $env:Build_SourceBranchName

        if($env:BUILD_SOURCEBRANCH) {
            $currentBranch = $env:BUILD_SOURCEBRANCH.substring($env:BUILD_SOURCEBRANCH.indexOf('/', 5) + 1)
            Write-Host "Get-GitCurrentBranchVSTS - Set to env BUILD_SOURCEBRANCH: $($currentBranch)"
        }
        if($env:SOURCEBRANCHFULLNAME) {
            $currentBranch = $env:SOURCEBRANCHFULLNAME
            Write-Host "Get-GitCurrentBranchVSTS - Set to env SOURCEBRANCHFULLNAME: $($currentBranch)"
        }
    }
    if($env:System_PullRequest_SourceBranch)
    {
        $prSrc = $($env:System_PullRequest_SourceBranch)
        $prSrc = $prSrc -replace "refs/heads/", ""
        
        Write-Host "Get-GitCurrentBranchVSTS - Got VSTS PR Source Branch: $prSrc"
        $currentBranch = $prSrc
    }
    $currentBranch
}

function Get-GitCurrentBranch {
    # Default git
    $currentBranch = (git symbolic-ref --short HEAD)

    # DevOps jiggery pokery
    $devopsBranch = Get-GitCurrentBranchVSTS
    if($devopsBranch) {
        $currentBranch = $devopsBranch
    }

    $currentBranch
}

function Get-GitLocalBranches {
    (git branch) | % { $_.TrimStart() -replace "\*[^\w]*","" }
}

function Get-GitRemoteBranches {
    (git branch --all) | % { $_.TrimStart() } | ?{ $_ -match "remotes/" }
}

function Remove-GitLocalBranches($CurrentBranch) {
    $branches = Get-GitLocalBranches
    foreach($branchname in $branches | ?{ $_ -notmatch "^\*" -and $_ -notmatch "$CurrentBranch" -and $_ -notmatch "master" -and $_ -notmatch "develop" }) {
        git branch -D $branchname.TrimStart()
    }
    #git remote update origin
    #git remote prune origin 
    git prune
    git fetch --prune
    git remote prune origin
}

function Invoke-GitFetchGitflowBranches($CurrentBranch) {
    git fetch origin master
    git fetch origin develop
    git checkout master
    git checkout develop
    git checkout $CurrentBranch
}

function Invoke-GitFetchRemoteBranches($CurrentBranch) {
    $remotes = Get-GitRemoteBranches
    $locals = Get-GitLocalBranches
    foreach($remote in $remotes) {
        $local = $remote -replace "remotes/origin/",""
        if($locals -notcontains $local) {
            git checkout $remote --track
        }
    }
    git checkout $CurrentBranch
}

function Install-NugetCaching() {
    # Enable nuget caching
    if($env:HTTP_PROXY) {
        $nuget = Get-Command nuget
        if($nuget)
        {
            Write-Host "Setting Nuget proxy to '$env:HTTP_PROXY'"
            & $nuget config -set http_proxy=$env:HTTP_PROXY
        }
        else {
            Write-Host "Couldn't find nuget to set cache"
        }
    }
}

function Clear-GitversionCache() {
    # GitVersion Issues with PR builds mean clearing cache between builds is worth doing
    if(Test-Path ".git/gitversion_cache") {
        Remove-Item -Recurse .git/gitversion_cache/* -ErrorAction SilentlyContinue | Out-Null
    }
    
    # Make sure we get new tool versions each build
    if(Test-Path "tools/packages.config.md5sum") {
        Remove-Item "tools/packages.config.md5sum"
        Get-ChildItem "tools/" -Exclude "tools/packages.config" -Hidden -Recurse | Remove-Item -Force
        Remove-Item "tools/*" -Recurse -Exclude "tools/packages.config"
    }
}

function Invoke-PreauthSetup([switch]$FetchBranches) {
    # TODO: Git fetch for gitversion issues
    # TODO: Module?
    try
    {
        if($env:GITUSER) 
        {
            Write-Host "GITUSER found, using preauth setup"
$preauthScript = @"
#!/usr/bin/pwsh
Write-Host "username=$($env:GITUSER)"
Write-Host "password=$($env:GITKEY)"
"@
            if($IsLinux) {
                $preauthScript = $preauthScript.Replace("`r`n","`n")
            }
            $preauthScript | Out-File -Encoding ASCII preauth.ps1
            $authPath = (Resolve-Path "./preauth.ps1").Path
            # git config --local --add core.askpass $authPath
            git config --local --add credential.helper $authPath
            if($IsLinux) {
                chmod a+x $authPath
            }
            # git config --local --add core.askpass "pwsh -Command { ./tmp/pre.ps1 -GitAuth } "
        } else {
            Write-Warning "No gituser found, pre fetch will fail if repo is private"
        }
        Remove-GitLocalBranches -CurrentBranch $currentBranch
        if($FetchBranches)
        {
            Invoke-GitFetchGitflowBranches -CurrentBranch $currentBranch
            Invoke-GitFetchRemoteBranches -CurrentBranch $currentBranch
        }

        Write-Host "Current branches:"
        git branch --all
    }
    catch {

    } finally {
        # Remove askpass config
        if($env:GITUSER) {
            # git config --local --unset-all core.askpass 
            git config --local --unset-all credential.helper
        }
        if(Test-Path ./preauth.ps1) {
            rm ./preauth.ps1
        }
    }
}

# TODO: Make this param/variable in project.json somehow?
function Invoke-NugetSourcesSetup()
{
    $nuget = Get-Command nuget -ErrorAction SilentlyContinue
    $pfRepoUrl = "$($env:LocalNugetServerUrl)"
    $pfRepoApiKey = "$($env:LocalNugetApiKey)"
    $pfRepoUser = "$($env:LocalNugetUserName)"
    $pfRepoPassword = "$($env:LocalNugetPassword)"

    if($IsLinux)
    {
        $linkExists = Get-ChildItem ~/.nuget/ | ?{ $_.LinkType -eq "SymbolicLink" -and $_.BaseName -eq "NuGet" }
        if(!$linkExists)
        {
            # Fix issues with mono/dotnet configs in
            # cat ~/.config/NuGet/NuGet.Config
            # cat ~/.nuget/NuGet/NuGet.Config
            # https://github.com/NuGet/Home/issues/4413
            Remove-Item ~/.nuget/NuGet -Recurse -ErrorAction SilentlyContinue
            ln -s ~/.config/NuGet/ ~/.nuget/NuGet/
            Remove-Item ~/.nuget/NuGet/nuget.config -ErrorAction SilentlyContinue
        }
    }

    if($nuget)
    {
        Write-Host "Checking Nuget sources '$pfRepoUrl'"

        if($pfRepoApiKey) {
            Write-Host "Setting PowerFarming.Nuget repo"
            nuget sources add -Name "PowerFarming Nuget" -source "$pfRepoUrl"
            #nuget setapikey "$pfRepoApiKey" -Source "$pfRepoUrl"
        } else {
            Write-Host "Credentials were missing, couldn't set up PowerFarming.Nuget Nuget Source Authentication"
        }
        # Needed if running Windows auth
        if($pfRepoUser) {
            Write-Host "Setting PowerFarming.Nuget repo user and password override"
            nuget sources update -Name "PowerFarming Nuget" -UserName "$pfRepoUser" -Password "$pfRepoPassword"
        }
    }

    # Try without override
    if($IsLinux -and $false) {
        # Set cake config var to main nuget one
        if(Test-Path "~/.config/NuGet/NuGet.Config") {
            Write-Host "Setting Cake nuget.config override"
            $env:CAKE_NUGET_CONFIGFILE = (Resolve-Path "~/.config/NuGet/NuGet.Config").Path
        }
    }

    $psRepo = Get-Command Get-PSRepository -ErrorAction SilentlyContinue
    $enablePSRepo = $false
    if($psRepo -and $enablePSRepo)
    {
        $psGallery = Get-PSRepository -Name "PSGallery"
        if($psGallery -and $psGallery.InstallationPolicy -ne "Trusted") {
            Write-Host "Trusting PSGallery PSRepository"
            Set-PSRepository -Name "PSGallery" -InstallationPolicy Trusted
        }

        # TODO: Set Authentication?
        $pfRepo = Get-PSRepository -Name "PowerFarming.Nuget" -ErrorAction SilentlyContinue
        if(!$pfRepo) {
            Write-Host "Registering PowerFarming.Nuget PSRepository"
            Register-PSRepository -Name "PowerFarming.Nuget" -SourceLocation "$pfRepoUrl"
            $pfRepo = Get-PSRepository -Name "PowerFarming.Nuget" -ErrorAction SilentlyContinue
        }
        if($pfRepo -and $pfRepo.InstallationPolicy -ne "Trusted") {
            Write-Host "Trusting PowerFarming.Nuget PSRepository"
            Set-PSRepository -Name "PowerFarming.Nuget" -InstallationPolicy Trusted
        }
        if($pfRepoApiKey) {
            $password = "$pfRepoApiKey" | ConvertTo-SecureString -asPlainText -Force
            $apiCreds = New-Object System.Management.Automation.PSCredential("apikey",$password)
            Set-PSRepository -Name "PowerFarming.Nuget" -Credential $apiCreds
        } else {
            Write-Host "Credentials were missing, couldn't set up PowerFarming.Nuget PSRepository Authentication"
        }
    }
}

function Invoke-CakeBootstrap() {
    if(Get-Command "dotnet-cake" -ErrorAction SilentlyContinue) {
        Write-Host "Running cake core bootstrap"
        dotnet-cake setup.cake --bootstrap
    }
}

#===============
# Main
#===============

# Useful missing vars
& "$PSScriptRoot/set-base-params.ps1"
$currentBranch = Get-GitCurrentBranch
$env:BRANCH_NAME=$env:GITBRANCH=$currentBranch
$isVSTSNode = $env:VSTS_AGENT
$isJenkinsNode = $env:JENKINS_HOME

# Install-PrePrerequisites
Install-NugetCaching
Clear-GitversionCache
# These are set up as part of the build.dntool.ps1 now...
# Install-DotnetBuildTools

if(!($isVSTSNode) -and !($isJenkinsNode)) {
    Invoke-PreauthSetup -FetchAllBranches:($isJenkinsNode) -CurrentBranch $currentBranch
} else {
    # Testing
    Invoke-PreauthSetup -FetchAllBranches:$false -CurrentBranch $currentBranch
}

Invoke-NugetSourcesSetup

# Nuget auth errors, ignore for now
# Invoke-CakeBootstrap

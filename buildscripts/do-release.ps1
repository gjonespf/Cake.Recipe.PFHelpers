#!/bin/pwsh
param([string]$ReleaseVersion, [switch]$FinaliseRelease) 
#TODO: Allow increments other than patch?

function Get-GitReleasesFromTags {
    $tags = $(git tag)
    $releaseTags = ($tags -match "[vV]?(\d+(\.\d+){1,3}).*")
    $releases = @()
    $releaseTags | ForEach-Object {
        $tag = $_
        $releaseVersion = [version]$tag
        $releases = $releases + $releaseVersion
    }
}

function Get-GitFlowVersionTagPrefix {
    $cfg = $(git flow config)
    $vercfg = $cfg | Where-Object { $_ -match "Version tag prefix" }
    $vercfg | Split-String ":" | Select-Object -l 1
}

function Get-GitFlowNewReleaseVersion ([string]$ReleaseVersion) {
    # Find release version or use supplied
    [version]$finalVersion = "0.0.0"
    try {
        # Get latest from git tags
        if(!($ReleaseVersion)) {
            $releases = Get-GitReleasesFromTags
            $lastRelease = $releases | Select-Object -l 1

            # Remember to increment version number, default patch
            $finalVersion = [version]"$($lastRelease.Major).$($lastRelease.Minor).$($lastRelease.Patch+1)"
            Write-Host "Using git version '$finalVersion' (last release version '$lastRelease')"

        } else {
            Write-Host "Using supplied version '$ReleaseVersion'"
        }
        [version]$finalVersion = $ReleaseVersion
    } catch {
        throw "Could not detect a valid version (using version text '$ReleaseVersion') - please manually suggest a version to use"
    }
}

function Invoke-RunProcessCatchErrors ([string]$Command, [switch]$ThrowOnErrors, [switch]$IgnoreErrors) {
    $expressionResult = Invoke-Expression "$Command 2>&1"
    if(($test | Where-Object { $_.writeErrorStream }) -and $ThrowOnErrors) {
        $stack = $test | Where-Object { $_.writeErrorStream }
        throw $stack
    }
    if($IgnoreErrors) {
        $expressionResult | Where-Object { !($_.writeErrorStream) }
    }
    else {
        $expressionResult
    }
}

Invoke-RunProcessCatchErrors -Command "git flow init -d" -ThrowOnErrors

# Remove "no release" as error
$currentRelease = Invoke-RunProcessCatchErrors -Command "git flow release" -IgnoreErrors

if($currentRelease -and !($FinaliseRelease)) {
    throw "Cannot continue, exisiting release '$currentRelease' and we're not fnalising.  If this release is no longer required, use command 'git flow release delete $currentRelease' otherwise run with param -FinaliseRelease"
}

$branches = $(git branch)
$currentBranch = $(git rev-parse --abbrev-ref HEAD)
$workingStatus = $(git status -s)

if($workingStatus) {
    Write-Error "Found changed files in working dir, cannot continue"
    Write-Error "$workingStatus"
    throw "Failing due to changed files in workspace"
}

if($currentBranch -notmatch "develop" -and $currentBranch -notmatch "master") {
    Write-Warning "Script will change to develop branch, break now if you don't wish this to happen or any key to continue"
    [console]::ReadKey($true)
}

[version]$finalVersion = Get-GitFlowNewReleaseVersion -ReleaseVersion $ReleaseVersion
if($finalVersion -eq "0.0.0") {
    throw "Error finding correct version for release, please manually specify using -ReleaseVersion"
}

if($FinaliseRelease) {
    Write-Warning "Script will now finalise release '$finalVersion'"
    Write-Host "Hit a key to action this release"
    [console]::ReadKey($true)

    Invoke-RunProcessCatchErrors -Command "git tag -a $ReleaseVersion -m ""Release $ReleaseVersion"" " -ThrowOnErrors
    Invoke-RunProcessCatchErrors -Command "git checkout master" -ThrowOnErrors
    Invoke-RunProcessCatchErrors -Command "git merge release/$ReleaseVersion" -ThrowOnErrors

    Write-Host "Hit a key to push this release"
    [console]::ReadKey($true)
    Invoke-RunProcessCatchErrors -Command "git push -v --progress --tags ""origin"" master:master" -ThrowOnErrors

    Write-Host "Hit a key to clean this release"
    [console]::ReadKey($true)
    Invoke-RunProcessCatchErrors -Command "git flow release delete $ReleaseVersion" -ThrowOnErrors
    Invoke-RunProcessCatchErrors -Command "git checkout develop" -ThrowOnErrors

} else {
    Write-Warning "Script will now set up release '$finalVersion'"
    
    Write-Host "Hit a key to action this release"
    [console]::ReadKey($true)

    #TODO: Prefix?
    $ReleaseVersion = "$finalVersion"

    Invoke-RunProcessCatchErrors -Command "git checkout develop" -ThrowOnErrors
    Invoke-RunProcessCatchErrors -Command "git pull" -ThrowOnErrors
    Invoke-RunProcessCatchErrors -Command "git flow release start $ReleaseVersion" -ThrowOnErrors
    # Push?
    #Invoke-RunProcessCatchErrors -Command "git push -v --progress --tags ""origin"" master:master" -ThrowOnErrors
}


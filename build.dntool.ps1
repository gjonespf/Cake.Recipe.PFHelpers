
# TODO: Prerequisites
[CmdletBinding()]
Param(
    [string]$Script = "setup.cake",
    [string]$Target = "build",
    [string]$Configuration,
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity,
    [switch]$ShowDescription,
    [Alias("WhatIf", "Noop")]
    [switch]$DryRun,
    [switch]$Experimental,
    [switch]$Mono,
    [version]$CakeVersion = '0.33.0',
    [switch]$UseNetCore = $true,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

if(!($env:PATH -match ".dotnet") -and $IsLinux) {
    if(Test-Path "~/.dotnet/tools") {
        $toolsPath = (Resolve-Path "~/.dotnet/tools").Path
        $env:PATH = $env:PATH + ":$toolsPath"
    } else {
        Write-Warning "Couldn't find dotnet core tools directory"
    }
}

# TODO: Until we can get this working, manually use Nuget pull
$brokenInternalNuget = $IsLinux
if($brokenInternalNuget)
{
    $env:CAKE_NUGET_USEINPROCESSCLIENT="false"
    $TOOLS_DIR = Join-Path $PSScriptRoot "tools"
    $NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
    $NUGET_URL = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    $PACKAGES_CONFIG = Join-Path $TOOLS_DIR "packages.config"
    $PACKAGES_CONFIG_MD5 = Join-Path $TOOLS_DIR "packages.config.md5sum"
    
    if (!(Test-Path $NUGET_EXE)) {
        Write-Verbose -Message "Downloading missing NuGet.exe..."
        $NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
        try {
            (New-Object System.Net.WebClient).DownloadFile($NUGET_URL, $NUGET_EXE)
        } catch {
            Throw "Could not download NuGet.exe."
        }
    }

    $NUGET_EXE = (Resolve-Path $NUGET_EXE).Path
    $ENV:NUGET_EXE = $NUGET_EXE
    Write-Verbose "Using nuget at: $NUGET_EXE"

    Write-Verbose "Restoring from Nuget"
    $NuGetOutput = Invoke-Expression "& nuget install -ExcludeVersion -PreRelease -OutputDirectory `"$TOOLS_DIR`" "
    #$NuGetOutput = Invoke-Expression "& $nugetMono `"$NUGET_EXE`" install -ExcludeVersion -PreRelease -OutputDirectory `"$TOOLS_DIR`" "


}

if(Get-Command "dotnet-cake" -ErrorAction SilentlyContinue) {
    Write-Information "Running dotnet-cake"
    dotnet-cake $Script --target=$Target
} else {
    Write-Error "Could not find dotnet-cake to run build script"
    Write-Information "Using PATH: $($env:PATH)"
}

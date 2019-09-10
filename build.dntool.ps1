
# TODO: Prerequisites
[CmdletBinding()]
Param(
    [string]$Script = "setup.cake",
    [string]$Target = "build",
    [string]$Configuration,
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity="Minimal",
    [switch]$ShowDescription,
    [Alias("WhatIf", "Noop")]
    [switch]$DryRun,
    [switch]$Experimental,
    #[version]$CakeVersion = '0.34.1',
    [version]$CakeVersion = '0.33.0',
    [string]$GitVersionVersion = '4.0.1-beta1-58',
    [string]$DotnetToolPath = '.dotnet/tools/',
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

$toolPathExists = Resolve-Path $DotnetToolPath -ErrorAction SilentlyContinue
if(!($toolPathExists)) {
    New-Item -ItemType Directory -Path $DotnetToolPath | Out-Null
}

function New-DotnetToolDefinition ($PackageId, $Version, $CommandName) {
    [pscustomobject] @{
        PackageId = $PackageId
        Version = $Version
        CommandName = $CommandName
    }
}

# Check existing versions...
$toollist = (dotnet tool list --tool-path $DotnetToolPath)
$toolvers = ($toollist | Select-Object -Skip 2) | %{ $packageId, $version, $commandname = $_.Trim() -split '\s+'
    [pscustomobject] @{
        PackageId = $packageId
        Version = $version
        CommandName = $commandname
    }
}
# TODO: Load from a json config as alternative?
$defaultToolVers = @(
    New-DotnetToolDefinition -PackageId "cake.tool" -Version $CakeVersion -CommandName "dotnet-cake"
    New-DotnetToolDefinition -PackageId "gitversion.tool" -Version $GitVersionVersion -CommandName "dotnet-gitversion"
)

foreach($tool in $defaultToolVers) {
    # New install
    if(!($toolvers | ?{ $_.PackageId -eq $tool.PackageId })) {
        Write-Information "Installing missing tool $($tool.PackageId)"
        dotnet tool install $tool.PackageId --tool-path $DotnetToolPath --version $tool.Version
    }
    $dotnetvers = [version](dotnet --version)
    # Update version (update to specific version coming in dotnet sdk 3 :/)
    if($dotnetvers.Major -le 2 -and ($toolvers | ?{ $_.PackageId -eq $tool.PackageId -and $_.Version -ne $tool.Version })) {
        Write-Information "Installing updated tool $($tool.PackageId)"
        dotnet tool uninstall $tool.PackageId --tool-path $DotnetToolPath
        dotnet tool install $tool.PackageId --tool-path $DotnetToolPath --version $tool.Version
    }
}

# Ensure we use the specific version we asked for
$dotnetcake = $DotnetToolPath + "/dotnet-cake"
if($IsWindows -or $env:windir) {
    $dotnetcake = $DotnetToolPath + "/dotnet-cake.exe"
}
if(Test-Path $dotnetcake) {
    $dotnetcake = (Resolve-Path $dotnetcake).Path

    Write-Information "Running dotnet-cake"
    & $dotnetcake $Script --target=$Target --verbosity=$Verbosity

} else {
    Write-Error "Could not find dotnet-cake to run build script"
    Write-Information "Using PATH: $($env:PATH)"
}


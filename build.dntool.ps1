
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

dotnet-cake $Script --target=$Target

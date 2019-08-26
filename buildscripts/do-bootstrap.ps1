#!/bin/pwsh
param([string]$TemplateUrl) 
# TODO: Allow this file to pull down templates?

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

$status = (git status -s)
if($status) {
    throw "DANGER: Please stash or backup any changes before running this script.  Failing here to ensure files survive."
}

$branches = $(git branch)
if(!($branches -match "[\\w]?develop")) {
    Write-Warning "Develop branch is missing, press any key to create this branch based on master"
    [console]::ReadKey($true)

    git checkout master
    git checkout -b develop
    git push origin develop
}

Invoke-RunProcessCatchErrors -Command "git flow init -d" -ThrowOnErrors

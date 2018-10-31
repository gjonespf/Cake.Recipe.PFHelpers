#!/usr/bin/pwsh

#===============
# Functions
#===============

function Get-GitCurrentBranch {
    (git symbolic-ref --short HEAD)
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

function Invoke-PreAuthScript([switch]$Cleanup) {

    if($Cleanup) {
        if(Test-Path ./preauth.ps1) {
            rm ./preauth.ps1 -ErrorAction SilentlyContinue
        }
        $env:GIT_ASKPASS=""
        return
    }

    $preauthScript = @"
#!/usr/bin/pwsh
Write-Host "username=$($env:GITUSER)"
Write-Host "password=$($env:GITKEY)"
"@
    if($IsLinux) {
        $preauthScript = $preauthScript.Replace("`r`n","`n")
    }
    $preauthScript | Out-File -Encoding ASCII preauth.ps1 
    $authPath = (Resolve-Path "./preauth.ps1").Path.Replace("\", "/")
    # git config --local --add core.askpass $authPath
    # git config --local --add credential.helper $authPath
    $env:GIT_ASKPASS="$authPath"
    if($IsLinux) {
        chmod a+x $authPath
    }
}

#===============
# Main
#===============

# Useful missing vars
$currentBranch = Get-GitCurrentBranch
$env:BRANCH_NAME=$env:GITBRANCH=$currentBranch

# Bugs in GitTools...
$env:IGNORE_NORMALISATION_GIT_HEAD_MOVE=1

# GitVersion Issues with PR builds mean clearing cache between builds is worth doing
if(Test-Path ".git/gitversion_cache") {
    Remove-Item -Recurse .git/gitversion_cache/* -ErrorAction SilentlyContinue | Out-Null
}

# Make sure we get new tool versions each build
if(Test-Path "tools/packages.config.md5sum") {
    Remove-Item "tools/packages.config.md5sum"
}

# TODO: Git fetch for gitversion issues
# TODO: Module?
try
{
    if($env:GITUSER) 
    {
        Write-Host "GITUSER found, using preauth setup"
        Invoke-PreAuthScript

        # git config --local --add core.askpass "pwsh -Command { ./tmp/pre.ps1 -GitAuth } "
    } else {
        Write-Warning "No gituser found, pre fetch will fail if repo is private"
    }
    Remove-GitLocalBranches -CurrentBranch $currentBranch
    Invoke-GitFetchGitflowBranches -CurrentBranch $currentBranch
    Invoke-GitFetchRemoteBranches -CurrentBranch $currentBranch

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
    Invoke-PreAuthScript -Cleanup
}

# SIG # Begin signature block
# MIIIvQYJKoZIhvcNAQcCoIIIrjCCCKoCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQURnJSj6I3+qHAPsRPrEeggeP4
# YfWgggYLMIIGBzCCBO+gAwIBAgIKOLg6zgAAAAACdTANBgkqhkiG9w0BAQUFADBt
# MRIwEAYKCZImiZPyLGQBGRYCbnoxEjAQBgoJkiaJk/IsZAEZFgJjbzEcMBoGCgmS
# JomT8ixkARkWDHBvd2VyZmFybWluZzElMCMGA1UEAxMccG93ZXJmYXJtaW5nLVBG
# TlotU1JWLTAyOC1DQTAeFw0xODAyMDgyMjQyNTVaFw0xOTAyMDgyMjQyNTVaMIGJ
# MRIwEAYKCZImiZPyLGQBGRYCbnoxEjAQBgoJkiaJk/IsZAEZFgJjbzEcMBoGCgmS
# JomT8ixkARkWDHBvd2VyZmFybWluZzEeMBwGA1UECxMVTmV3IFplYWxhbmQgV2hv
# bGVzYWxlMQswCQYDVQQLEwJJVDEUMBIGA1UEAxMLR2F2aW4gSm9uZXMwggEiMA0G
# CSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCIYJoWDmbUQhW5zrDYo2Vvw1x9lxxT
# SdyFdZpMr81XHhe+MDCmVa1qMKrb+7weBhUMq/gvywWfOkR+FIdy9vyW2dFGgxd7
# 5iSIA1lDnGWv8OIFICtlGv6BiDSkhb+r5KGnF4/SbH+MnSesRjtWZfQZR9spKvAm
# mExVyKtKv1EYROsSbIVN5g5AF6sNwQs0sgBMODe51YCiCGjW5g5KjuQv+B0vy7G8
# k6HeUQ6aThhfV/ra5s/Ou1KPJjVr0rAho2pYl8UoZDmw6cQGtkVfp5Y95FhTEaJP
# g85VGdtNoiRkbzrF/0mGFFMPsSONJDAW07T5KqukXKxz01aAONs+/ta7AgMBAAGj
# ggKKMIIChjAdBgNVHQ4EFgQUwnv3rIm8lrgsvpUoRsUYVhhA/hIwHwYDVR0jBBgw
# FoAUwyl94sUGMISSqn1Dg21DQS+Tk4EwgekGA1UdHwSB4TCB3jCB26CB2KCB1YaB
# 0mxkYXA6Ly8vQ049cG93ZXJmYXJtaW5nLVBGTlotU1JWLTAyOC1DQSxDTj1QRk5a
# LVNSVi0wMjgsQ049Q0RQLENOPVB1YmxpYyUyMEtleSUyMFNlcnZpY2VzLENOPVNl
# cnZpY2VzLENOPUNvbmZpZ3VyYXRpb24sREM9cG93ZXJmYXJtaW5nLERDPWNvLERD
# PW56P2NlcnRpZmljYXRlUmV2b2NhdGlvbkxpc3Q/YmFzZT9vYmplY3RDbGFzcz1j
# UkxEaXN0cmlidXRpb25Qb2ludDCB2AYIKwYBBQUHAQEEgcswgcgwgcUGCCsGAQUF
# BzAChoG4bGRhcDovLy9DTj1wb3dlcmZhcm1pbmctUEZOWi1TUlYtMDI4LUNBLENO
# PUFJQSxDTj1QdWJsaWMlMjBLZXklMjBTZXJ2aWNlcyxDTj1TZXJ2aWNlcyxDTj1D
# b25maWd1cmF0aW9uLERDPXBvd2VyZmFybWluZyxEQz1jbyxEQz1uej9jQUNlcnRp
# ZmljYXRlP2Jhc2U/b2JqZWN0Q2xhc3M9Y2VydGlmaWNhdGlvbkF1dGhvcml0eTAl
# BgkrBgEEAYI3FAIEGB4WAEMAbwBkAGUAUwBpAGcAbgBpAG4AZzALBgNVHQ8EBAMC
# B4AwEwYDVR0lBAwwCgYIKwYBBQUHAwMwNAYDVR0RBC0wK6ApBgorBgEEAYI3FAID
# oBsMGWdqb25lc0Bwb3dlcmZhcm1pbmcuY28ubnowDQYJKoZIhvcNAQEFBQADggEB
# ALD0yEldMFKKvK34ChFdWLuU1W4eACG2mLGXskeP6UsmuJmY1mpJVr/x6tQosBvV
# RnNxyy3nvuyZVD071R7FxOAt07CyovDToJRBPRP6biBKDg1mQCN0eWMoAlVmIfjx
# iYCKU+TN6mKn+wgVTHlPpgoFkY17h8PuzYX33bfM2V4aKcDnFIOZFSTbLa+QkBSP
# /KF/6hsYQC9AGSczqAqqjCk6mL1paqMYcAME301OAPuNrOyPn5nr1e2ZruWkup/D
# /UKSvkEPlOR2cNYVBGvta82Jyh0SYWm8S2e+b6bLt71xwU6qAylDNDu9ZgvowkXp
# Tdg9K3nCPLybNxqLua3lKnYxggIcMIICGAIBATB7MG0xEjAQBgoJkiaJk/IsZAEZ
# FgJuejESMBAGCgmSJomT8ixkARkWAmNvMRwwGgYKCZImiZPyLGQBGRYMcG93ZXJm
# YXJtaW5nMSUwIwYDVQQDExxwb3dlcmZhcm1pbmctUEZOWi1TUlYtMDI4LUNBAgo4
# uDrOAAAAAAJ1MAkGBSsOAwIaBQCgeDAYBgorBgEEAYI3AgEMMQowCKACgAChAoAA
# MBkGCSqGSIb3DQEJAzEMBgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgor
# BgEEAYI3AgEVMCMGCSqGSIb3DQEJBDEWBBSQ6h49joR/60m7AVqTdh8W1h6uETAN
# BgkqhkiG9w0BAQEFAASCAQAaxrWcSVQErjNeBAKLyGAOuRJ/iIvQZyVPVLiAcSo1
# yca+UueS96VFMddVEW0QgXlAprU8HrnVn7njwQmx16uNUS6ngV+wHzQOwdsaEZuT
# vsRWsn09R9dbF+oEevmX3t9m+USJvR70Btc4jhkncMmOSqYniXJz3L485nze56Vv
# js8bHfZCQGfrZsCKfN29DB62WUuvui8g2bRJzAxewn9QDisgcSa4puzaixYRbPIX
# vKD7Ki80hTc4ZrfaiXWpuXJU3uLpiGnDQyp4rhUj+Q78bH+pJAQvGsmCt1tLX2Ss
# bwFF2In2C3BrLU1vzfWWGoG852gQU08MaRuJhTNR1Bql
# SIG # End signature block

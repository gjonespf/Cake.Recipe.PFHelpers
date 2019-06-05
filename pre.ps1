#!/usr/bin/pwsh

# Common preinit for all projects
. ./buildscripts/do-preinit.ps1

# Do your project specific pre stuff here

# Ignore this GitTools issue for now
$env:IGNORE_NORMALISATION_GIT_HEAD_MOVE="1"



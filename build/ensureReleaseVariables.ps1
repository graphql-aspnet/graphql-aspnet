# *************************************************************
# project:  graphql-aspnet
# --
# repo: https://github.com/graphql-aspnet
# docs: https://graphql-aspnet.github.io
# --
# License:  MIT
# *************************************************************

# Package Release Validation Script
# -------------------------------------
# This script is executed as a step in a release pipeline 
# as a safety check against the team member initiating the automated deployment.
#
# It serves as a check for consistancy between the artifacts created during a build 
# and those published to nuget.org on the release trigger.
#
# The team member is prompted to choose a build from the pipeline container 
# and prompted for a version number and suffix when starting the release.
#
# This script checks that the actual files (the .nupkg) in the build artifacts 
# match the version typed by the team member doing the deployment. 

$versionSuffix = $env:versionSuffix   # e.g. "beta"  or ""
$versionNumber = $env:versionNumber  # e.g. "0.5.1"
$packageId = $env:packageId          # e.g.  "GraphQL.AspNet|GraphQL.AspNet.Subscriptions"
$artifactDirectory = $env:artifactDirectory  

# ------------------------------
# Validate the Version Number
# -------------------------------
# when version number validation fails fail the release
write-host "Validating Version Number: " -NoNewline
if ( ($versionNumber -eq "") -or ($null -eq $versionNumber)) {
    write-host "`"`" (ERROR!)." -ForegroundColor Red
    write-host "No Version Number was supplied to the release. Package Deployment Halted."  -ForegroundColor Red
    exit 1
}

## concat in the suffix (optional)
# with suffix:     1.0.0-beta
# without suffix:  1.0.0
if ( ($versionSuffix -ne "") -and ($null -ne $versionSuffix) ) {
    $versionNumber = "$versionNumber-$versionSuffix"
}

write-host  "$versionNumber (Done)." -ForegroundColor Green

$packagesToCheck = $packageId.Split("|", [System.StringSplitOptions]::None)
foreach ($packageToCheck in $packagesToCheck) {
    # ------------------------------
    # Validate the Package Id
    # -------------------------------
    # package Id must be supplied, and will be checked against the nupkg file name(s)
    write-host "Validating Package Id: " -NoNewline
    if ( ($packageToCheck -eq "") -or ($null -eq $packageToCheck)) {
        write-host "(ERROR!)"  -ForegroundColor Red
        write-host "No Package Id was supplied to the release. Package Deployment Halted."  -ForegroundColor Red
        exit 1
    }
    write-host "$packageToCheck (Done)" -ForegroundColor Green

    # ------------------------------
    # Check that a nuget file of the correct name exists
    # -------------------------------
    $fileName = "$packageToCheck.$versionNumber.nupkg"
    write-host "Checking for expected artifact: " -NoNewline
    write-host $fileName -ForegroundColor Green -NoNewline
    $matchedFiles = get-childitem "$artifactDirectory" -Recurse -Include $fileName | Select-Object -Expand FullName
    if ($null -eq $matchedFiles) {
        write-host " (ERROR!)" -ForegroundColor Red
        write-host "Expected File Not Found. Build Terminated." -ForegroundColor Red
        exit 1
    }
    write-host " (Done)" -ForegroundColor Green
}


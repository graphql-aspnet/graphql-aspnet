# *************************************************************
# project:  graphql-aspnet
# --
# repo: https://github.com/graphql-aspnet
# docs: https://graphql-aspnet.github.io
# --
# License:  MIT
# *************************************************************

# Package Creation Validation Script
# -------------------------------------
# This script is executed as a step in a production build for release to nuget.org.
# It ensures that the parameters defined in the build are acceptable such that 
# the package being built "could be" deployed to nuget.

$versionNumber = $env:VersionNumber
$versionSuffix = $env:VersionSuffix
$packageId = $env:PackageId
$branch = $env:Branch 
$packageData = $null;

write-host ""
write-host "--------------"

# ------------------------------
# Validate the Current Branch
# -------------------------------
# This script is executed as part of the CI steps for protected branches, which is automatic. 
# When branch validation fails, don't fail the script just exit cleanly and don't build artifacts.
write-host "Validating Branch: " -NoNewline
if ( ($branch -eq "") -or ($null -eq $branch )) {
    write-host "(WARN)" -ForegroundColor Yellow
    write-host "No branch was supplied to the build. Ensure the source branch variable is correctly set."  -ForegroundColor Red
    Write-Host "##vso[task.setvariable variable=deployment.buildArtifacts;]skipped"
    exit 0
}

# only "master" and "release/*" branches can be packaged for deployment
if ( ($branch.StartsWith("refs/heads/master") -eq $false) -and ($branch.StartsWith("refs/heads/release") -eq $false )) {
    write-host "$branch  (WARN)" -ForegroundColor Yellow
    write-host "The branch '$branch' is not valid for artifact creation. Only 'master' and 'release/*' branches can be shipped."  -ForegroundColor Red
    Write-Host "##vso[task.setvariable variable=deployment.buildArtifacts;]skipped"
    exit 0
}

write-host $branch -NoNewline
write-host  " (Done)." -ForegroundColor Green

# ------------------------------
# Validate the Version Number
# -------------------------------
# When branch validation fails, don't fail the script just exit cleanly and don't build artifacts.
write-host "Validating Version Number: " -NoNewline
if ( ($versionNumber -eq "") -or ($null -eq $versionNumber)) {
    write-host "`"`" (WARN)." -ForegroundColor Yellow
    write-host "No Version Number was supplied to the build. Package Deployment Skipped."  -ForegroundColor Yellow
    write-host "##vso[task.setvariable variable=deployment.buildArtifacts;]skipped"
    exit 0
}

## concat in the suffix (optional)
# with suffix:     1.0.0-beta
# without suffix:  1.0.0
if ( ($versionSuffix -ne "") -and ($null -ne $versionSuffix) ) {
    $versionNumber = "$versionNumber-$versionSuffix"
}

write-host  "$versionNumber (Done)." -ForegroundColor Green

# ------------------------------
# Validate the Package Id
# -------------------------------
write-host "Validating Package Id: " -NoNewline
if ( ($packageId -eq "") -or ($null -eq $packageId)) {
    write-host "(ERROR!)"  -ForegroundColor Red
    write-host "No Nuget Package id was supplied to the build. Ensure the 'deployment.packageId' variable is set in the release pipeline."  -ForegroundColor Red
    exit 1
}

# ------------------------------
# Validate Published Versions for Conflicts
# ------------------------------
# fetch the metadata for the library as it exists on nuget.org right now
$normalizedPackageId = $packageId.ToLower()
$url = "https://api.nuget.org/v3/registration3-gz-semver2/$normalizedPackageId/index.json"
try {
    $packageData = Invoke-RestMethod -Uri $url 
}
catch {
    write-host "$packageId (ERROR!)" -ForegroundColor Red
    write-host "Unable to retrieve package details from: $url" -ForegroundColor Red
    write-host "Either a connection was not established or the package does not exist." -ForegroundColor Red
    exit 1
}

write-host $packageId -NoNewline
write-host  " (Done)." -ForegroundColor Green

# ensure the version that is being built doesn't already exist on nuget.
write-host "--------------"
write-host "Analyzing published Nuget packages for version conflicts with this release..."
write-host "Checking Existing Packages for: " -NoNewline
write-host $packageId -ForegroundColor DarkCyan

$packageData.items[0].items | ForEach-Object {            
    $packageContent = Invoke-RestMethod -Uri $_.catalogEntry.'@id'
    write-host "Inspecting: $($packageContent.version), Published: $($packageContent.created): " -NoNewline
    if ( $packageContent.version -eq $versionNumber ) {
        write-host "ERROR!"  -ForegroundColor Red
        write-host "Package version '$versionNumber' already exists. Unable to redeploy the same version number." -ForegroundColor Red
        exit 1
    }
    else {
        write-host  " (Done)." -ForegroundColor Green
    }
}

write-host "Validation Complete. No conflicts found. Continuing to deploy new version: $versionNumber" -ForegroundColor Green
write-host "--------------"

## write the "success" indicator into the build pipeline
Write-Host "##vso[task.setvariable variable=deployment.fullVersionId;]$versionNumber"
Write-Host "##vso[task.setvariable variable=deployment.buildArtifacts;]allowed"

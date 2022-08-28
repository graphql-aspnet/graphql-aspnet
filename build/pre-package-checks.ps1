# This script is executed BEFORE a package is created 
# and before its published to nuget.
#
# This script performs some checks to ensure the package can be successfully built 
# and configured correctly
param (
    [String] $versionNumber,    #e.g.  "0.5.1"    
    [String] $versionSuffix,    #e.g. "beta" or ""    
    [String] $csProjFile        # ./bobs-fishing.csproj
)

write-host "Executing Preflight checks for Nuget Package deployment"

# ------------------------------
# Validate the Version Number
# -------------------------------
# when version number validation fails fail the release
write-host "Validating Version Number Format: " -NoNewline
if ( ($versionNumber -eq "") -or ($null -eq $versionNumber)) {
    write-host "(ERROR!)." -ForegroundColor Red
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

# ------------------------------
# Validate Published Versions for Conflicts
# ------------------------------

# figure out what the full package id would be
$projIdFinder = "$PsScriptRoot\retrievePackageId.ps1"
[string]$packageToCheck =  & "$projIdFinder" -csProjFile  $csProjFile 
if($packageToCheck -eq 1 -or $null -eq $packageToCheck -or "" -eq ($packageToCheck.Trim()) ){
    return 1
}

write-host "Checking Existing Packages for: " -NoNewline
write-host $packageToCheck -ForegroundColor DarkCyan

# fetch the metadata for the library as it exists on nuget.org right now
$normalizedPackageId = $packageToCheck.ToLower()
$url = "https://api.nuget.org/v3/registration5-semver1/$normalizedPackageId/index.json"
try {
    $packageData = Invoke-RestMethod -Uri $url 
}
catch {
    write-host "ERROR!" -ForegroundColor Red
    write-host "Unable to retrieve package details from: $url" -ForegroundColor Red
    write-host "Either a connection was not established or the package does not exist." -ForegroundColor Red
    exit 1
}

# Inspect all existing package names and ensure the version 
# that is being built doesn't already exist on nuget.
$packageData.items[0].items | ForEach-Object {            
    $packageContent = Invoke-RestMethod -Uri $_.catalogEntry.'@id'
    write-host "Inspecting  Package: $($packageContent.version), Published: $($packageContent.created): " -NoNewline
    if ( $packageContent.version -eq $versionNumber ) {
        write-host "ERROR!"  -ForegroundColor Red
        write-host "Package version '$versionNumber' already exists. Unable to redeploy the same version number." -ForegroundColor Red
        exit 1
    }
    else {
        write-host  " (Done)." -ForegroundColor Green
    }
}

write-host "No Conflicts Found"
write-host "Pre-flight Validation Complete" -ForegroundColor Green
exit 0

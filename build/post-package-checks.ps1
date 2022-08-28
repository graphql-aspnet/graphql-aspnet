# This script is executed AFTER a package is created. It performs some  
# checks to ensure the package was correctly built according to the expected parameters.

param (
    [String] $versionNumber,      #  "0.5.1"
    [String] $versionSuffix,      #  "beta" or ""
    [String] $csProjFile,         #  bobs-fishing.csproj
    [String] $outputDirectory   # ./output
)

# ------------------------------
# Validate the Version Number
# -------------------------------
write-host "Validating Version Number: " -NoNewline
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
# Check that the package was created with the correct version number
# ------------------------------

# deteremine the package id
$projIdFinder = "$PsScriptRoot\retrievePackageId.ps1"
[string]$packageToCheck =  & "$projIdFinder" -csProjFile  $csProjFile 
if($packageToCheck -eq 1 -or $null -eq $packageToCheck -or "" -eq ($packageToCheck.Trim()) ){
    exit 1
}

write-host "Processing Package Id: " -NoNewline
write-host $packageToCheck -ForegroundColor DarkCyan

# check the output folder for a package matching the version number
$fileName = "$packageToCheck.$versionNumber.nupkg"
write-host "Searching Artifact Directory: $outputDirectory"
write-host "Checking for expected artifact: $fileName ..." -NoNewline
$matchedFiles = get-childitem "$outputDirectory" $fileName | Select-Object -Expand FullName
if ($null -eq $matchedFiles) {
    write-host "(ERROR!)" -ForegroundColor Red
    write-host "ERROR: Expected File Not Found. Build Terminated." -ForegroundColor Red
    exit 1
}
write-host " (Done)" -ForegroundColor Green

write-host "Post-Package Validation Complete" -ForegroundColor Green
exit 0
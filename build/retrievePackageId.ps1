# Inspects the XML of any CS project file 
# and returns the first found instance of <PackageId>

param(
    [String] $csProjFile   # './src/folder/some-file.csproj'
)

write-host "Determining PackageId: $csProjFile"
if(!(Test-Path $csProjFile)){
    write-host 'ERROR: Unable to Determine Package Id' -ForegroundColor Red
    write-host 'ERROR: Missing Project File' -ForegroundColor Red
    exit 1
}


[xml]$fileData =  Get-Content -Path $csProjFile
[string]$packageId = $fileData.Project.PropertyGroup.PackageId

if($null -eq $packageId){
    write-host 'ERROR: Unable to Determine Package Id' -ForegroundColor Red
    write-host "ERROR: <PackageId> element not found"  -ForegroundColor Red
    exit 1
}

$packageId = $packageId.Trim()
if("" -eq $packageId){
    write-host 'ERROR: Unable to Determine Package Id' -ForegroundColor Red
    write-host "ERROR: <PackageId> element is blank"  -ForegroundColor Red
    exit 1
}

write-host "Package Id Found: $packageId" -ForegroundColor Green
return $packageId
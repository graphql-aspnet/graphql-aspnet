
# Configures the folder to execute a CI build


# copy in the strict linting rules to force any 
# build warnings to actual errors
$strictLintPath = "./build/strict-linting.ruleset"
$developerLintPath = "./src/styles.ruleset"

Write-Host "Configuring Environment for CI Build" -ForegroundColor Yellow

if(-Not (Test-path $developerLintPath)){
    Write-Host "Invalid Lint Rule Path. Unable to locate $developerLintPath" -ForegroundColor Red
    exit 1
}

if(-Not (Test-path $strictLintPath)){
    Write-Host "Invalid Lint Rule Path. Unable to locate $strictLintPath"  -ForegroundColor Red
    exit 1
}

Write-Host "Enforcing Strict Linting (Stylecop)..."
Remove-Item $developerLintPath
Copy-Item  $strictLintPath -Destination $developerLintPath

Write-Host "CI Build Configuration Complete" -ForegroundColor Green
exit 0
# This script copies in the strict linting rule set used during PR builds
# It then executes the build under this strict linting to surface any errors that would occur during the PR build
#
# Once completed the script resets the original ruleset and returns the user's cursor to its 
# original location before the script started

$originalLocation = Get-Location
$gitFolder = (Get-Item $PSScriptRoot).parent

Set-Location $gitFolder

Rename-Item "./src/styles.ruleset" "styles.ruleset.original"    
Copy-Item "./build/strict-linting.ruleset" "./src/styles.ruleset"

dotnet clean "./src/graphql-aspnet.sln"
dotnet build "./src/graphql-aspnet.sln"

Set-Location $gitFolder
Remove-Item "./src/styles.ruleset"
Rename-Item "./src/styles.ruleset.original" "styles.ruleset"

Set-Location $originalLocation
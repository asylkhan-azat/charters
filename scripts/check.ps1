$ErrorActionPreference = "Stop"

function Assert-NativeSuccess([string] $step, [int] $exitCode) {
    if ($exitCode -ne 0) {
        throw "$step failed with exit code $exitCode"
    }
}

dotnet restore Charters.sln
Assert-NativeSuccess "dotnet restore" $LASTEXITCODE
dotnet build Charters.sln -c Release --no-restore
Assert-NativeSuccess "dotnet build" $LASTEXITCODE
dotnet test Charters.sln -c Release --no-build
Assert-NativeSuccess "dotnet test" $LASTEXITCODE

dotnet run --project src/Charters.Headless -c Release --no-build -- --ticks 500 --seed 7 --scenario data/scenarios/a1-proof.json --metrics > "$env:TEMP\charters-run.txt"
Assert-NativeSuccess "headless smoke" $LASTEXITCODE

Write-Host "All checks passed."

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

dotnet run --project src/Charters.Headless -c Release --no-build -- --ticks 500 --seed 7 > "$env:TEMP\charters-run1.txt"
Assert-NativeSuccess "first determinism run" $LASTEXITCODE
dotnet run --project src/Charters.Headless -c Release --no-build -- --ticks 500 --seed 7 > "$env:TEMP\charters-run2.txt"
Assert-NativeSuccess "second determinism run" $LASTEXITCODE
if (Compare-Object (Get-Content "$env:TEMP\charters-run1.txt") (Get-Content "$env:TEMP\charters-run2.txt")) {
    throw "Determinism smoke failed: identical-seed digests differ"
}

Write-Host "All checks passed."

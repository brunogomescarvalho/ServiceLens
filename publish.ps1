$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    dotnet publish .\ServiceLens\ServiceLens.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\win-x64
    if ($LASTEXITCODE -ne 0) { throw 'Falha na publicação.' }
} finally { Pop-Location }

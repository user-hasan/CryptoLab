param(
    [string]$Configuration = "Debug"
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
dotnet run --project tests\CryptoLab.Tests\CryptoLab.Tests.csproj --configuration $Configuration

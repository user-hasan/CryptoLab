param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $root "publish\CryptoLab"
Set-Location $root

# 1) Framework-dependent publish (offline, no runtime packs from NuGet)
dotnet publish src\CryptoLab.App\CryptoLab.App.csproj --configuration $Configuration --runtime $Runtime --self-contained false -p:PublishSingleFile=false -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }

# 2) Merge the locally installed .NET Desktop Runtime into the app folder (self-contained, no downloads)
$runtimeVersion = Get-ChildItem "C:\Program Files\dotnet\shared\Microsoft.NETCore.App" -Directory | Where-Object { $_.Name -like "8.*" } | Sort-Object Name -Descending | Select-Object -First 1
if (-not $runtimeVersion) { throw ".NET 8 runtime not found on this machine." }
$version = $runtimeVersion.Name
$desktopDir = "C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\$version"
if (-not (Test-Path $desktopDir)) { throw "WindowsDesktop runtime $version not found." }

Copy-Item "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\$version\*" -Destination $publishDir -Recurse -Force
Copy-Item "$desktopDir\*" -Destination $publishDir -Recurse -Force

# 3) Rewrite runtimeconfig.json to include the merged frameworks
$configPath = Join-Path $publishDir "CryptoLab.runtimeconfig.json"
$config = Get-Content $configPath -Raw | ConvertFrom-Json
$config.runtimeOptions.PSObject.Properties.Remove("frameworks")
$included = @(
    @{ name = "Microsoft.NETCore.App"; version = $version },
    @{ name = "Microsoft.WindowsDesktop.App"; version = $version }
)
$config.runtimeOptions | Add-Member -NotePropertyName includedFrameworks -NotePropertyValue $included
$config | ConvertTo-Json -Depth 10 | Set-Content $configPath -Encoding UTF8

Write-Host "Self-contained release (runtime $version merged) written to: $publishDir"
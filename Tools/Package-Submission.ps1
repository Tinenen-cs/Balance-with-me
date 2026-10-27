param(
    [string]$BuildDirectory = (Join-Path $PSScriptRoot '../Builds/Windows'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '../Submission')
)
$ErrorActionPreference='Stop'
$buildPath=(Resolve-Path -LiteralPath $BuildDirectory).Path
$outputPath=[IO.Path]::GetFullPath($OutputDirectory)
foreach($required in @('BalanceWithMe.exe','UnityPlayer.dll','BalanceWithMe_Data','MonoBleedingEdge')) {
    if(-not (Test-Path -LiteralPath (Join-Path $buildPath $required))) { throw "Missing Unity build component: $required" }
}
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=Join-Path $outputPath 'BalanceWithMe-Windows-x64.zip'
if(Test-Path -LiteralPath $zip) { throw "Output already exists. Choose a new output directory: $zip" }
$archive=[IO.Compression.ZipFile]::Open($zip,[IO.Compression.ZipArchiveMode]::Create)
try {
    foreach($file in Get-ChildItem -LiteralPath $buildPath -File -Recurse) {
        $relative=$file.FullName.Substring($buildPath.Length+1).Replace('\','/')
        if($relative -match 'DoNotShip' -or $file.Extension -eq '.pdb') { continue }
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$file.FullName,$relative,[IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
} finally { $archive.Dispose() }
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$portable=Join-Path $outputPath 'BalanceWithMe-Portable-Windows-x64.exe'
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll "/resource:$zip,game.zip" "/out:$portable" (Join-Path $PSScriptRoot 'PortableLauncher.cs')
if($LASTEXITCODE -ne 0) { throw 'Portable launcher compilation failed.' }
$hashes=Get-FileHash -LiteralPath $portable,$zip -Algorithm SHA256
$hashes | ForEach-Object { "$($_.Hash)  $([IO.Path]::GetFileName($_.Path))" } | Set-Content -LiteralPath (Join-Path $outputPath 'SHA256SUMS.txt') -Encoding ASCII
Get-Item -LiteralPath $portable,$zip | Select-Object FullName,Length

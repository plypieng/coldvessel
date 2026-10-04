[CmdletBinding()]
param(
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest = Get-Content -LiteralPath (Join-Path $repoRoot 'modinfo.json') -Raw | ConvertFrom-Json
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot ("coldvessel_{0}.zip" -f $manifest.version)
}
$OutputPath = [IO.Path]::GetFullPath($OutputPath)

$files = @(
    foreach ($directory in @('src', 'assets')) {
        Get-ChildItem -LiteralPath (Join-Path $repoRoot $directory) -File -Recurse
    }
    foreach ($name in @('modinfo.json', 'README.md', 'COMPATIBILITY.md')) {
        Get-Item -LiteralPath (Join-Path $repoRoot $name)
    }
)

# ZIP entry names must use '/', including when building on Windows.
$stream = [IO.File]::Open($OutputPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write)
try {
    $archive = New-Object IO.Compression.ZipArchive($stream, [IO.Compression.ZipArchiveMode]::Create, $true)
    try {
        foreach ($file in ($files | Sort-Object FullName)) {
            $entryName = $file.FullName.Substring($repoRoot.Length + 1).Replace('\', '/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                $archive, $file.FullName, $entryName, [IO.Compression.CompressionLevel]::Optimal
            ) | Out-Null
        }
    } finally {
        $archive.Dispose()
    }
} finally {
    $stream.Dispose()
}

& (Join-Path $PSScriptRoot 'Test-Release.ps1') -PackagePath $OutputPath
Get-FileHash -LiteralPath $OutputPath -Algorithm SHA256

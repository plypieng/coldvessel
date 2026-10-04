[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$expectedFiles = New-Object 'Collections.Generic.Dictionary[string,string]' ([StringComparer]::Ordinal)
foreach ($directory in @('src', 'assets')) {
    foreach ($file in (Get-ChildItem -LiteralPath (Join-Path $repoRoot $directory) -File -Recurse)) {
        $entryName = $file.FullName.Substring($repoRoot.Length + 1).Replace('\', '/')
        $expectedFiles.Add($entryName, $file.FullName)
    }
}
foreach ($name in @('modinfo.json', 'README.md', 'COMPATIBILITY.md')) {
    $expectedFiles.Add($name, (Join-Path $repoRoot $name))
}

$seenNames = New-Object 'Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
$archive = [IO.Compression.ZipFile]::OpenRead([IO.Path]::GetFullPath($PackagePath))
try {
    foreach ($entry in $archive.Entries) {
        $name = $entry.FullName
        if ($name.Contains('\') -or $name.StartsWith('/') -or $name -match '(^|/)\.\.?(/|$)') {
            throw "Non-portable ZIP entry: $name"
        }
        if (-not $seenNames.Add($name)) { throw "Duplicate ZIP entry: $name" }
        if (-not $expectedFiles.ContainsKey($name)) { throw "Unexpected ZIP entry: $name" }
        if ($name.StartsWith('assets/') -and $name -cne $name.ToLowerInvariant()) {
            throw "Asset path must be lowercase: $name"
        }

        $entryStream = $entry.Open()
        $sha = [Security.Cryptography.SHA256]::Create()
        try {
            $entryHash = [BitConverter]::ToString($sha.ComputeHash($entryStream)).Replace('-', '')
        } finally {
            $sha.Dispose()
            $entryStream.Dispose()
        }
        $sourceHash = (Get-FileHash -LiteralPath $expectedFiles[$name] -Algorithm SHA256).Hash
        if ($entryHash -cne $sourceHash) { throw "ZIP contents differ from the source: $name" }

        if ($name.EndsWith('.json')) {
            $reader = New-Object IO.StreamReader($entry.Open())
            try {
                $reader.ReadToEnd() | ConvertFrom-Json | Out-Null
            } finally {
                $reader.Dispose()
            }
        }
    }
    foreach ($name in $expectedFiles.Keys) {
        if (-not $seenNames.Contains($name)) { throw "Missing ZIP entry: $name" }
    }
} finally {
    $archive.Dispose()
}

Write-Output ("Validated {0}: {1} files, portable paths, valid JSON, source hashes matched." -f $PackagePath, $expectedFiles.Count)

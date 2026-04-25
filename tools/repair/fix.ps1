param(
    [string]$RulesFile = "P:\SWFC\tools\repair\rules.json"
)

$ErrorActionPreference = "Stop"

function Write-Section($text) {
    Write-Host ""
    Write-Host "==== $text ====" -ForegroundColor Yellow
}

function Read-JsonFile {
    param([string]$Path)
    Get-Content $Path -Raw | ConvertFrom-Json
}

function Normalize-TextFiles {
    param([string]$Root)

    $files = Get-ChildItem -Path $Root -File -Recurse -Include *.cs,*.csproj,*.props,*.targets,*.json,*.md,*.razor,*.css,*.js

    foreach ($file in $files) {
        if ($file.FullName -match "\\bin\\" -or $file.FullName -match "\\obj\\") {
            continue
        }

        $content = Get-Content $file.FullName -Raw

        $original = $content

        $content = $content -replace "`r?`n", "`r`n"

        $lines = $content -split "`r`n"
        $lines = $lines | ForEach-Object { $_.TrimEnd() }
        $content = ($lines -join "`r`n")

        if ($content -ne $original) {
            [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.UTF8Encoding]::new($false))
            Write-Host "Normalisiert: $($file.FullName)"
        }
    }
}

function Invoke-DotnetFormatIfEnabled {
    param($Rules)

    if ($Rules.AutoFix.RunDotnetFormat -eq $true) {
        Write-Section "dotnet format"
        & dotnet format $Rules.SolutionFile
    }
}

function Remove-BinObjIfEnabled {
    param($Rules)

    if ($Rules.AutoFix.RemoveBinObj -eq $true) {
        Write-Section "Remove bin/obj"
        $dirs = Get-ChildItem -Path $Rules.ProjectRoot -Directory -Recurse -Force |
            Where-Object { $_.Name -in @("bin", "obj") }

        foreach ($dir in $dirs) {
            Remove-Item $dir.FullName -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "Gelöscht: $($dir.FullName)"
        }
    }
}

$Rules = Read-JsonFile -Path $RulesFile

Write-Section "Normalize files"
Normalize-TextFiles -Root $Rules.ProjectRoot

Invoke-DotnetFormatIfEnabled -Rules $Rules
Remove-BinObjIfEnabled -Rules $Rules

Write-Section "Done"
Write-Host "Auto-Fix abgeschlossen."
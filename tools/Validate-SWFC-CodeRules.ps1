param(
    [string]$Root = "P:\SWFC",
    [string]$ReportPath = "P:\SWFC\artifacts\swfc-code-rules-report.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:Issues = New-Object System.Collections.Generic.List[object]

function Add-Issue {
    param(
        [string]$Severity,
        [string]$Rule,
        [string]$Path,
        [string]$Message,
        [string]$Match = ""
    )

    $script:Issues.Add([PSCustomObject]@{
        Severity = $Severity
        Rule     = $Rule
        Path     = $Path
        Message  = $Message
        Match    = $Match
    })
}

function Add-Error {
    param([string]$Rule, [string]$Path, [string]$Message, [string]$Match = "")
    Add-Issue -Severity "Error" -Rule $Rule -Path $Path -Message $Message -Match $Match
}

function Add-Warning {
    param([string]$Rule, [string]$Path, [string]$Message, [string]$Match = "")
    Add-Issue -Severity "Warning" -Rule $Rule -Path $Path -Message $Message -Match $Match
}

function Add-Info {
    param([string]$Rule, [string]$Path, [string]$Message, [string]$Match = "")
    Add-Issue -Severity "Info" -Rule $Rule -Path $Path -Message $Message -Match $Match
}

function Normalize-Path {
    param([string]$Path)
    [System.IO.Path]::GetFullPath($Path)
}

function Ensure-Directory {
    param([string]$Path)

    $dir = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($dir) -and -not (Test-Path $dir)) {
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
    }
}

function Should-IgnorePath {
    param([string]$FullPath)

    $normalized = (Normalize-Path $FullPath).Replace('/', '\')

    $ignoredPatterns = @(
        '\.git\',
        '\.vs\',
        '\.github\',
        '\bin\',
        '\obj\',
        '\node_modules\',
        '\wwwroot\lib\'
    )

    foreach ($pattern in $ignoredPatterns) {
        if ($normalized -like "*$pattern*") {
            return $true
        }
    }

    return $false
}

function Get-FilteredFiles {
    param([string]$BasePath)

    Get-ChildItem -Path $BasePath -Recurse -File -Force | Where-Object {
        -not (Should-IgnorePath $_.FullName)
    }
}

function Get-ProjectNameFromPath {
    param([string]$FullPath)

    $normalized = (Normalize-Path $FullPath).Replace('/', '\')

    if ($normalized -match '\\src\\(SWFC\.[^\\]+)\\') {
        return $Matches[1]
    }

    if ($normalized -match '\\src\\(SWFC\.[^\\]+)$') {
        return $Matches[1]
    }

    if ($normalized -match '\\tests\\([^\\]+)\\') {
        return $Matches[1]
    }

    return $null
}

function Read-ContentSafe {
    param([string]$Path)

    try {
        return [System.IO.File]::ReadAllText($Path)
    }
    catch {
        Add-Warning "FileRead" $Path "Datei konnte nicht gelesen werden."
        return $null
    }
}

$Root = Normalize-Path $Root
if (-not (Test-Path $Root)) {
    throw "Root existiert nicht: $Root"
}

Write-Host "SWFC CODE RULE VALIDATION START: $Root" -ForegroundColor Cyan

$allFiles = Get-FilteredFiles -BasePath $Root

$csharpFiles = $allFiles | Where-Object { $_.Extension -eq ".cs" }
$razorFiles = $allFiles | Where-Object { $_.Name -like "*.razor" -and $_.Name -notlike "*.razor.css" }
$cssFiles = $allFiles | Where-Object { $_.Extension -eq ".css" }
$csprojFiles = $allFiles | Where-Object { $_.Extension -eq ".csproj" }

# =========================================================
# 1. VERBOTENE UI-FRAMEWORKS
# =========================================================

$forbiddenFrameworkPatterns = @(
    'MudBlazor',
    'Telerik',
    'Syncfusion'
)

foreach ($file in $allFiles) {
    $content = Read-ContentSafe -Path $file.FullName
    if ($null -eq $content) { continue }

    foreach ($pattern in $forbiddenFrameworkPatterns) {
        if ($content -match [regex]::Escape($pattern)) {
            Add-Error "ForbiddenUiFramework" $file.FullName "Verbotenes UI-Framework gefunden: $pattern" $pattern
        }
    }
}

# =========================================================
# 2. INLINE STYLES IN RAZOR VERBOTEN
# =========================================================

foreach ($file in $razorFiles) {
    $content = Read-ContentSafe -Path $file.FullName
    if ($null -eq $content) { continue }

    if ($content -match 'style\s*=') {
        Add-Error "InlineStyleInRazor" $file.FullName "Inline-Style in Razor gefunden. Laut Regeln verboten." "style="
    }
}

# =========================================================
# 3. DOMAIN DARF KEIN ASP.NET / WEB KENNEN
# =========================================================

foreach ($file in $csharpFiles) {
    $project = Get-ProjectNameFromPath -FullPath $file.FullName
    if ($project -ne "SWFC.Domain") { continue }

    $content = Read-ContentSafe -Path $file.FullName
    if ($null -eq $content) { continue }

    $patterns = @(
        'using\s+Microsoft\.AspNetCore',
        'using\s+SWFC\.Web',
        'using\s+SWFC\.Infrastructure',
        'using\s+SWFC\.Application'
    )

    foreach ($pattern in $patterns) {
        if ($content -match $pattern) {
            Add-Error "DomainLayerViolation" $file.FullName "Domain-Schicht verletzt Layer-Regeln." $pattern
        }
    }
}

# =========================================================
# 4. APPLICATION DARF KEIN WEB / ASP.NET / INFRA KENNEN
# =========================================================

foreach ($file in $csharpFiles) {
    $project = Get-ProjectNameFromPath -FullPath $file.FullName
    if ($project -ne "SWFC.Application") { continue }

    $content = Read-ContentSafe -Path $file.FullName
    if ($null -eq $content) { continue }

    $patterns = @(
        'using\s+Microsoft\.AspNetCore',
        'using\s+SWFC\.Web',
        'using\s+SWFC\.Infrastructure'
    )

    foreach ($pattern in $patterns) {
        if ($content -match $pattern) {
            Add-Error "ApplicationLayerViolation" $file.FullName "Application-Schicht verletzt Layer-Regeln." $pattern
        }
    }
}

# =========================================================
# 5. INFRA DARF KEIN WEB KENNEN
# =========================================================

foreach ($file in $csharpFiles) {
    $project = Get-ProjectNameFromPath -FullPath $file.FullName
    if ($project -ne "SWFC.Infrastructure") { continue }

    $content = Read-ContentSafe -Path $file.FullName
    if ($null -eq $content) { continue }

    if ($content -match 'using\s+SWFC\.Web') {
        Add-Error "InfrastructureLayerViolation" $file.FullName "Infrastructure darf keine Web-Abhängigkeit haben." 'using SWFC.Web'
    }
}

# =========================================================
# 6. VERBOTENE UNSCHARFE NAMEN
# =========================================================

$forbiddenNames = @(
    'Helper',
    'Manager',
    'Processor',
    'Util',
    'Misc',
    'Stuff',
    'Temp',
    'NewFile'
)

foreach ($file in $csharpFiles) {
    $base = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)

    foreach ($name in $forbiddenNames) {
        if ($base -eq $name) {
            Add-Error "ForbiddenCodeName" $file.FullName "Verbotener unscharfer Dateiname." $name
        }
    }
}

# =========================================================
# 7. COMMON/SHARED PRÜFEN
# =========================================================

foreach ($file in $allFiles) {
    $normalized = $file.FullName.Replace('/', '\')

    if ($normalized -match '\\Common\\') {
        Add-Warning "CommonFolder" $file.FullName "Common-Ordner gefunden. Gegen Regelwerk prüfen."
    }

    if ($normalized -match '\\Shared\\') {
        Add-Warning "SharedFolder" $file.FullName "Shared-Ordner gefunden. Gegen Regelwerk prüfen."
    }
}

# =========================================================
# 8. RAZOR DATEIGRÖSSE
# =========================================================

foreach ($file in $razorFiles) {
    $lineCount = @(Get-Content $file.FullName).Count

    if ($lineCount -gt 400) {
        Add-Warning "RazorFileSize" $file.FullName "Razor-Datei größer als 400 Zeilen." $lineCount.ToString()
    }
}

# =========================================================
# 9. DOMAIN / APPLICATION DATEIGRÖSSE
# =========================================================

foreach ($file in $csharpFiles) {
    $project = Get-ProjectNameFromPath -FullPath $file.FullName
    $lineCount = @(Get-Content $file.FullName).Count

    if ($project -eq "SWFC.Domain" -and $lineCount -gt 300) {
        Add-Warning "DomainFileSize" $file.FullName "Domain-Datei größer als 300 Zeilen." $lineCount.ToString()
    }

    if ($project -eq "SWFC.Application" -and $lineCount -gt 250) {
        Add-Warning "ApplicationFileSize" $file.FullName "Application-Datei größer als 250 Zeilen." $lineCount.ToString()
    }
}

# =========================================================
# 10. WEB FACHLOGIK HEURISTIK
# =========================================================

foreach ($file in $razorFiles) {
    $content = Read-ContentSafe -Path $file.FullName
    if ($null -eq $content) { continue }

    $suspiciousPatterns = @(
        'Domain\.',
        'new\s+[A-Z][A-Za-z0-9]*\(',
        'static\s+.*Rules',
        'Calculate',
        'Compute',
        'ValidateBusiness'
    )

    foreach ($pattern in $suspiciousPatterns) {
        if ($content -match $pattern) {
            Add-Warning "PossibleBusinessLogicInUi" $file.FullName "Mögliche Fachlogik oder Regelberechnung im UI gefunden." $pattern
        }
    }
}

# =========================================================
# 11. CSS: THEME REGELN
# =========================================================

foreach ($file in $cssFiles) {
    $normalized = $file.FullName.Replace('/', '\')

    if ($file.Name -eq "theme.css") {
        if ($normalized -notmatch '\\SWFC\.Web\\wwwroot\\') {
            Add-Error "ThemeCssPlacement" $file.FullName "theme.css muss global in SWFC.Web/wwwroot liegen."
        }
    }
}

# =========================================================
# 12. CSProj: VERBOTENE UI PAKETE
# =========================================================

foreach ($file in $csprojFiles) {
    $content = Read-ContentSafe -Path $file.FullName
    if ($null -eq $content) { continue }

    foreach ($pattern in $forbiddenFrameworkPatterns) {
        if ($content -match [regex]::Escape($pattern)) {
            Add-Error "ForbiddenPackageReference" $file.FullName "Verbotenes UI-Paket referenziert: $pattern" $pattern
        }
    }
}

# =========================================================
# 13. REPORT
# =========================================================

$errors   = @($script:Issues | Where-Object { $_.Severity -eq "Error" }).Count
$warnings = @($script:Issues | Where-Object { $_.Severity -eq "Warning" }).Count
$infos    = @($script:Issues | Where-Object { $_.Severity -eq "Info" }).Count

Write-Host ""
Write-Host "=========================" -ForegroundColor Cyan
Write-Host "ERRORS   : $errors" -ForegroundColor Red
Write-Host "WARNINGS : $warnings" -ForegroundColor Yellow
Write-Host "INFOS    : $infos" -ForegroundColor Gray
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

$sorted = @($script:Issues | Sort-Object Severity, Rule, Path)

foreach ($issue in $sorted) {
    $color = switch ($issue.Severity) {
        "Error"   { "Red" }
        "Warning" { "Yellow" }
        default   { "Gray" }
    }

    Write-Host "[$($issue.Severity)] [$($issue.Rule)] $($issue.Path)" -ForegroundColor $color
    Write-Host "  $($issue.Message)" -ForegroundColor $color
    if (-not [string]::IsNullOrWhiteSpace($issue.Match)) {
        Write-Host "  Match: $($issue.Match)" -ForegroundColor DarkGray
    }
}

Ensure-Directory -Path $ReportPath

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# SWFC Code Rules Report")
$lines.Add("")
$lines.Add("**Root:** `$Root`  ")
$lines.Add("**Generated:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$lines.Add("")
$lines.Add("| Type | Count |")
$lines.Add("|---|---:|")
$lines.Add("| Errors | $errors |")
$lines.Add("| Warnings | $warnings |")
$lines.Add("| Infos | $infos |")
$lines.Add("")

if (@($sorted).Count -eq 0) {
    $lines.Add("Keine Auffälligkeiten gefunden.")
}
else {
    foreach ($issue in $sorted) {
        $lines.Add("- **$($issue.Severity)** `[$($issue.Rule)]` `$($issue.Path)`  ")
        $lines.Add("  $($issue.Message)")
        if (-not [string]::IsNullOrWhiteSpace($issue.Match)) {
            $lines.Add("  Match: `$($issue.Match)`")
        }
    }
}

[System.IO.File]::WriteAllLines($ReportPath, $lines, [System.Text.Encoding]::UTF8)

Write-Host ""
Write-Host "Report geschrieben: $ReportPath" -ForegroundColor Green

if ($errors -gt 0) {
    exit 1
}

exit 0
param(
    [string]$Root = "P:\SWFC",
    [string]$ReportPath = "P:\SWFC\artifacts\swfc-validation-report.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:Issues = New-Object System.Collections.Generic.List[object]

function Add-Issue {
    param(
        [string]$Severity,
        [string]$Rule,
        [string]$Path,
        [string]$Message
    )

    $script:Issues.Add([PSCustomObject]@{
        Severity = $Severity
        Rule     = $Rule
        Path     = $Path
        Message  = $Message
    })
}

function Add-Error {
    param([string]$Rule, [string]$Path, [string]$Message)
    Add-Issue -Severity "Error" -Rule $Rule -Path $Path -Message $Message
}

function Add-Warning {
    param([string]$Rule, [string]$Path, [string]$Message)
    Add-Issue -Severity "Warning" -Rule $Rule -Path $Path -Message $Message
}

function Add-Info {
    param([string]$Rule, [string]$Path, [string]$Message)
    Add-Issue -Severity "Info" -Rule $Rule -Path $Path -Message $Message
}

function Normalize-Path {
    param([string]$Path)
    return [System.IO.Path]::GetFullPath($Path)
}

function Get-RelativePathSafe {
    param(
        [string]$BasePath,
        [string]$TargetPath
    )

    return [System.IO.Path]::GetRelativePath(
        (Normalize-Path $BasePath),
        (Normalize-Path $TargetPath))
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
        '\node_modules\',
        '\bin\',
        '\obj\',
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

function Get-FilteredDirectories {
    param([string]$BasePath)

    Get-ChildItem -Path $BasePath -Recurse -Directory -Force | Where-Object {
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

function Get-CsprojReferences {
    param([string]$CsprojPath)

    try {
        [xml]$xml = Get-Content $CsprojPath -Raw
        $refs = @()

        $projectRefs = $xml.Project.ItemGroup.ProjectReference
        foreach ($ref in $projectRefs) {
            $include = [string]$ref.Include
            if (-not [string]::IsNullOrWhiteSpace($include)) {
                $refs += [System.IO.Path]::GetFileNameWithoutExtension($include)
            }
        }

        return $refs | Sort-Object -Unique
    }
    catch {
        Add-Warning "CsprojParse" $CsprojPath "ProjectReference konnte nicht sauber gelesen werden."
        return @()
    }
}

$Root = Normalize-Path $Root

if (-not (Test-Path $Root)) {
    throw "Root existiert nicht: $Root"
}

Write-Host "SWFC VALIDATION START: $Root" -ForegroundColor Cyan

# =========================
# REGEL-BASIS
# =========================

$expectedProjects = @(
    "SWFC.Domain",
    "SWFC.Application",
    "SWFC.Infrastructure",
    "SWFC.Web"
)

$optionalProjects = @(
    "SWFC.Worker"
)

$allowedDomainTopLevel = @(
    "M100-System",
    "M200-Business",
    "M400-Integration",
    "M500-Runtime",
    "M800-Security"
)

$allowedApplicationTopLevel = @(
    "M100-System",
    "M200-Business",
    "M400-Integration",
    "M500-Runtime",
    "M800-Security",
    "Common"
)

$allowedInfrastructureTopLevel = @(
    "Persistence",
    "Repositories",
    "Services",
    "DependencyInjection",
    "M100-System",
    "M200-Business",
    "M400-Integration",
    "M500-Runtime",
    "M800-Security"
)

$allowedWebTopLevel = @(
    "Pages",
    "Components",
    "Layout",
    "wwwroot",
    "Properties"
)

$validModuleGroups = @(
    "M100",
    "M200",
    "M300",
    "M400",
    "M500",
    "M600",
    "M700",
    "M800",
    "M900",
    "M1000"
)

$forbiddenBaseNames = @(
    "Helper",
    "Manager",
    "Processor",
    "Util",
    "Common",
    "Misc",
    "Stuff",
    "Temp",
    "NewFile",
    "DataService"
)

$allowedProjectRefs = @{
    "SWFC.Domain"        = @()
    "SWFC.Application"   = @("SWFC.Domain")
    "SWFC.Infrastructure"= @("SWFC.Domain", "SWFC.Application")
    "SWFC.Web"           = @("SWFC.Domain", "SWFC.Application", "SWFC.Infrastructure")
    "SWFC.Worker"        = @("SWFC.Domain", "SWFC.Application", "SWFC.Infrastructure")
}

$forbiddenUsingRules = @(
    @{
        Project = "SWFC.Domain"
        Patterns = @(
            '^\s*using\s+SWFC\.Application(\.|;)',
            '^\s*using\s+SWFC\.Infrastructure(\.|;)',
            '^\s*using\s+SWFC\.Web(\.|;)',
            '^\s*using\s+Microsoft\.AspNetCore(\.|;)'
        )
        Message = "Domain darf keine Application/Infrastructure/Web/AspNetCore-Abhängigkeiten haben."
    },
    @{
        Project = "SWFC.Application"
        Patterns = @(
            '^\s*using\s+SWFC\.Infrastructure(\.|;)',
            '^\s*using\s+SWFC\.Web(\.|;)',
            '^\s*using\s+Microsoft\.AspNetCore(\.|;)'
        )
        Message = "Application darf keine Infrastructure/Web/AspNetCore-Abhängigkeiten haben."
    },
    @{
        Project = "SWFC.Infrastructure"
        Patterns = @(
            '^\s*using\s+SWFC\.Web(\.|;)'
        )
        Message = "Infrastructure darf keine Web-Abhängigkeiten haben."
    }
)

# =========================
# 1. PROJEKTE
# =========================

foreach ($project in $expectedProjects) {
    $path = Join-Path $Root "src\$project"
    if (-not (Test-Path $path)) {
        Add-Error "ProjectExists" $path "Erwartetes Projekt fehlt."
    }
}

# =========================
# 2. TOP-LEVEL-ORDNER
# =========================

$topLevelRules = @{
    "SWFC.Domain"         = $allowedDomainTopLevel
    "SWFC.Application"    = $allowedApplicationTopLevel
    "SWFC.Infrastructure" = $allowedInfrastructureTopLevel
    "SWFC.Web"            = $allowedWebTopLevel
}

foreach ($projectName in $topLevelRules.Keys) {
    $projectPath = Join-Path $Root "src\$projectName"
    if (-not (Test-Path $projectPath)) { continue }

    $allowed = $topLevelRules[$projectName]
    $children = Get-ChildItem -Path $projectPath -Force | Where-Object {
        $_.PSIsContainer -and $_.Name -notin @("bin", "obj")
    }

    foreach ($child in $children) {
        if ($child.Name -notin $allowed) {
            Add-Warning "TopLevelFolder" $child.FullName "Unerwarteter Top-Level-Ordner in $projectName."
        }
    }
}

# =========================
# 3. DATEIEN / ORDNER HOLEN
# =========================

$allFiles = Get-FilteredFiles -BasePath $Root
$allDirs  = Get-FilteredDirectories -BasePath $Root

# =========================
# 4. MODULORDNER PRÜFEN
# =========================

foreach ($dir in $allDirs) {
    $name = $dir.Name

    if ($name -match '^M(\d{3,4})-') {
        $moduleNumber = [int]$Matches[1]

        if ($moduleNumber -ge 100 -and $moduleNumber -lt 1000) {
            $groupValue = [math]::Floor($moduleNumber / 100) * 100
            $groupName = "M{0}" -f $groupValue
        }
        else {
            $groupName = "M1000"
        }

        if ($groupName -notin $validModuleGroups) {
            Add-Error "ModuleGroup" $dir.FullName "Modul liegt nicht in einer gültigen SWFC-Hauptgruppe."
        }

        if ($name -notmatch '^M\d{3,4}-[A-Za-z][A-Za-z0-9-]*$') {
            Add-Error "ModuleFolderName" $dir.FullName "Modulordner entspricht nicht dem Format Mxxx-Name bzw. Mxxxx-Name."
        }
    }
}

# =========================
# 5. VERBOTENE DATEIBASENAMEN
# =========================

foreach ($file in $allFiles) {
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)

    if ($baseName.EndsWith(".razor", [System.StringComparison]::OrdinalIgnoreCase)) {
        $baseName = $baseName.Substring(0, $baseName.Length - 6)
    }

    if ($baseName -in $forbiddenBaseNames) {
        Add-Error "ForbiddenBaseName" $file.FullName "Verbotener unscharfer Dateiname."
    }
}

# =========================
# 6. PROJEKTREFERENZEN
# =========================

$csprojFiles = $allFiles | Where-Object { $_.Extension -eq ".csproj" }

foreach ($csproj in $csprojFiles) {
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($csproj.Name)
    $refs = Get-CsprojReferences -CsprojPath $csproj.FullName

    if ($allowedProjectRefs.ContainsKey($projectName)) {
        foreach ($ref in $refs) {
            if ($ref -notin $allowedProjectRefs[$projectName]) {
                Add-Error "ProjectReference" $csproj.FullName "$projectName referenziert unzulässig $ref."
            }
        }
    }
}

# =========================
# 7. USING-REGELN
# =========================

$csharpFiles = $allFiles | Where-Object { $_.Extension -eq ".cs" }

foreach ($file in $csharpFiles) {
    $project = Get-ProjectNameFromPath -FullPath $file.FullName
    if (-not $project) { continue }

    $content = Read-ContentSafe -Path $file.FullName
    if ($null -eq $content) { continue }

    foreach ($rule in $forbiddenUsingRules) {
        if ($rule.Project -ne $project) { continue }

        foreach ($pattern in $rule.Patterns) {
            if ([regex]::IsMatch($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)) {
                Add-Error "ForbiddenUsing" $file.FullName $rule.Message
                break
            }
        }
    }
}

# =========================
# 8. WEB-PLACEMENT
# =========================

foreach ($file in $allFiles) {
    $project = Get-ProjectNameFromPath -FullPath $file.FullName
    if (-not $project) { continue }

    if ($file.Name -like "*.razor" -and $file.Name -notlike "*.razor.css") {
        if ($project -ne "SWFC.Web") {
            Add-Error "RazorPlacement" $file.FullName ".razor-Dateien gehören nur nach SWFC.Web."
        }
    }

    if ($file.Name -like "*.razor.css") {
        if ($project -ne "SWFC.Web") {
            Add-Error "RazorCssPlacement" $file.FullName ".razor.css-Dateien gehören nur nach SWFC.Web."
        }
    }
}

# =========================
# 9. JSON-REGELN
# =========================

foreach ($file in $allFiles | Where-Object { $_.Extension -eq ".json" }) {
    $project = Get-ProjectNameFromPath -FullPath $file.FullName
    if (-not $project) { continue }

    $normalized = $file.FullName.Replace('/', '\')

    if ($project -eq "SWFC.Web") {
        $allowedWebJson = @(
            '\wwwroot\',
            '\Properties\launchSettings.json',
            '\appsettings.json',
            '\appsettings.Development.json',
            '\appsettings.Production.json',
            '\appsettings.Staging.json'
        )

        $isAllowed = $false
        foreach ($allowedPath in $allowedWebJson) {
            if ($normalized -like "*$allowedPath*") {
                $isAllowed = $true
                break
            }
        }

        if (-not $isAllowed) {
            Add-Warning "JsonPlacement" $file.FullName "JSON in SWFC.Web an ungewöhnlichem Ort."
        }
    }
    else {
        if ($normalized -notlike "*\Properties\launchSettings.json") {
            Add-Warning "JsonPlacement" $file.FullName "JSON außerhalb SWFC.Web/wwwroot ist prüfbedürftig."
        }
    }
}

# =========================
# 10. DATEINAMEN
# =========================

foreach ($file in $csharpFiles) {
    $name = $file.Name

    if ($name -match '^\d{14}_.*\.cs$') {
        continue
    }

    if ($name -match '^\d{14}_.*\.Designer\.cs$') {
        continue
    }

    if ($name -match '^[A-Z][A-Za-z0-9]*\.cs$') {
        continue
    }

    if ($name -match '^[A-Z][A-Za-z0-9]*\.[A-Z][A-Za-z0-9]*\.cs$') {
        continue
    }

    Add-Warning "FileNaming" $file.FullName "C#-Dateiname entspricht nicht sauber den Naming-Regeln oder ist prüfbedürftig."
}

# =========================
# 11. RAZOR/CSS-PAARE
# =========================

$razorFiles = $allFiles | Where-Object {
    $_.Name -like "*.razor" -and $_.Name -notlike "*.razor.css"
}

foreach ($razor in $razorFiles) {
    $cssPath = "$($razor.FullName).css"
    if (Test-Path $cssPath) {
        continue
    }

    # Kein Fehler, nur Hinweis
    Add-Info "RazorCssPair" $razor.FullName "Keine lokale .razor.css-Datei vorhanden."
}

# =========================
# 12. LEERE ORDNER
# =========================

foreach ($dir in $allDirs) {
    $children = Get-ChildItem -Path $dir.FullName -Force -ErrorAction SilentlyContinue | Where-Object {
        -not (Should-IgnorePath $_.FullName)
    }

    if (-not $children) {
        Add-Info "EmptyFolder" $dir.FullName "Leerer Ordner."
    }
}

# =========================
# 13. REPORT
# =========================

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
}

Ensure-Directory -Path $ReportPath

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# SWFC Validation Report")
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
    }
}

[System.IO.File]::WriteAllLines($ReportPath, $lines, [System.Text.Encoding]::UTF8)

Write-Host ""
Write-Host "Report geschrieben: $ReportPath" -ForegroundColor Green

if ($errors -gt 0) {
    exit 1
}

exit 0
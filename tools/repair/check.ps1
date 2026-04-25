param(
    [string]$RulesFile = "P:\SWFC\tools\repair\rules.json"
)

$ErrorActionPreference = "Stop"

function Write-Section([string]$Text) {
    Write-Host ""
    Write-Host "==== $Text ====" -ForegroundColor Cyan
}

function Ensure-Directory([string]$Path) {
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

function Read-JsonFile([string]$Path) {
    return Get-Content $Path -Raw | ConvertFrom-Json
}

function Add-Issue {
    param(
        [string]$Severity,
        [string]$Type,
        [string]$Target,
        [string]$Message
    )

    $script:Issues.Add([PSCustomObject]@{
        Severity = $Severity
        Type     = $Type
        Target   = $Target
        Message  = $Message
    }) | Out-Null
}

function Get-RelativePath([string]$BasePath, [string]$FullPath) {
    $baseUri = New-Object System.Uri(($BasePath.TrimEnd('\') + '\'))
    $fullUri = New-Object System.Uri($FullPath)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($fullUri).ToString()).Replace('/', '\')
}

function Get-AllFilesSafe([string]$Root) {
    return Get-ChildItem -Path $Root -File -Recurse -Force |
        Where-Object {
            $_.FullName -notmatch '\\bin\\' -and
            $_.FullName -notmatch '\\obj\\'
        }
}

function Get-AllDirsSafe([string]$Root) {
    return Get-ChildItem -Path $Root -Directory -Recurse -Force |
        Where-Object {
            $_.FullName -notmatch '\\bin\\' -and
            $_.FullName -notmatch '\\obj\\'
        }
}

function Test-TopLevelFolders($Rules) {
    Write-Section "Top-level folders"

    foreach ($folder in $Rules.RequiredTopLevelFolders) {
        $path = Join-Path $Rules.ProjectRoot $folder
        if (-not (Test-Path $path)) {
            Add-Issue "Error" "MissingTopLevelFolder" $path "Pflichtordner fehlt."
        }
    }

    foreach ($folder in $Rules.ForbiddenTopLevelFolders) {
        $path = Join-Path $Rules.ProjectRoot $folder
        if (Test-Path $path) {
            Add-Issue "Warning" "ForbiddenTopLevelFolder" $path "Nicht erlaubte künstliche Top-Level-Struktur gefunden."
        }
    }
}

function Test-RequiredProjects($Rules) {
    Write-Section "Required projects"

    foreach ($proj in $Rules.RequiredProjects) {
        if (-not (Test-Path $proj)) {
            Add-Issue "Error" "MissingProject" $proj "Pflichtprojekt fehlt."
        }
    }

    foreach ($proj in $Rules.OptionalProjects) {
        if (Test-Path $proj) {
            Write-Host "Optional vorhanden: $proj"
        }
    }
}

function Test-ProjectFolderStructure($Rules) {
    Write-Section "Project folder structure"

    foreach ($project in $Rules.ProjectFolders.PSObject.Properties.Name) {
        $projectPath = Join-Path $Rules.SourceRoot $project
        if (-not (Test-Path $projectPath)) {
            Add-Issue "Error" "MissingProjectFolder" $projectPath "Projektordner fehlt."
            continue
        }

        foreach ($expectedFolder in $Rules.ProjectFolders.$project) {
            $expectedPath = Join-Path $projectPath $expectedFolder
            if (-not (Test-Path $expectedPath)) {
                Add-Issue "Warning" "MissingExpectedFolder" $expectedPath "Erwarteter Ordner fehlt."
            }
        }
    }
}

function Test-ThemeLocation($Rules) {
    Write-Section "Theme location"

    foreach ($file in $Rules.RequiredWebFiles) {
        if (-not (Test-Path $file)) {
            Add-Issue "Error" "MissingRequiredWebFile" $file "Erforderliche Web-Datei fehlt."
        }
    }

    $themeFiles = Get-ChildItem -Path $Rules.ProjectRoot -File -Recurse -Filter theme.css -Force -ErrorAction SilentlyContinue
    foreach ($themeFile in $themeFiles) {
        if ($themeFile.FullName -ne $Rules.RequiredWebFiles[0]) {
            Add-Issue "Warning" "ThemeCssLocation" $themeFile.FullName "theme.css liegt nicht am erlaubten globalen Ort."
        }
    }
}

function Test-ForbiddenDirectoryNames($Rules) {
    Write-Section "Forbidden directory names"

    $dirs = Get-AllDirsSafe -Root $Rules.ProjectRoot
    foreach ($dir in $dirs) {
        if ($Rules.ForbiddenDirectoryNames -contains $dir.Name) {
            Add-Issue "Warning" "ForbiddenDirectoryName" $dir.FullName "Verbotener Ordnername gefunden."
        }
    }
}

function Test-ForbiddenFileNames($Rules) {
    Write-Section "Forbidden file names"

    $files = Get-AllFilesSafe -Root $Rules.ProjectRoot
    foreach ($file in $files) {
        if ($Rules.ForbiddenFileNames -contains $file.Name) {
            Add-Issue "Warning" "ForbiddenFileName" $file.FullName "Verbotener Dateiname gefunden."
        }
    }
}

function Test-ApplicationFolderRules($Rules) {
    Write-Section "Application folder rules"

    $appRoot = Join-Path $Rules.SourceRoot "SWFC.Application"
    if (-not (Test-Path $appRoot)) { return }

    $dirs = Get-AllDirsSafe -Root $appRoot
    foreach ($dir in $dirs) {
        if ($Rules.ForbiddenApplicationFolders -contains $dir.Name) {
            Add-Issue "Warning" "ForbiddenApplicationFolder" $dir.FullName "Application darf nicht technisch in Commands/Queries/Handlers/Validators zerschnitten werden."
        }
    }
}

function Test-ModuleFolderNames($Rules) {
    Write-Section "Module folder names"

    $projects = @("SWFC.Domain", "SWFC.Application")
    $pattern = '^M\d{3,4}[-\.]'

    foreach ($project in $projects) {
        $projectPath = Join-Path $Rules.SourceRoot $project
        if (-not (Test-Path $projectPath)) { continue }

        $topDirs = Get-ChildItem -Path $projectPath -Directory -Force
        foreach ($dir in $topDirs) {
            if ($dir.Name -match '^M\d{3,4}') {
                if ($dir.Name -notmatch '^M\d{3,4}-') {
                    Add-Issue "Warning" "ModuleFolderFormat" $dir.FullName "Modulordner soll dem Format Mxxx-Name folgen."
                }
            }
        }
    }
}

function Test-CsprojReferences($Rules) {
    Write-Section "Project references"

    $csprojFiles = Get-ChildItem -Path $Rules.SourceRoot -Filter *.csproj -File -Recurse
    foreach ($csproj in $csprojFiles) {
        [xml]$xml = Get-Content $csproj.FullName -Raw
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($csproj.Name)

        $references = @()
        if ($xml.Project.ItemGroup) {
            foreach ($itemGroup in $xml.Project.ItemGroup) {
                foreach ($projectReference in $itemGroup.ProjectReference) {
                    if ($projectReference.Include) {
                        $references += [System.IO.Path]::GetFileNameWithoutExtension($projectReference.Include)
                    }
                }
            }
        }

        $rule = $Rules.CsprojReferenceRules | Where-Object { $_.Project -eq $projectName }
        if ($null -ne $rule) {
            foreach ($forbidden in $rule.MustNotReference) {
                if ($references -contains $forbidden) {
                    Add-Issue "Error" "ForbiddenProjectReference" $csproj.FullName "$projectName referenziert verbotenerweise $forbidden."
                }
            }
        }

        foreach ($itemGroup in $xml.Project.ItemGroup) {
            foreach ($packageReference in $itemGroup.PackageReference) {
                if ($null -ne $packageReference.Include) {
                    $packageName = [string]$packageReference.Include
                    if ($Rules.ForbiddenUiPackages -contains $packageName) {
                        Add-Issue "Error" "ForbiddenUiPackage" $csproj.FullName "Verbotenes UI-Framework referenziert: $packageName"
                    }
                }
            }
        }
    }
}

function Test-Namespaces($Rules) {
    Write-Section "Namespaces"

    $csFiles = Get-AllFilesSafe -Root $Rules.SourceRoot | Where-Object { $_.Extension -eq ".cs" }
    foreach ($file in $csFiles) {
        $content = Get-Content $file.FullName -Raw
        if ($content -match 'namespace\s+([A-Za-z0-9_\.]+)\s*[;{]') {
            $namespace = $Matches[1]
            if (-not $namespace.StartsWith($Rules.NamespaceRoot)) {
                Add-Issue "Warning" "NamespaceRoot" $file.FullName "Namespace beginnt nicht mit $($Rules.NamespaceRoot)."
            }
        }
    }
}

function Test-RazorCssPairing($Rules) {
    Write-Section "Razor CSS pairing"

    $webRoot = Join-Path $Rules.SourceRoot "SWFC.Web"
    if (-not (Test-Path $webRoot)) { return }

    $razorFiles = Get-AllFilesSafe -Root $webRoot | Where-Object { $_.Name -like "*.razor" }
    foreach ($razor in $razorFiles) {
        if ($razor.Name -like "*.razor.css") { continue }

        $cssPath = "$($razor.FullName).css"
        if (Test-Path $cssPath) {
            continue
        }
    }

    $cssFiles = Get-AllFilesSafe -Root $webRoot | Where-Object { $_.Name -like "*.razor.css" }
    foreach ($css in $cssFiles) {
        $razorPath = $css.FullName -replace '\.css$', ''
        if (-not (Test-Path $razorPath)) {
            Add-Issue "Warning" "OrphanRazorCss" $css.FullName ".razor.css ohne passende Razor-Datei."
        }
    }
}

function Test-InlineStyles($Rules) {
    Write-Section "Inline styles"

    $webRoot = Join-Path $Rules.SourceRoot "SWFC.Web"
    if (-not (Test-Path $webRoot)) { return }

    $razorFiles = Get-AllFilesSafe -Root $webRoot | Where-Object { $_.Extension -eq ".razor" }
    foreach ($file in $razorFiles) {
        $content = Get-Content $file.FullName -Raw
        if ($content -match $Rules.SuspiciousPatterns.InlineStyleRegex) {
            Add-Issue "Warning" "InlineStyle" $file.FullName "Inline-Style gefunden."
        }
    }
}

function Test-FileSizes($Rules) {
    Write-Section "File sizes"

    $files = Get-AllFilesSafe -Root $Rules.SourceRoot
    foreach ($file in $files) {
        $lineCount = (Get-Content $file.FullName | Measure-Object -Line).Lines

        if ($file.Extension -eq ".razor" -and $lineCount -gt [int]$Rules.FileSizeGuidelines.RazorMaxLines) {
            Add-Issue "Warning" "LargeRazorFile" $file.FullName "Razor-Datei überschreitet Richtwert von $($Rules.FileSizeGuidelines.RazorMaxLines) Zeilen."
        }

        if ($file.Extension -eq ".cs") {
            if ($file.FullName -match '\\SWFC\.Domain\\' -and $lineCount -gt [int]$Rules.FileSizeGuidelines.DomainMaxLines) {
                Add-Issue "Warning" "LargeDomainFile" $file.FullName "Domain-Datei überschreitet Richtwert von $($Rules.FileSizeGuidelines.DomainMaxLines) Zeilen."
            }

            if ($file.FullName -match '\\SWFC\.Application\\' -and $lineCount -gt [int]$Rules.FileSizeGuidelines.ApplicationMaxLines) {
                Add-Issue "Warning" "LargeApplicationFile" $file.FullName "Application-Datei überschreitet Richtwert von $($Rules.FileSizeGuidelines.ApplicationMaxLines) Zeilen."
            }
        }
    }
}

function Test-ArchitectureHeuristics($Rules) {
    Write-Section "Architecture heuristics"

    $files = Get-AllFilesSafe -Root $Rules.SourceRoot

    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw

        if ($file.FullName -match '\\SWFC\.Web\\' -and $file.Extension -in @(".razor", ".cs")) {
            if ($content -match 'DbContext|Repository|WriteRepository|ReadRepository|SaveChanges|ExecuteSql|BEGIN TRANSACTION') {
                Add-Issue "Warning" "WebContainsTechnicalOrBusinessLogic" $file.FullName "Web enthält verdächtige technische/fachliche Logik."
            }
        }

        if ($file.FullName -match '\\SWFC\.Infrastructure\\' -and $file.Extension -eq ".cs") {
            if ($content -match 'CanApprove|ShouldApprove|CalculateRisk|BusinessRule|DomainRule|Decision') {
                Add-Issue "Warning" "InfrastructureDecisionLogic" $file.FullName "Infrastructure enthält verdächtige Entscheidungslogik."
            }
        }

        if ($file.FullName -match '\\M204-' -and $file.Extension -eq ".cs") {
            # ok, inventory logic allowed
        }
        elseif ($file.Extension -eq ".cs" -and $content -match 'StockMovement|InventoryMovement|AdjustStock|BookInventory|InventoryTransaction') {
            Add-Issue "Warning" "InventoryLogicOutsideM204" $file.FullName "Bestandslogik außerhalb M204 gefunden."
        }

        if ($file.Extension -eq ".cs" -and $content -match 'Consumption|Verbrauch') {
            if ($file.FullName -notmatch '\\M205-') {
                Add-Issue "Info" "ConsumptionOutsideEnergy" $file.FullName "Verbrauchsbezug außerhalb M205 gefunden."
            }
        }

        if ($file.FullName -match '\\M205-' -and $file.Extension -eq ".cs") {
            if ($content -match 'StoreConsumption|SaveConsumption|ConsumptionValue') {
                Add-Issue "Warning" "DirectConsumptionStorage" $file.FullName "M205 soll Messwerte speichern, nicht direkten Verbrauch als Primärlogik."
            }
        }

        if ($file.FullName -match '\\M500-' -and $file.Extension -eq ".cs") {
            if ($content -match 'CustomerMaster|OrganizationUnit|CostCenter|ShiftModel|UserMaster|CreateMachine|UpdateMachine') {
                Add-Issue "Warning" "MasterDataLogicInRuntime" $file.FullName "M500 enthält verdächtige Stammdatenlogik."
            }
        }

        if ($file.FullName -match '\\M102-' -and $file.Extension -eq ".cs") {
            if ($content -match 'Login|Password|Session|Claim|AccessRule|Permission|Authorization') {
                Add-Issue "Warning" "SecurityIdentityMixInM102" $file.FullName "M102 darf nicht Auth/Berechtigung vermischen."
            }
        }

        if ($file.FullName -match '\\M103-' -and $file.Extension -eq ".cs") {
            if ($content -match 'AccessRule|PermissionMatrix|VisibilityResolver|AuditEntry') {
                Add-Issue "Warning" "SecurityMixInM103" $file.FullName "M103 darf nicht Authorization/Visibility/Audit übernehmen."
            }
        }

        if ($file.FullName -match '\\M800-' -and $file.Extension -eq ".cs") {
            if ($content -match 'Login|PasswordHash|ClaimsPrincipalFactory|SessionToken') {
                Add-Issue "Warning" "IdentityMixInM800" $file.FullName "M800 darf nicht Identitäts-/Loginlogik übernehmen."
            }
        }

        if ($file.FullName -match '\\M900-' -and $file.Extension -eq ".cs") {
            if ($content -match 'ExecuteControl|SendControlCommand|WritePlc|TriggerMachineAction') {
                Add-Issue "Warning" "DirectControlInM900" $file.FullName "M900 darf nicht direkt ohne Runtime- und Security-Freigabe steuern."
            }
        }
    }
}

function Invoke-DotnetBuild($Rules) {
    Write-Section "dotnet build"

    $logPath = Join-Path $script:LogsDir "build.log"
    $output = & dotnet build $Rules.SolutionFile 2>&1
    $output | Out-File -FilePath $logPath -Encoding UTF8

    if ($LASTEXITCODE -ne 0) {
        Add-Issue "Error" "BuildFailed" $Rules.SolutionFile "dotnet build fehlgeschlagen. Siehe logs\build.log"
    }
}

function Invoke-DotnetTest($Rules) {
    Write-Section "dotnet test"

    $logPath = Join-Path $script:LogsDir "test.log"
    $output = & dotnet test $Rules.SolutionFile 2>&1
    $output | Out-File -FilePath $logPath -Encoding UTF8

    if ($LASTEXITCODE -ne 0) {
        Add-Issue "Warning" "TestsFailed" $Rules.SolutionFile "dotnet test fehlgeschlagen. Siehe logs\test.log"
    }
}

function Write-Reports($Rules) {
    Write-Section "Write reports"

    $jsonPath = Join-Path $script:ReportsDir "issues.json"
    $mdPath   = Join-Path $script:ReportsDir "issues.md"

    $script:Issues | ConvertTo-Json -Depth 10 | Out-File -FilePath $jsonPath -Encoding UTF8

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# SWFC Check Report")
    $lines.Add("")
    $lines.Add("Erstellt: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
    $lines.Add("")
    $lines.Add("## Zusammenfassung")
    $lines.Add("")
    $lines.Add("- Gesamt: $($script:Issues.Count)")
    $lines.Add("- Errors: $((@($script:Issues | Where-Object Severity -eq 'Error')).Count)")
    $lines.Add("- Warnings: $((@($script:Issues | Where-Object Severity -eq 'Warning')).Count)")
    $lines.Add("- Infos: $((@($script:Issues | Where-Object Severity -eq 'Info')).Count)")
    $lines.Add("")

    $grouped = $script:Issues | Group-Object Type | Sort-Object Name
    $lines.Add("## Nach Typ")
    $lines.Add("")
    foreach ($group in $grouped) {
        $lines.Add("- **$($group.Name)**: $($group.Count)")
    }
    $lines.Add("")

    $lines.Add("## Details")
    $lines.Add("")
    foreach ($issue in $script:Issues) {
        $lines.Add("- **[$($issue.Severity)] [$($issue.Type)]** `$($issue.Target)` - $($issue.Message)")
    }

    $lines | Out-File -FilePath $mdPath -Encoding UTF8
}

$Rules = Read-JsonFile -Path $RulesFile

$script:ReportsDir = Join-Path (Split-Path $RulesFile -Parent) "reports"
$script:LogsDir    = Join-Path (Split-Path $RulesFile -Parent) "logs"
$script:Issues     = New-Object System.Collections.Generic.List[object]

Ensure-Directory $script:ReportsDir
Ensure-Directory $script:LogsDir

Test-TopLevelFolders $Rules
Test-RequiredProjects $Rules
Test-ProjectFolderStructure $Rules
Test-ThemeLocation $Rules
Test-ForbiddenDirectoryNames $Rules
Test-ForbiddenFileNames $Rules
Test-ApplicationFolderRules $Rules
Test-ModuleFolderNames $Rules
Test-CsprojReferences $Rules
Test-Namespaces $Rules
Test-RazorCssPairing $Rules
Test-InlineStyles $Rules
Test-FileSizes $Rules
Test-ArchitectureHeuristics $Rules
Invoke-DotnetBuild $Rules
Invoke-DotnetTest $Rules
Write-Reports $Rules

Write-Host ""
Write-Host "Fertig:" -ForegroundColor Green
Write-Host (Join-Path $script:ReportsDir "issues.md")
Write-Host (Join-Path $script:ReportsDir "issues.json")
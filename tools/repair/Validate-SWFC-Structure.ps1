param(
    [string]$Root = "P:\SWFC"
)

$ErrorCount = 0

function Error($msg) {
    Write-Host "[ERROR] $msg" -ForegroundColor Red
    $global:ErrorCount++
}

function Warn($msg) {
    Write-Host "[WARN ] $msg" -ForegroundColor Yellow
}

function Info($msg) {
    Write-Host "[INFO ] $msg" -ForegroundColor Gray
}

Write-Host "SWFC VALIDATION START" -ForegroundColor Cyan

# =========================
# 1. PROJEKTSTRUKTUR
# =========================

$projects = @(
    "SWFC.Domain",
    "SWFC.Application",
    "SWFC.Infrastructure",
    "SWFC.Web"
)

foreach ($p in $projects) {
    if (!(Test-Path "$Root\src\$p")) {
        Error "Projekt fehlt: $p"
    }
}

# =========================
# 2. VERBOTENE NAMEN
# =========================

$forbiddenNames = @(
    "Helper",
    "Manager",
    "Processor",
    "Util",
    "Common",
    "Misc",
    "Stuff",
    "Temp"
)

Get-ChildItem -Recurse -Path $Root -File | ForEach-Object {
    foreach ($bad in $forbiddenNames) {
        if ($_.Name -like "*$bad*") {
            Error "Verbotener Dateiname: $($_.FullName)"
        }
    }
}

# =========================
# 3. MODULNAMEN
# =========================

Get-ChildItem "$Root\src" -Directory -Recurse | ForEach-Object {
    if ($_.Name -match "^M\d{3,4}-") {
        if ($_.Name -notmatch "^M\d{3,4}-[A-Za-z]") {
            Error "Ungültiger Modulname: $($_.FullName)"
        }
    }
}

# =========================
# 4. DOMAIN REGELN
# =========================

Get-ChildItem "$Root\src\SWFC.Domain" -Recurse -Filter *.cs | ForEach-Object {
    $content = Get-Content $_.FullName -Raw

    if ($content -match "SWFC\.Application") {
        Error "Domain referenziert Application: $($_.FullName)"
    }

    if ($content -match "SWFC\.Infrastructure") {
        Error "Domain referenziert Infrastructure: $($_.FullName)"
    }

    if ($content -match "Microsoft\.AspNetCore") {
        Error "Domain enthält Web-Code: $($_.FullName)"
    }
}

# =========================
# 5. APPLICATION REGELN
# =========================

Get-ChildItem "$Root\src\SWFC.Application" -Recurse -Filter *.cs | ForEach-Object {
    $content = Get-Content $_.FullName -Raw

    if ($content -match "SWFC\.Web") {
        Error "Application referenziert Web: $($_.FullName)"
    }

    if ($content -match "Microsoft\.AspNetCore") {
        Error "Application enthält Web-Code: $($_.FullName)"
    }
}

# =========================
# 6. INFRA REGELN
# =========================

Get-ChildItem "$Root\src\SWFC.Infrastructure" -Recurse -Filter *.cs | ForEach-Object {
    $content = Get-Content $_.FullName -Raw

    if ($content -match "SWFC\.Web") {
        Error "Infrastructure referenziert Web: $($_.FullName)"
    }
}

# =========================
# 7. WEB REGELN
# =========================

# Razor nur in Web
Get-ChildItem "$Root\src" -Recurse -Filter *.razor | ForEach-Object {
    if ($_.FullName -notmatch "SWFC\.Web") {
        Error "Razor außerhalb Web: $($_.FullName)"
    }
}

# JSON nur in wwwroot
Get-ChildItem "$Root\src" -Recurse -Filter *.json | ForEach-Object {
    if ($_.FullName -notmatch "wwwroot") {
        Warn "JSON außerhalb wwwroot: $($_.FullName)"
    }
}

# =========================
# 8. UI LOGIK VERBOT
# =========================

Get-ChildItem "$Root\src\SWFC.Web" -Recurse -Filter *.razor | ForEach-Object {
    $content = Get-Content $_.FullName -Raw

    if ($content -match "new .*Domain") {
        Error "Fachlogik im UI: $($_.FullName)"
    }
}

# =========================
# 9. MODULE BEREICHE
# =========================

$validGroups = @("M100","M200","M300","M400","M500","M600","M700","M800","M900","M1000")

Get-ChildItem "$Root\src" -Directory -Recurse | ForEach-Object {
    if ($_.Name -match "^M(\d{3,4})") {
        $num = $Matches[1]

        if ($num.Length -eq 3) {
            $group = "M$num"
            if ($group -notin $validGroups) {
                Error "Ungültige Modulgruppe: $($_.FullName)"
            }
        }
    }
}

# =========================
# 10. DATEINAMEN
# =========================

Get-ChildItem "$Root\src" -Recurse -Filter *.cs | ForEach-Object {
    if ($_.Name -notmatch "^[A-Z][A-Za-z0-9]*\.cs$") {
        Warn "Unsauberer Dateiname: $($_.FullName)"
    }
}

# =========================
# ERGEBNIS
# =========================

Write-Host ""
Write-Host "========================="

if ($ErrorCount -eq 0) {
    Write-Host "✔ KEINE KRITISCHEN FEHLER" -ForegroundColor Green
} else {
    Write-Host "✖ FEHLER: $ErrorCount" -ForegroundColor Red
}

Write-Host "========================="
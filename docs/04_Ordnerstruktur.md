# SWFC – VERBINDLICHE ORDNERSTRUKTUR (REAL)

---

# WICHTIG (KRITISCH)

Diese Struktur basiert auf dem REALEN Projekt.

NICHT erlaubt:
- bestehende funktionierende Struktur umstellen
- Dateien verschieben ohne technischen Grund
- „Idealstruktur“ über reale Struktur legen

---

# GRUNDSATZ

> Reale funktionierende Struktur hat Vorrang vor theoretischer Idealstruktur

---

# ROOT

/src
/docs

---

# PROJEKTE

/src

- SWFC.Domain
- SWFC.Application
- SWFC.Infrastructure
- SWFC.Web

OPTIONAL (wenn vorhanden):

- SWFC.Worker

WICHTIG:
Wenn ein Worker existiert oder benötigt wird:
→ NICHT entfernen
→ gehört zur Runtime-Architektur (M500)

---

# DOMAIN

/src/SWFC.Domain

/M100-System
/M200-Business
/M400-Integration
/M500-Runtime
/M800-Security

---

# APPLICATION

/src/SWFC.Application

/M100-System
/M200-Business
/M400-Integration
/M500-Runtime
/M800-Security

---

# INFRASTRUCTURE

/src/SWFC.Infrastructure

/Persistence
/Repositories
/Services

---

# WEB

/src/SWFC.Web

/Pages
/Components
/Layout
/wwwroot

---

# WICHTIG – WWWROOT (KRITISCH)

Globale statische Dateien bleiben hier:

/src/SWFC.Web/wwwroot

Beispiel:

/wwwroot/css/theme.css
/wwwroot/css/variables.css

VERBOTEN:
- Verschieben von theme.css in künstliche Ordner wie /themes
- Änderung funktionierender Struktur ohne Grund

---

# THEME REGEL

- theme.css bleibt in wwwroot
- ist globale Designquelle
- wird NICHT umorganisiert

---

# LAYOUT REGEL

- AppShell, Sidebar, Header bleiben bestehen
- werden NICHT strukturell neu erfunden
- nur technisch verbessert

---

# MODULREGEL

- Module bleiben in ihrer bestehenden Struktur
- keine Umordnung ohne klare fachliche Begründung

---

# VERBOTEN

- künstliche neue Top-Level-Strukturen
- Verschieben funktionierender Dateien
- Entfernen von Worker-Struktur
- Einführung paralleler Layoutsysteme

---

# KERNPRINZIP

> Bestehendes funktionierendes System wird stabilisiert, nicht ersetzt

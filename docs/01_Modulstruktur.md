# SWFC – Modulstruktur (FINAL)

---

## 🎯 Zweck

Diese Datei beschreibt die **vollständige Modulstruktur von SWFC**.

Sie ist:

- verbindlich für Architektur
- Grundlage für alle Moduldefinitionen
- Referenz für Implementierung (Codex)
- Maßstab für Strukturprüfungen

---

## 🧠 Grundprinzip

> Jedes Modul hat eine klare Verantwortung  
> Keine Doppelungen  
> Keine Vermischung  
> Klare Trennung der Ebenen

---

# 🧱 ROOT-MODULE

---

## M100 – System
Systemweite Grundlage, Basiskomponenten, Organisation und Setup.

---

## M200 – Business
Fachliche Kernlogik des operativen Betriebs.

---

## M300 – Presentation
Darstellung, UI, Interaktion.

---

## M400 – Integration
Schnittstellen zu externen Systemen und Maschinen.

---

## M500 – Runtime
Echtzeit, Automation, Leitwarte, Ausführung.

---

## M600 – Planning
Planung, Konzepte, Entscheidungen.

---

## M700 – Support
Support, Bugs, Incidents, SLA.

---

## M800 – Security
Sicherheit, Zugriff, Audit, Compliance.

---

## M900 – Intelligence
Analyse, Vorhersage, Optimierung.

---

## M1000 – Platform
Technische Plattform, Erweiterbarkeit, Plugins.

---

## M1100 – Productization / Distribution
Produktisierung, Auslieferung und Betrieb.

---

# 📦 DETAILSTRUKTUR

---

## M100 – System

- M101 – Foundation
- M102 – Organization
  - M102.01 – Organization Structure
  - M102.02 – Users
  - M102.03 – Cost Centers
  - M102.04 – Shift Model
- M103 – Authentication
- M104 – Documents
- M105 – Configuration
- M106 – Theme
- M107 – Setup / Deployment

---

## M200 – Business

- M201 – Assets
- M202 – Maintenance
- M203 – Inspections
- M204 – Inventory
- M205 – Energy
- M206 – Purchasing
- M207 – Quality
- M208 – Safety
- M209 – Projects
- M210 – Customers
- M211 – Analytics
- M212 – Production
- M213 – Workforce

---

## M300 – Presentation

- M301 – AppShell
- M302 – Reporting
- M303 – Notifications
- M304 – Search
- M305 – Forms / Input

---

## M400 – Integration

- M401 – Import / Export
- M402 – API
- M403 – ERP
- M404 – IoT / Maschinen
- M405 – Messaging / Events
- M406 – Identity Integration
- M407 – DMS

---

## M500 – Runtime

- M501 – Scheduler
- M502 – Automation
- M503 – Job Execution
- M504 – Control / Leitwarte
- M505 – Real-Time Processing

---

## M600 – Planning

- M601 – Roadmap
- M602 – Concepts
- M603 – Prototypes
- M604 – Decisions
- M605 – Standards / Guidelines
- M606 – Requirements
- M607 – Evaluation

---

## M700 – Support

- M701 – BugTracking
- M702 – ChangeRequests
- M703 – SupportCases
- M704 – Incident
- M705 – Knowledge Base
- M706 – SLA

---

## M800 – Security

- M801 – Security Foundation
- M802 – Application Security
- M803 – Data Security
- M804 – DevSecOps
- M805 – Audit Logging
- M806 – Access Control
- M807 – Secrets
- M808 – Monitoring
- M809 – Compliance

---

## M900 – Intelligence

- M901 – Analytics
- M902 – Prediction
- M903 – Optimization
- M904 – Anomaly Detection
- M905 – Governance

---

## M1000 – Platform

- M1001 – Plugin System
- M1002 – SDK
- M1003 – Extension Management
- M1004 – Marketplace
- M1005 – Versioning & Update Management

---

## M1100 – Productization / Distribution

- M1101 – Distribution
- M1102 – Updates
- M1103 – Product Versioning
- M1104 – Licensing
- M1105 – Backup / Restore
- M1106 – Product Operations

---

# 🔥 KRITISCHE ARCHITEKTURREGEL

---

## Trennung intern vs extern

### Intern (System / Plattform)

- M107 → Setup / Bootstrap
- M1005 → technische Versionierung

---

### Extern (Produkt)

- M1100 → Produktbetrieb
- M1102 → Update-Verteilung
- M1103 → Produktversion
- M1104 → Lizenz
- M1105 → Backup / Restore

---

## Merksatz

> M107 macht das System startfähig  
> M1005 verwaltet die technische Version  
> M1100 betreibt das Produkt

---

# ❌ VERBOTENE VERMISCHUNG

- M107 darf keine Lizenzlogik enthalten
- M1005 darf keine Produktlogik enthalten
- M1100 darf kein Bootstrap durchführen
- Setup ≠ Update-Verteilung
- Plattform ≠ Produkt

---

# ✅ STATUS

- vollständig
- konsistent
- erweitert um M107 und M1100
- bereit für Codex
- verbindlich für weitere Entwicklung
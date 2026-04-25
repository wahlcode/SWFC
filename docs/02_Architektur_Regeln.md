# SWFC – Architektur Regeln

---

## 1. Schichtenarchitektur

- Domain → Fachlogik
- Application → UseCases
- Infrastructure → Datenhaltung, externe Systeme, technische Services
- Web → UI / Darstellung / Eingabe

### Verbindlich
- Fachlogik gehört in Domain und Application
- UI enthält keine Fachlogik
- Infrastructure enthält keine fachliche Entscheidungslogik
- Web ist nur Darstellung und Interaktion

---

## 2. Modularchitektur

Jedes Modul hat genau eine fachliche Verantwortung.

### Modulgruppen
- M100 = System
- M200 = Business
- M300 = Presentation
- M400 = Integration
- M500 = Runtime
- M600 = Planning
- M700 = Support
- M800 = Security
- M900 = Intelligence
- M1000 = Platform

### Verbindlich
- keine Vermischung von Verantwortungen
- keine Schattenlogik in fremden Modulen
- keine technischen Abkürzungen über Modulgrenzen hinweg

---

## 3. Trennung zentraler Bereiche

### Struktur
- M102 = organisatorischer Zwilling
- M201 = technischer Zwilling

### Prozesse
- M202 = Wartung / Veränderung
- M203 = Prüfung / Bewertung
- M207 = Qualitätsabweichung / Qualitätssteuerung
- M208 = Sicherheit / Freigabe / Gefährdung

### Bestand
- M204 ist die einzige Quelle für Bestandsveränderungen

### Energie
- M205 speichert Messwerte
- Verbrauch wird abgeleitet
- keine direkte Verbrauchsspeicherung als Primärlogik

### Runtime
- M500 bildet Echtzeit, Leitwarte, Automation und Ausführung ab
- M500 enthält keine fachlichen Stammdaten

### Security
- M103 = Identität
- M102 = Person
- M800 = Berechtigung / Sicherheitskontrolle

### Intelligence
- M900 analysiert, prognostiziert und optimiert
- M900 steuert nicht direkt ohne Runtime- und Security-Freigabe

---

## 4. Kommunikation zwischen Modulen

Erlaubt:
- Referenzen / IDs
- definierte UseCases
- APIs
- Events / Messaging
- klar definierte Übergaben

Verboten:
- direkte Logikübernahme
- Kopieren fachlicher Regeln
- versteckte Abhängigkeiten
- implizite Nebenwirkungen

---

## 5. Datenregeln

Für fachlich relevante Daten gilt:

- keine Löschung
- nur Historisierung
- Deaktivierung
- Versionierung, wenn fachlich erforderlich

### Zusätzlich
- klare Foreign Keys
- keine Business-Logik in der Datenbank
- vollständige Nachvollziehbarkeit
- auditfähige Speicherung

---

## 6. Zeit, IDs und Einheiten

### Zeit
- Speicherung in UTC
- lokale Anzeige nur im UI

### IDs
- global eindeutig
- unveränderlich
- systemweit referenzierbar

### Einheiten
- zentrales Einheitensystem
- konsistente Verwendung in allen Modulen

---

## 7. UI-Regeln

### UI-Framework

Es wird kein externes UI-Framework verwendet.

Verboten:
- MudBlazor
- Telerik
- Syncfusion
- vergleichbare UI-Komponentenbibliotheken mit Logikanteil

UI basiert ausschließlich auf:
- eigenem Theme (theme.css)
- Razor-Komponenten
- projektspezifischen Komponentenmustern

- `theme.css` global
- `.razor.css` lokal
- keine Inline-Styles
- keine Fachlogik in Razor-Komponenten
- keine Sicherheitsentscheidungen im UI

### M300-Aufteilung
- M301 = AppShell / Struktur / Navigation / Seitenmuster
- M302 = Reporting-Darstellung
- M303 = Notifications / Alerts / Eskalationsanzeige
- M304 = Suche
- M305 = Forms / Input

---

## 8. Integrationsregeln

M400 enthält nur Integrationslogik:

- Import / Export
- API
- ERP
- IoT / Maschinen
- Messaging / Events
- Identity Integration
- DMS / File Integration

### Verbindlich
- keine Fachlogik in Integrationsmodulen
- Fachmodule sprechen nicht direkt unkontrolliert mit externen Systemen
- Integrationen laufen über definierte Schnittstellen

---

## 9. Runtime-Regeln

M500 enthält:

- Scheduler
- Automation
- Job Execution
- Leitwarte / Control
- Real-Time Processing

### Verbindlich
- Runtime ist getrennt von Business
- Runtime darf Fachregeln nicht umgehen
- Leitwarte darf nur kontrolliert eingreifen
- Maschine / SPS / PCS behält immer die Sicherheitsoberhand

---

## 10. Security-Regeln

M800 enthält die vollständige Sicherheitsschicht:

- Security Foundation
- Application Security
- Data Security
- DevSecOps
- Audit Logging
- Access Control
- Secrets & Key Management
- Security Monitoring / Detection
- Compliance / Policies

### Verbindlich
- Zero Trust
- keine Sicherheitsumgehung über UI, API oder Runtime
- vollständiges Audit für sicherheitskritische Aktionen
- kontrollierte Freigaben für Leitwarte / Steuerung

---

## 11. Planning-Regeln

M600 enthält:

- Roadmap
- Concepts
- Prototypes
- Decisions
- Standards / Guidelines
- Requirements
- Evaluation

### Verbindlich
- keine direkte Umsetzung ohne Anforderung, Bewertung oder Entscheidung
- Architekturentscheidungen müssen dokumentiert werden
- Standards sind systemweit verbindlich

---

## 12. Support-Regeln

M700 trennt strikt:

- BugTracking
- ChangeRequests
- SupportCases
- Incident Management
- Knowledge Base
- SLA / Service Levels

### Verbindlich
- technische Fehler ≠ Nutzerproblem ≠ Incident
- kritische Vorfälle werden separat behandelt
- Wissensaufbau ist Teil des Systems

---

## 13. Intelligence-Regeln

M900 enthält:

- Analytics / Intelligence Engine
- Prediction / Forecasting
- Optimization
- Anomaly Detection
- Intelligence Governance

### Verbindlich
- keine Black-Box-Steuerung
- Ergebnisse müssen nachvollziehbar sein
- Vorschläge und Vorhersagen ersetzen keine Freigabe
- Governance ist verpflichtend

---

## 14. Platform-Regeln

M1000 enthält:

- Plugin System
- Developer Platform / SDK
- Extension Management
- Marketplace
- Versioning & Update Management

### Verbindlich
- Core bleibt geschützt
- Erweiterungen arbeiten nur über definierte Schnittstellen
- Updates und Versionen sind kontrolliert und nachvollziehbar

---

## 15. Kernsatz

> Klare Trennung, klare Verantwortungen und kontrollierte Übergaben machen SWFC wartbar, sicher und skalierbar.

# SWFC – MASTER REGELWERK V3 (FINAL)

---

# 1. SYSTEMZIEL

SWFC ist:

> Eine modulare Unternehmensplattform für den gesamten operativen Betrieb inkl. Leitwarte, Energie, Produktion und Intelligence

Ziel:
- EIN System für die gesamte Firma (operativ führend)
- klare Modultrennung
- skalierbar, auditfähig, sicher
- echtzeitfähig und steuerfähig

---

# 2. GRUNDPRINZIPIEN

## 2.1 Modulprinzip
- Ein Modul = eine Verantwortung
- keine Vermischung
- lose gekoppelt

## 2.2 Schichtenarchitektur
- Domain → Fachlogik
- Application → UseCases
- Infrastructure → DB / externe Systeme
- Web → UI

❌ keine Fachlogik im UI

## 2.3 Systemdenken
SWFC ist:
- Plattform
- Leitwarte
- Betriebssteuerungssystem

---

# 3. MODULSTRUKTUR

- M100 – System
- M200 – Business
- M300 – Presentation
- M400 – Integration
- M500 – Runtime
- M600 – Planning
- M700 – Support
- M800 – Security
- M900 – Intelligence
- M1000 – Platform

---

# 4. SYSTEMWEITE KERNREGELN

## 4.1 Daten
- keine Löschung fachlich relevanter Daten
- nur Deaktivierung, Historisierung, Versionierung
- auditfähige Nachvollziehbarkeit

## 4.2 Zeit
- Speicherung in UTC
- lokale Anzeige im UI

## 4.3 IDs
- global eindeutig
- unveränderlich
- systemweit referenzierbar

## 4.4 Kommunikation
✔ über IDs / Referenzen / APIs / Events  
❌ keine direkte Logikübernahme  

## 4.5 Sicherheit
- Zero Trust
- vollständiges Audit Logging
- keine Sicherheitsumgehung über UI, API oder Runtime

---

# 5. SYSTEM (M100)

## M101 – Foundation
✔ Result Pattern  
✔ Error Handling  
✔ Guards  
✔ ValueObjects  
✔ ID-Basis  
✔ UTC-Zeit  
✔ Einheitenbasis  
✔ Audit-Basisfelder  

❌ keine Fachlogik  

## M102 – Organization
✔ organisatorischer Zwilling  
✔ Werke / Standorte / Teams / Kostenstellen / Schichten  
✔ historisierbare Benutzerstammdaten in M102.02  

❌ keine technische Struktur  
❌ keine Auth  
❌ keine Rechte  

## M103 – Authentication
✔ Login  
✔ SSO / Local  
✔ Session  
✔ Claims  

❌ keine Berechtigungen  

## M104 – Documents
✔ Upload  
✔ Versionierung  
✔ Historie  
✔ Referenzierbar  

❌ keine Fachlogik  
❌ keine Workflows  

✔ keine Löschung  

## M105 – Configuration
✔ globale Einstellungen  
✔ Feature Flags  
✔ Systemdefaults  

❌ keine Stammdaten  
❌ kein Sammelbecken  

## M106 – Theme
✔ Designsystem  
✔ Farben / Tokens  
✔ Layout-Regeln  
✔ Dark Mode  
✔ Benutzer-Theme  

❌ keine Logik  
❌ keine Berechtigungen  

---

# 6. BUSINESS (M200)

## M201 – Assets
✔ technischer Zwilling  
✔ Komponentenbaum  
✔ keine Zyklen  
✔ gleiche Komponenten über Typen, nicht Mehrfachzuordnung  

## M202 – Maintenance
✔ Wartungspläne  
✔ Wartungsaufträge  
✔ Historie  
✔ IATF-konform  

## M203 – Inspections
✔ Prüfpläne  
✔ Prüfungen  
✔ Prüfergebnisse  
✔ M203 prüft und bewertet, M202 wartet und verändert  

## M204 – Inventory
✔ nur M204 ändert Bestand  
✔ Bewegungen statt direkter Bestandsänderung  
✔ Inventur / Korrektur nachvollziehbar  

## M205 – Energy
✔ automatische + manuelle + kombinierte Erfassung  
✔ nur Messwerte speichern  
❌ kein direkter Verbrauch als Primärspeicherung  

### RFID
- Standard: RFID + Eingabe
- ohne RFID nur Ausnahme

### Offline
✔ unterstützt

### Echtzeit
✔ über M505

## M206 – Purchasing
✔ SWFC führt operativ  
✔ ERP bleibt kaufmännisch  

## M207 – Quality
✔ Qualitätsmeldungen  
✔ Ursachen  
✔ Maßnahmen  
✔ Eskalation  

## M208 – Safety
✔ Gefährdungen  
✔ Risiken  
✔ Schutzmaßnahmen  
✔ Freigaben  
✔ Grundlage für sichere Eingriffe  

## M209 – Projects
✔ Projekte  
✔ Maßnahmen  
✔ Aufgaben  
✔ KVP-fähig  

## M210 – Customers
✔ schlanke operative Kundenbasis  
✔ kein volles CRM  

## M211 – Analytics
✔ KPIs  
✔ Reports  
✔ Dashboards  
✔ abteilungsspezifische Auswertung  

## M212 – Production
✔ mittlere Tiefe  
✔ operative Aufträge  
✔ Rückmeldungen  
✔ später erweiterbar  

## M213 – Workforce
✔ Einsatzplanung  
✔ Rückmeldung  
✔ kein volles HR-System  

---

# 7. PRESENTATION (M300)

## M301 – AppShell
✔ Navigation  
✔ Seitenstruktur  
✔ Rollenbezug  
✔ Standard-UI-Patterns  

## M302 – Reporting
✔ Darstellung von Reports  
✔ Export  
❌ keine Berechnungslogik  

## M303 – Notifications
✔ Benachrichtigungen  
✔ Prioritäten  
✔ Zielgruppen  
✔ Eskalation  

## M304 – Search
✔ modulübergreifende Suche  
✔ Filter  
✔ Rechtebezug  

## M305 – Forms / Input
✔ standardisierte Eingabeschicht  
✔ Validierungsdarstellung  
❌ keine Fachlogik  

---

# 8. INTEGRATION (M400)

✔ Import / Export  
✔ API  
✔ ERP  
✔ IoT / Maschinen  
✔ Messaging / Events  
✔ Identity Integration  
✔ DMS / File Integration  

❌ keine Fachlogik in Integrationsmodulen  

---

# 9. RUNTIME (M500)

✔ Scheduler  
✔ Automation  
✔ Job Execution  
✔ Leitwarte  
✔ Echtzeitverarbeitung  

## Leitwarte (VERBINDLICH)
- Steuerung erlaubt
- nur mit Freigabe
- nur mit Berechtigung
- vollständig geloggt
- niemals Sicherheitsersatz

> Maschine / SPS / PCS hat immer die Oberhand

---

# 10. PLANNING (M600)

✔ Roadmap  
✔ Concepts  
✔ Prototypes  
✔ Decisions  
✔ Standards / Guidelines  
✔ Requirements  
✔ Evaluation  

---

# 11. SUPPORT (M700)

✔ BugTracking  
✔ ChangeRequests  
✔ SupportCases  
✔ Incident Management  
✔ Knowledge Base  
✔ SLA / Service Levels  

---

# 12. SECURITY (M800)

- M103 = Identität
- M102 = Person
- M800 = Berechtigung

✔ strikt getrennt

Inhalt:
- Security Foundation
- Application Security
- Data Security
- DevSecOps
- Audit Logging
- Access Control
- Secrets & Key Management
- Security Monitoring / Detection
- Compliance / Policies

---

# 13. INTELLIGENCE (M900)

✔ Analyse  
✔ Vorhersage  
✔ Optimierung  
✔ Anomalie-Erkennung  
✔ Governance  

❌ keine direkte Steuerung ohne Runtime- und Sicherheitsfreigabe  

---

# 14. PLATFORM (M1000)

✔ Plugin-System  
✔ SDK  
✔ Extension Management  
✔ Marketplace  
✔ Versioning & Update Management  

---

# 15. DATEI- UND UI-REGELN

## Dateigrößen
- Razor: 300–400 Zeilen
- Application: 200–250 Zeilen
- Domain: ca. 300 Zeilen

## Verboten
❌ Monster-Dateien  
❌ God Components  
❌ Logik im UI  
❌ unklare Sammeldateien  

## UI
✔ `theme.css` global  
✔ `.razor.css` lokal  
❌ Inline Styles  

---

# 16. KERNREGEL

> Architektur schlägt Implementierung.

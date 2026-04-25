# SWFC – System Overview

## Zweck

SWFC ist eine modulare Unternehmensplattform mit dem Ziel,

> **den kompletten operativen Betrieb der Firma in einem System abzubilden, zu steuern und weiterzuentwickeln**

---

## Zielbild

SWFC ist nicht nur:

- Datensammlung
- CRUD-Anwendung
- Einzelsystem

sondern:

> **ein vollständiges digitales Betriebs-, Leit- und Steuerungssystem**

---

## Unternehmenskontext

- Automobilzulieferer
- hohe Anforderungen an:
  - Nachvollziehbarkeit
  - Sicherheit
  - Auditfähigkeit
  - Energie- und Qualitätsmanagement

---

## Kritische Normen und Anforderungen

SWFC ist ausgelegt für:

- IATF 16949
- ISO 50001

Daraus folgen verbindlich:

- keine Löschung fachlich relevanter Daten
- vollständige Historisierung
- revisionssichere Nachvollziehbarkeit
- klare Benutzer- und Verantwortungszuordnung
- auditfähige Prozesse und Auswertungen

---

## Strategische Ausrichtung

### 1. Betrieb zuerst
SWFC fokussiert primär den operativen Betrieb:

- Anlagen
- Wartung
- Prüfungen
- Lager
- Energie
- Qualität
- Sicherheit
- Projekte
- Produktion
- Mitarbeitereinsatz

### 2. Verwaltung nicht ausgeschlossen
Kaufmännische und administrative Bereiche werden architektonisch berücksichtigt, aber nicht im gleichen Ausbaustand wie der Betrieb umgesetzt.

Aktuell gilt:

- SWFC führt operativ
- ERP-Systeme (z. B. SAP) bleiben für kaufmännische Prozesse relevant

---

## Leitwarte und Steuerung

SWFC ist als Leit- und Steuerungssystem ausgelegt.

Es gilt verbindlich:

- SWFC darf steuernd eingreifen
- nur mit Berechtigung
- nur mit Freigabe
- nur kontrolliert
- niemals blind
- niemals als Ersatz physischer Sicherheitsmechanismen

### Sicherheitsgrundsatz

> **Maschine / SPS / PCS behalten immer die Sicherheitsoberhand**

---

## Architekturprinzip

SWFC trennt strikt zwischen:

- Struktur
- Fachprozess
- Echtzeit / Runtime
- Sicherheit
- Intelligence
- Plattform

### Beispiele
- M201 = technischer Zwilling
- M102 = organisatorischer Zwilling
- M500 = Runtime / Leitwarte / Echtzeit
- M800 = Sicherheit
- M900 = Analyse / Vorhersage / Optimierung

---

## Daten- und Systemprinzipien

### Keine Löschung
Für alle fachlich relevanten Daten gilt:

- keine Löschung
- nur Deaktivierung
- Historisierung
- Versionierung

### UTC
- Zeitspeicherung in UTC
- lokale Anzeige im UI

### IDs
- global eindeutig
- unveränderlich
- systemweit referenzierbar

### Einheiten
- zentrales Einheitensystem
- konsistente Verwendung

---

## Sicherheitsprinzipien

- Zero Trust
- Trennung von Identität, Person und Berechtigung
- zentrale Zugriffskontrolle
- vollständiges Audit Logging
- Secrets- und Key-Management
- Security Monitoring
- Compliance / Policies

---

## Echtzeit und Energie

SWFC unterstützt:

- automatische Erfassung
- manuelle Erfassung
- kombinierte Erfassung

Besonders für Energie gilt:

- Messwerte statt direkter Verbrauchsspeicherung
- Verbrauch wird abgeleitet
- RFID-gestützte mobile Erfassung
- Offline-Fähigkeit relevant
- Echtzeitverarbeitung möglich

---

## Produktions- und Workforce-Ausrichtung

### Produktion
SWFC bildet die operative Produktion ab:

- Maschinenzustände
- Stückzahlen
- Ausschuss
- Stillstände
- operative Aufträge

### Workforce
SWFC bildet den operativen Mitarbeitereinsatz ab:

- Einsatzplanung
- Rückmeldungen
- Tätigkeitsbezug

---

## Intelligence

SWFC ist nicht nur reaktiv, sondern intelligent erweiterbar durch:

- Analyse
- Vorhersage
- Optimierung
- Anomalie-Erkennung
- Governance für intelligente Funktionen

---

## Plattformfähigkeit

SWFC ist langfristig als Plattform gedacht:

- Plugin-System
- SDK / Developer Platform
- Extension Management
- Marketplace
- Versioning & Update Management

---

## Kernsatz

> **SWFC ist das vollständige digitale Abbild des operativen Unternehmens – mit Leitwarte, Sicherheit, Intelligence und Erweiterbarkeit.**

---

## Status

- Systemziel definiert
- Architektur von M100 bis M1000 festgelegt
- operativer Betrieb vollständig abgedeckt
- für Umsetzung und Wiki-Konsolidierung bereit

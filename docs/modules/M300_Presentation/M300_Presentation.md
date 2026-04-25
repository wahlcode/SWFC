# M300 – Presentation

## Zweck
M300 bildet die **Darstellungs- und Interaktionsschicht** des SWFC-Systems ab.

Es enthält alle UI-bezogenen Komponenten für:
- Anzeige von Daten
- Benutzerinteraktion
- Navigation
- Eingabe
- Suche
- Benachrichtigungen

M300 beantwortet die Frage:

> **Wie erlebt und bedient der Benutzer das System?**

---

## Grundprinzip

> M300 enthält KEINE Fachlogik  
> M300 stellt dar und ermöglicht Interaktion

---

## Struktur

M300 besteht aus:

- M301 – AppShell  
- M302 – Reporting  
- M303 – Notifications  
- M304 – Search  
- M305 – Forms / Input  

---

## Datenfluss

- M300 greift auf M200 (Business) und M211 (Analytics) zu  
- M300 verändert keine Fachlogik direkt  

---

## Rollenbasierte Darstellung

Die UI ist abhängig von:

- Benutzer  
- Rolle  
- Verantwortungsbereich  

Ziel:

> Jeder Benutzer sieht nur relevante Inhalte

---

## UI-Prinzipien

### 1. Konsistenz
- einheitliches Design  
- gleiche Interaktionsmuster  

---

### 2. Modultrennung
- jede UI gehört zu einem Modul  
- keine Vermischung  

---

### 3. Klarheit
- keine überladenen Oberflächen  
- klare Struktur  

---

### 4. Eingabe & Darstellung getrennt
- Anzeige (M301–M304)  
- Eingabe (M305)  

---

## Abgrenzung

M300 enthält **nicht**:

- Fachlogik (→ M200)  
- Datenhaltung  
- Integrationen (→ M400)  
- Runtime / Steuerung (→ M500)  

---

## Bedeutung im Gesamtsystem

M300 ist entscheidend für:

- Benutzerakzeptanz  
- Effizienz  
- Bedienbarkeit  
- Fehlervermeidung  

---

## Kernsatz

> **M300 = die komplette Benutzeroberfläche von SWFC**

---

## Status
- vollständig definiert  
- architektonisch geprüft  
- ohne funktionale Lücken  
- verbindlich für SWFC  
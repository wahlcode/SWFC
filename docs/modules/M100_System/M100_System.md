# M100 – System

---

## 🎯 Zweck

M100 bildet die **systemweite Grundlage von SWFC**.

Es enthält alle Module, die:

- modulübergreifend benötigt werden
- keine Fachdomäne darstellen
- die Plattform technisch und logisch zusammenhalten
- das System initial startfähig und konfigurierbar machen

---

## 🧠 Kernsatz

> M100 = Systembasis  
> kein Business  
> keine Fachprozesse  
> keine operative Steuerlogik

---

## 📦 Module

- M101 – Foundation
- M102 – Organization
- M103 – Authentication
- M104 – Documents
- M105 – Configuration
- M106 – Theme
- M107 – Setup / Deployment

---

## 🔧 Rolle im Gesamtsystem

M100 stellt sicher:

- das System ist strukturell konsistent
- alle Module auf gemeinsame Grundlagen zugreifen können
- zentrale Mechanismen (Zeit, IDs, Konfiguration etc.) einheitlich sind
- das System technisch initialisierbar ist

---

## 🔥 Erweiterung: M107 – Setup / Deployment

M107 erweitert M100 um die **interne Inbetriebnahme- und Bootstrap-Logik**.

### M107 ist zuständig für:

- Erstinstallation
- Setup-State
- DB-Verbindung
- DB-Erstellung
- Migrationsausführung im Setup-Kontext
- technische Systeminitialisierung
- Herstellung eines startfähigen Systems

---

## ❗ Abgrenzung M107

M107 ist:

- kein Produktmodul
- kein Lizenzmodul
- kein Backup-/Restore-Modul
- kein Update-Verteilungsmodul
- kein Produktbetriebsmodul

Diese Verantwortungen liegen in:

- M1005 – Versioning & Update Management
- M1100 – Productization / Distribution

---

## 🔗 Grundprinzipien

- strikte Trennung von Fachlogik
- keine Vermischung mit Business-Modulen
- keine Runtime-Steuerung
- keine Produktlogik
- Setup ist intern und technisch
- Produktbetrieb ist extern und gehört nach M1100

---

## ❌ Verboten in M100

- Wartung, Produktion, Lagerlogik
- Runtime-Steuerung
- Leitwarte
- Produktvertrieb
- Lizenzierung
- Backup-/Restore-Produktlogik
- Update-Verteilung

---

## Bedeutung

M100 ist:

- Fundament des Systems
- Voraussetzung für alle Module
- Grundlage für Stabilität und Konsistenz
- Ausgangspunkt jeder Systeminitialisierung

---

## Status

- vollständig definiert
- erweitert um M107
- architektonisch konsistent
- Codex-tauglich
\# M504 – Control / Leitwarte



\## Zweck

M504 bildet die \*\*Leitwarten-, Kontroll- und Bedienebene\*\* des SWFC-Systems ab.



Es enthält alle Funktionen für:

\- Live-Ansichten

\- Leitwarten-Darstellung

\- Bedienoberflächen

\- kontrollierte Eingriffe



M504 beantwortet die Frage:



> \*\*Wie werden laufende Anlagen live überwacht und kontrolliert bedient?\*\*



\---



\## Grundprinzip



> M504 ist die Leit- und Bedienoberfläche der Runtime-Schicht



\---



\## Inhalt



M504 enthält:



\- Live-Ansichten

\- Leitwartenoberflächen

\- Übersicht mehrerer Anlagen / Bereiche

\- Bedien- und Eingriffsoberflächen

\- zentrale Kontrollansichten



\---



\## Nutzung



M504 wird genutzt für:



\- Schaltwarte / Leitwarte

\- Überwachung laufender Anlagen

\- Anzeige von Zuständen

\- kontrollierte Eingriffe

\- spätere Spiegelung / Steuerung von Leitwartenbildschirmen



\---



\## Sicherheitsgrundsatz



> Kein Eingriff ohne:

\- Berechtigung

\- Sicherheitsprüfung

\- definierte Freigaberegeln

\- Abstimmung mit Personal vor Ort



\---



\## Verknüpfungen



M504 arbeitet eng mit:



\- M208 – Safety

\- M800 – Security

\- M505 – Real-Time Processing

\- M404 – IoT / Maschinen

\- M212 – Production / Operations



\---



\## Abgrenzung



M504 enthält \*\*nicht\*\*:



\- allgemeine Fachlogik

\- Sicherheitsentscheidung selbst

\- Maschinenintegration selbst

\- reine Event-Kommunikation



Diese liegen in:

\- M200-Fachmodulen

\- M208 / M800

\- M404

\- M405 / M505



\---



\## Regeln



\- Leitwarte zeigt und bedient, ersetzt aber keine Sicherheitsinstanz

\- physische Sicherheit bleibt außerhalb von SWFC

\- SPS / PCS behalten finale Schutzfunktion



\---



\## Bedeutung im Gesamtsystem



M504 ist zentral für:



\- Live Monitoring

\- Leitwarte

\- Fernsteuerung

\- Betriebsübersicht

\- kontrollierte Eingriffe



\---



\## Kernsatz



> \*\*M504 = Leit- und Bedienebene von SWFC\*\*



\---



\## Status

\- fachlich definiert

\- architektonisch geprüft

\- leitwartenfähig

\- verbindlich für SWFC

# M504 – Control / Leitwarte

## Zweck
M504 bildet die **Leitwarten- und Steuerungsebene** ab.

---

## Steuerungsprinzip (VERBINDLICH)

> SWFC darf steuernd eingreifen – aber nur kontrolliert und abgesichert

---

## Sicherheitsgrundsatz (KRITISCH)

- Maschine / SPS behält IMMER die Sicherheitsoberhand
- SWFC ersetzt niemals physische Sicherheitsmechanismen

---

## Regeln für Steuerung

Steuerung ist nur erlaubt wenn:

- Berechtigung vorhanden (M800)
- Freigabe erfolgt
- Aktion wird protokolliert (M805)
- keine automatische Blindsteuerung

---

## Verhalten

- Anzeige + Steuerung möglich
- keine unkontrollierten Eingriffe
- keine automatische gefährliche Aktionen

---

## Ziel

> Leitwarte mit echter Steuerfähigkeit – ohne Sicherheitsrisiko

---

## Kernsatz

> M504 = kontrollierte Leitwarte mit sicherer Eingriffsmöglichkeit

\# M205 – Energy



\## Zweck

M205 bildet die \*\*Energie- und Medienerfassungslogik\*\* des Unternehmens ab.



Es enthält alle fachlichen Prozesse rund um:

\- Zähler

\- Messwerte

\- manuelle und automatische Erfassung

\- Hierarchien

\- Plausibilisierung

\- Energiekennzahlen



M205 beantwortet die Frage:



> \*\*Welche Messwerte wurden wann an welchem Zähler erfasst und wie lassen sich daraus Energie- und Medienkennzahlen ableiten?\*\*



\---



\## Grundprinzip



> M205 speichert \*\*Messwerte / Zählerstände\*\*  

> und \*\*nicht den Verbrauch als Primärdaten\*\*



Verbrauch entsteht durch:

\- Differenzbildung

\- Auswertung

\- Kennzahlenberechnung



\---



\## Unterstützte Medien



M205 unterstützt Medien wie:



\- Strom

\- Wasser

\- Gas

\- O₂



Wichtig:



> Medien sind \*\*erweiterbar\*\* und nicht auf eine feste Liste begrenzt



\---



\## Kernobjekte



\### EnergyMeter

Ein Zähler enthält u. a.:



\- Medium

\- Erfassungsmodus

\- Parent/Child-Beziehung

\- Hauptzähler / Unterzähler

\- optionaler Bezug zu Anlagen / Maschinen

\- optionaler Bezug zu externen Systemen



\### EnergyReading

Ein Messwert enthält u. a.:



\- Datum / Zeit

\- Wert

\- Quelle

\- Benutzer (bei manueller Erfassung)

\- Kontext der Erfassung



\---



\## Erfassungsarten



Ein Zähler kann betrieben werden als:



\- manuell

\- automatisch

\- kombiniert (manuell + automatisch)



Wichtig:



> kombinierte Erfassung dient u. a. der Kontrolle automatischer Werte



\---



\## Manuelle Erfassung



Manuelle Erfassung enthält mindestens:



\- Datum / Zeit

\- Messwert

\- Benutzer

\- Quelle



Diese Erfassung ist audit- und nachvollziehbar.



\---



\## Automatische Erfassung



M205 ist vorbereitet für:



\- externe Systeme (z. B. Janitza)

\- Import

\- Polling

\- spätere Anbindung von Runtime-/Live-Systemen



Wichtig:



> Live-/Echtzeitlogik gehört nicht nach M205, sondern später zu M500



\---



\## Vergleich automatisch / manuell



M205 kann Abweichungen erkennen zwischen:



\- automatisch erfassten Werten

\- manuell erfassten Werten



Das dient:

\- Kontrolle

\- Plausibilisierung

\- Vertrauensbildung in automatische Erfassung



\---



\## Hierarchien



M205 unterstützt:



\- Hauptzähler

\- Unterzähler

\- Parent/Child-Strukturen



Damit können Verteilungen und Verbrauchsstrukturen abgebildet werden.



\---



\## Verknüpfungen



\### Zu M201 – Assets

Zähler können optional verknüpft werden mit:



\- Maschinen

\- Anlagen

\- Komponenten (falls fachlich sinnvoll)



Das ermöglicht:



\- Energiebezug pro Maschine / Anlage

\- technische Energieauswertung

\- ISO-50001-konforme Struktur



\---



\## Plausibilisierung



M205 kann prüfen auf:



\- negative Sprünge

\- unrealistische Werte

\- Ausreißer

\- Abweichungen zwischen Erfassungsarten



\---



\## Energiekennzahlen



M205 unterstützt Energiekennzahlen (EnPI), z. B.:



\- Verbrauch pro Maschine

\- Verbrauch pro Zeitraum

\- Vergleich zwischen Bereichen / Anlagen



\---



\## Regeln



\### Datenhaltung

\- keine Löschung fachlich relevanter Messwerte

\- nur Historisierung / Korrektur mit Nachvollziehbarkeit



\### Abgrenzung

M205 enthält \*\*nicht\*\*:



\- Wartungslogik (→ M202)

\- Lagerlogik (→ M204)

\- Steuerung / Leitwarte (→ M500)

\- reine Live-Daten-Visualisierung (→ M300 / M500)



\---



\## Bedeutung im Gesamtsystem



M205 ist zentral für:



\- ISO 50001

\- Nachvollziehbarkeit von Energie- und Medienwerten

\- Vergleich manuell / automatisch

\- technische Energieauswertung

\- spätere Integration externer Zählerinfrastruktur



\---



\## Kernsatz



> \*\*M205 = fachliche Wahrheit aller Energie- und Medienmesswerte\*\*



\---



\## Status

\- fachlich definiert

\- architektonisch geprüft

\- verbindlich für SWFC

# M205 – Energy

## Zweck
M205 bildet das **Energiemanagementsystem** ab.

---

## Erfassungsarten (VERBINDLICH)

M205 unterstützt:

- automatische Erfassung
- manuelle Erfassung
- kombinierte Erfassung (automatisch + Kontrolle)

---

## RFID-basierte Erfassung (VERBINDLICH)

### Grundprinzip

> Jeder Zähler soll eindeutig identifizierbar sein

---

### Regeln

- Standard: Erfassung über **RFID + manuelle Eingabe**
- Ziel: Vermeidung von Zählerverwechslungen

---

### Ausnahmefälle

Manuelle Eingabe **ohne RFID** ist nur erlaubt, wenn:

- Zähler hat **kein RFID**
- RFID ist **defekt / nicht lesbar**

Diese Fälle gelten als:

> **Ausnahmebetrieb und müssen nachvollziehbar sein**

---

## Offline-Fähigkeit (VERBINDLICH)

M205 muss unterstützen:

- mobile Erfassung ohne stabile Netzverbindung
- spätere Synchronisation

Begründung:
- reales Firmennetz ist nicht zuverlässig

---

## Plausibilisierung

Bei auffälligen Werten:

- Werte werden gespeichert
- Werte werden markiert
- System kann Alarm / Hinweis erzeugen

---

## Echtzeitverarbeitung (VERBINDLICH)

M205 nutzt Echtzeitdaten:

- Rohdaten → M404 (IoT)
- Verarbeitung → M505 (Real-Time)
- Fachliche Auswertung → M205

---

## Kernsatz

> M205 = Energiemanagement mit kombinierter, sicherer und praxisnaher Datenerfassung

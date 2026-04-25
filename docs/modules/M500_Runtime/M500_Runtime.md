\# M500 – Runtime



\## Zweck

M500 bildet die \*\*Runtime-, Echtzeit- und Leitwarten-Schicht\*\* des SWFC-Systems ab.



Es enthält alle technischen und operativen Laufzeitprozesse für:

\- zeitgesteuerte Ausführung

\- Automatisierung

\- Echtzeitverarbeitung

\- Leitwarte / Control

\- Reaktion auf Live-Zustände



M500 beantwortet die Frage:



> \*\*Wie arbeitet SWFC im laufenden Betrieb in Echtzeit und wie wird aktiv eingegriffen?\*\*



\---



\## Struktur



M500 besteht aus:



\- M501 – Scheduler

\- M502 – Automation

\- M503 – Job Execution

\- M504 – Control / Leitwarte

\- M505 – Real-Time Processing



\---



\## Grundprinzipien



\### 1. Trennung von Struktur und Realität

\- M201 = technische Struktur

\- M500 = reale Laufzeit / Zustände / Reaktionen



\---



\### 2. Keine Sicherheitsumgehung

M500 darf Eingriffe nur ausführen, wenn:

\- Berechtigung geprüft ist

\- Sicherheitsregeln erfüllt sind

\- Freigabe / Abstimmung vor Ort erfolgt ist



\---



\### 3. Runtime ist nicht Fachlogik

M500 führt aus, verarbeitet und reagiert,

aber die fachliche Wahrheit bleibt in:

\- M200

\- M800

\- M100



\---



\## Abgrenzung



M500 enthält nicht:



\- Fachstammdaten (→ M200 / M100)

\- allgemeine Integrationslogik (→ M400)

\- Plattformbetrieb wie Updater / Versioning (→ M1000)



\---



\## Bedeutung im Gesamtsystem



M500 ist zentral für:



\- Leitwarte

\- Live Monitoring

\- Runtime-Automatisierung

\- kontrollierte Eingriffe

\- Reaktion auf Echtzeitdaten



\---



\## Kernsatz



> \*\*M500 = lebende Echtzeit- und Steuerungsschicht von SWFC\*\*



\---



\## Status

\- vollständig neu strukturiert

\- architektonisch geprüft

\- leitwartenfähig

\- verbindlich für SWFC

## Ergänzung – Echtzeitverarbeitung

- Echtzeitdaten werden über M505 verarbeitet
- Fachmodule (z. B. M205) greifen darauf zu

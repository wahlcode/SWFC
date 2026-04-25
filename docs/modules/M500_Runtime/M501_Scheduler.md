\# M501 – Scheduler



\## Zweck

M501 bildet die \*\*zeitgesteuerte Auslösung von Runtime-Prozessen\*\* ab.



Es enthält alle Mechanismen für:

\- Zeitpläne

\- Fälligkeiten

\- periodische Ausführung

\- geplante Trigger



M501 beantwortet die Frage:



> \*\*Wann soll etwas automatisch gestartet oder geprüft werden?\*\*



\---



\## Grundprinzip



> M501 plant und triggert  

> M501 führt nicht selbst die Fachlogik aus



\---



\## Inhalt



M501 enthält:



\- Zeitpläne

\- periodische Trigger

\- terminbasierte Auslösung

\- Fälligkeitsauslösung

\- geplante Runtime-Starts



\---



\## Nutzung



M501 wird genutzt für z. B.:



\- Wartungsfälligkeiten (M202)

\- Prüfzyklen (M203)

\- geplante Jobs

\- periodische Auswertungen

\- automatische Kontrollläufe



\---



\## Abgrenzung



M501 enthält \*\*nicht\*\*:



\- Fachlogik

\- Job-Ausführung selbst

\- Echtzeitverarbeitung

\- Leitwartenfunktionen



Diese liegen in:

\- M503 – Job Execution

\- M505 – Real-Time Processing

\- M504 – Control / Leitwarte



\---



\## Regeln



\- Scheduler löst aus, entscheidet aber nicht fachlich

\- fachliche Regeln bleiben in Fachmodulen

\- Ausführung erfolgt kontrolliert über Runtime-Prozesse



\---



\## Bedeutung im Gesamtsystem



M501 ist zentral für:



\- Automatisierung

\- Fälligkeiten

\- planbare Runtime-Prozesse



\---



\## Kernsatz



> \*\*M501 = zeitgesteuerter Auslöser der Runtime\*\*



\---



\## Status

\- fachlich definiert

\- architektonisch geprüft

\- verbindlich für SWFC


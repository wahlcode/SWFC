\# M402 – API



\## Zweck

M402 bildet die \*\*API-Schnittstelle des SWFC-Systems\*\* ab.



Es ermöglicht:

\- Zugriff auf Daten

\- Integration externer Systeme

\- Erweiterung durch Apps und Dienste



M402 beantwortet die Frage:



> \*\*Wie greifen externe Systeme strukturiert auf SWFC zu?\*\*



\---



\## Grundprinzip



> M402 enthält keine Fachlogik  

> M402 stellt definierte Schnittstellen bereit



\---



\## Inhalt



M402 enthält:



\- REST API

\- Endpunkte für Module

\- Authentifizierte Zugriffe

\- strukturierte Datenbereitstellung



\---



\## Funktionen



\- Lesen von Daten (GET)

\- Schreiben von Daten (POST/PUT)

\- Steuerung definierter Aktionen



\---



\## Sicherheit



\- Zugriff nur über Authentifizierung

\- Berechtigungen werden geprüft (M800)



\---



\## Verknüpfung



\- nutzt M200-Module

\- arbeitet mit M406 (Identity)



\---



\## Regeln



\- keine direkte Datenbankfreigabe

\- keine Umgehung von Fachlogik

\- API ruft immer Fachmodule auf



\---



\## Kernsatz



> \*\*M402 = kontrollierter Zugangspunkt für externe Systeme\*\*


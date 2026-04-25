\# M107 – Setup / Deployment



\---



\## 🎯 Zweck



M107 bildet die \*\*interne Setup-, Bootstrap- und Deployment-Vorbereitungslogik\*\* des SWFC-Systems ab.



Es ist zuständig für:



\- Erstinstallation

\- Setup-State

\- DB-Verbindung

\- DB-Erstellung / DB-Erreichbarkeit

\- Migrationsausführung im Setup-Kontext

\- System-Initialisierung

\- Herstellung eines startfähigen Systems



M107 beantwortet die Frage:



> \*\*Wie wird SWFC intern technisch startfähig gemacht?\*\*



\---



\## 🧠 Kernsatz



> M107 macht SWFC startfähig



\---



\## Inhalt



M107 enthält insbesondere:



\- Setup-Status

\- Initialisierungsstatus

\- DB-Verbindungsprüfung

\- DB-Erstellung / DB-Vorbereitung

\- Ausführung initialer Migrationen

\- technische Erstinitialisierung

\- Setup-Abschlusslogik



\---



\## Setup-State



M107 verwaltet ausschließlich Setup-/Bootstrap-Zustände wie:



\- `IsConfigured`

\- `SetupCompleted`

\- `DatabaseInitialized`

\- `SetupInProgress`



Wichtig:



> M107 verwaltet keinen Produktbetriebsstatus  

> M107 verwaltet keinen Lizenzstatus  

> M107 verwaltet keinen Kunden-Update-Status



\---



\## Verknüpfungen



\### Zu M105 – Configuration



M107 darf Konfigurationswerte speichern oder lesen, wenn diese für den Setup-Prozess erforderlich sind.



Beispiele:



\- DB-Verbindungsdaten

\- Setup-Flags

\- Initiale Systemparameter



\---



\### Zu Infrastructure



M107 darf technische Infrastrukturmechanismen nutzen für:



\- DB-Verbindungstest

\- DB-Erstellung

\- Migrationsausführung

\- technische Initialisierungslogik



\---



\### Zu M1005 – Versioning \& Update Management



M107 darf technische Versions- oder Kompatibilitätsinformationen lesen, wenn sie für Setup-Kompatibilität relevant sind.



Wichtig:



> M107 übernimmt nicht die Verantwortung von M1005



\---



\## Harte Trennung



\### M107 ist zuständig für



\- Bootstrap

\- Erstinstallation

\- Setup-Kontext

\- Initiale Datenbankstartfähigkeit

\- initiale technische Systemherstellung



\### M107 ist NICHT zuständig für



\- Produktversion beim Kunden

\- Lizenzierung

\- Backup / Restore

\- Produktbetrieb

\- Update-Verteilung

\- Plattform-Versionverwaltung als Hauptverantwortung



\---



\## Aufrufregeln



\### M107 darf aufrufen



\- technische Infrastrukturservices

\- DB-Verbindungs- und Migrationsdienste

\- M105 Configuration, wenn Setup-Konfiguration persistiert werden muss

\- technische Plattformservices, wenn zur Erstinitialisierung nötig



\---



\### M107 darf NICHT aufrufen



\- Lizenzlogik aus M1100

\- Kundenproduktstatus

\- Backup-/Restore-Orchestrierung

\- Update-Verteilung

\- Produktbetriebslogik

\- kundenseitige Rollout-Logik



\---



\## Migrationen



M107 ist zuständig für:



\- Migrationsausführung im Setup-/Bootstrap-Kontext

\- Erstinitialisierung einer leeren oder uninitialisierten Datenbank



Beispiel:



\- System ist neu

\- Datenbank ist leer

\- M107 führt Initial-Migration und Grundinitialisierung aus



Wichtig:



> Laufende Produktupdate-Verteilung gehört nicht nach M107



\---



\## Deployment-Bezug



M107 unterstützt nur den \*\*internen technischen Inbetriebnahme- und Setup-Kontext\*\*.



M107 ist kein vollständiges Produkt-Deployment-Modul.



Das bedeutet:



\- kein Kundenrollout

\- keine Produktverteilung

\- keine Lizenzbereitstellung

\- keine Backup-Verantwortung



Diese Themen gehören nach M1100.



\---



\## UI-Regeln



Erlaubt:



\- Razor-Seiten (`.razor`)

\- lokale Styles (`.razor.css`)

\- Nutzung bestehender globaler Styles wie `theme.css`



Verboten:



\- generierte Razor-Dateien (`.rz`)

\- UI-Scaffolding

\- automatisch generierte Seitenstrukturen

\- generierte UI-Artefakte

\- Tool-basierte UI-Generierung statt manueller Struktur



Grundsatz:



> Setup-UI darf existieren, aber nicht generiert werden



\---



\## Zulässige UI-Inhalte



M107-UI darf enthalten:



\- Setup Wizard

\- DB-Verbindungstest

\- Initialisierungsstatus

\- Setup-Flow

\- technische Fehlermeldungen im Setup-Kontext



\---



\## Regeln



\### Datenhaltung



\- Setup-relevante Zustände müssen nachvollziehbar sein

\- Setup-Status darf nicht implizit oder versteckt geführt werden

\- technische Initialisierung muss prüfbar sein



\---



\### Abgrenzung



M107 enthält \*\*nicht\*\*:



\- Fachlogik

\- Runtime-Jobs

\- Produktbetrieb

\- Produktlizenzierung

\- Backup-/Restore-Fachzuständigkeit

\- Kundenupdate-Management



\---



\## Bedeutung im Gesamtsystem



M107 ist zentral für:



\- technische Erstinbetriebnahme

\- initiale Startfähigkeit

\- Setup-Nachvollziehbarkeit

\- kontrollierte Bootstrap-Logik

\- klare Trennung zwischen internem Setup und externem Produktbetrieb



\---



\## Status



\- verbindlich definiert

\- Teil von M100 – System

\- getrennt von M1005 und M1100

\- Codex-tauglich


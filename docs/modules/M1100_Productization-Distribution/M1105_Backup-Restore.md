\# M1105 – Backup / Restore



\---



\## 🎯 Zweck



M1105 bildet die \*\*produktbezogene Backup- und Restore-Verantwortung\*\* ab.



Es ist zuständig für:



\- Backup-Strategien im Produktkontext

\- Restore-Prozesse

\- Recovery-Kontexte

\- Sicherungsstatus

\- Wiederherstellungsnachvollziehbarkeit



M1105 beantwortet die Frage:



> \*\*Wie wird das ausgelieferte Produkt gesichert und wiederhergestellt?\*\*



\---



\## 🧠 Kernsatz



> M1105 = Backup- und Restore-Modul des Produkts



\---



\## Inhalt



M1105 enthält:



\- Backup-Profile

\- Restore-Kontexte

\- Recovery-Zuordnungen

\- Sicherungsstatus

\- Wiederherstellungsstatus

\- Nachvollziehbarkeit von Sicherung und Wiederherstellung



\---



\## Regeln



\- nur M1105 trägt die produktbezogene Hauptverantwortung für Backup / Restore

\- M107 darf keine produktive Backup-/Restore-Hauptlogik übernehmen

\- M1005 darf keine Backup-/Restore-Fachzuständigkeit tragen



\---



\## Harte Trennung



M1105 ist zuständig für:



\- Backup / Restore im Produktbetrieb



M1105 ist NICHT zuständig für:



\- Bootstrap-Setup

\- DB-Erstinitialisierung

\- interne Plattformversionierung

\- Lizenzlogik

\- Authentifizierung

\- fachliche Sicherheitsentscheidung



\---



\## Verknüpfungen



\### Zu M1106 – Product Operations



M1105 arbeitet mit Product Operations zusammen, bleibt aber für Sicherung/Wiederherstellung eigenständig verantwortlich.



\---



\## UI-Regeln



Erlaubt:



\- Razor-Seiten (`.razor`)

\- lokale Styles (`.razor.css`)



Verboten:



\- `.rz` generierte Seiten

\- UI-Scaffolding

\- automatisch generierte Oberflächen



\---



\## Abgrenzung



M1105 enthält \*\*nicht\*\*:



\- Setup-State

\- Produktversion-Hauptlogik

\- Lizenzstatus-Hauptlogik

\- interne Migrations-Bootstrap-Verantwortung



\---



\## Status



\- verbindlich definiert

\- Teil von M1100

\- Codex-tauglich


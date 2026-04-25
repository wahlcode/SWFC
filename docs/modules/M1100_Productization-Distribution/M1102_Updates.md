\# M1102 – Updates



\---



\## 🎯 Zweck



M1102 bildet die \*\*produktbezogene Update-Verteilung und Update-Orchestrierung\*\* ab.



Es ist zuständig für:



\- Update-Auslieferung

\- Rollout-Logik

\- Update-Freigabestatus

\- Update-Verteilung im Produktkontext

\- kundenbezogene Updatezuordnung



M1102 beantwortet die Frage:



> \*\*Wie werden Produktupdates kontrolliert verteilt?\*\*



\---



\## 🧠 Kernsatz



> M1102 = Update-Verteilung des Produkts



\---



\## Inhalt



M1102 enthält:



\- Update-Pakete im Produktkontext

\- Rollout-Status

\- Verteilungsstatus

\- Freigabestatus

\- Update-Zielgruppen / Produktziele

\- Update-Historie im Produktkontext



\---



\## Harte Trennung



M1102 ist zuständig für:



\- externe / produktbezogene Update-Verteilung



M1102 ist NICHT zuständig für:



\- interne technische Plattformversionierung

\- Bootstrap-Migration

\- Erstinstallation

\- DB-Erstinitialisierung



Diese liegen in:



\- M1005

\- M107



\---



\## Verknüpfungen



\### Zu M1005 – Versioning \& Update Management



M1102 darf M1005 nutzen für:



\- technische Updatefähigkeit

\- technische Kompatibilität

\- Plattformstand



Wichtig:



> M1005 entscheidet nicht über Kundenrollout  

> M1102 entscheidet nicht über interne Plattformkernversionierung



\---



\### Zu M1103 – Product Versioning



M1102 nutzt Produktversionsinformationen zur Rollout-Steuerung.



\---



\## Regeln



\- kein technischer Bootstrap

\- keine interne DB-Setup-Hauptverantwortung

\- keine Lizenzverwaltung

\- Updates müssen nachvollziehbar und kontrolliert sein



\---



\## UI-Regeln



Erlaubt:



\- Razor-Seiten (`.razor`)

\- lokale Styles (`.razor.css`)



Verboten:



\- `.rz` generierte Seiten

\- UI-Scaffolding

\- automatisch generierte Seiten



\---



\## Abgrenzung



M1102 enthält \*\*nicht\*\*:



\- Lizenzlogik

\- Backup-/Restore-Hauptverantwortung

\- Produktbetriebs-Hauptlogik

\- technisches Setup



\---



\## Status



\- verbindlich definiert

\- Teil von M1100

\- Codex-tauglich


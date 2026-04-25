\# M1104 – Licensing



\---



\## 🎯 Zweck



M1104 bildet die \*\*Lizenzierung des SWFC-Produkts\*\* ab.



Es ist zuständig für:



\- Lizenzstatus

\- Lizenzzuordnung

\- Lizenzgültigkeit

\- lizenzabhängige Produktfreigaben

\- Produktnutzungsrahmen im Lizenzkontext



M1104 beantwortet die Frage:



> \*\*Welche Produktnutzung ist im ausgelieferten Kontext erlaubt?\*\*



\---



\## 🧠 Kernsatz



> M1104 = Lizenzierungsmodul des Produkts



\---



\## Inhalt



M1104 enthält:



\- Lizenzstatus

\- Lizenztyp

\- Gültigkeit

\- Produktfreigaben im Lizenzkontext

\- lizenzabhängige Aktivierungsrahmen



\---



\## Regeln



\- nur M1104 darf Lizenzstatus im Produktkontext verwalten

\- M107 darf keine Lizenzentscheidung treffen

\- M1005 darf keine Lizenzlogik enthalten



\---



\## Harte Trennung



M1104 ist zuständig für:



\- Lizenzierung

\- Produktfreigaben im Lizenzkontext



M1104 ist NICHT zuständig für:



\- Authentifizierung

\- fachliche Berechtigungsentscheidung

\- Setup-State

\- interne Plattformversionierung



\---



\## Verknüpfungen



\### Zu M800 – Security



M1104 ersetzt keine Sicherheitsentscheidung.



Lizenzierung ist kein Ersatz für Berechtigung.



\### Zu M103 – Authentication



M1104 ersetzt keine Anmeldung oder Identität.



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



M1104 enthält \*\*nicht\*\*:



\- Setup-/Bootstrap-Logik

\- Plattform-Core-Versionierung

\- Backup-/Restore-Hauptlogik

\- Produktoperations-Hauptlogik



\---



\## Status



\- verbindlich definiert

\- Teil von M1100

\- Codex-tauglich


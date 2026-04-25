\# M1103 – Product Versioning



\---



\## 🎯 Zweck



M1103 bildet die \*\*Version des ausgelieferten Produkts im Kunden- bzw. Produktkontext\*\* ab.



Es ist zuständig für:



\- Produktversion

\- Release-Zuordnung

\- installierte Version beim Kunden

\- freigegebene Version

\- produktbezogene Versionsnachvollziehbarkeit



M1103 beantwortet die Frage:



> \*\*Welche Produktversion ist wo tatsächlich im Einsatz?\*\*



\---



\## 🧠 Kernsatz



> M1103 = Produktversion im Einsatz



\---



\## Inhalt



M1103 enthält:



\- Produktversionen

\- Release-Stände

\- installierte Versionen

\- Freigabestände

\- Produktversionshistorie



\---



\## Harte Trennung



\### M1103 verwaltet



\- externe Produktversionierung



\### M1103 verwaltet NICHT



\- interne Plattformversionslogik

\- technische DB-Kompatibilitätsführung als Hauptverantwortung

\- Setup-State



Diese gehören in:



\- M1005

\- M107



\---



\## Verknüpfungen



\### Zu M1005 – Versioning \& Update Management



M1103 darf technische Versionen referenzieren, aber nicht ersetzen.



Beispiel:



\- M1005: technische Plattformversion 2.3.0

\- M1103: Kunde X nutzt Produktversion 2.2.1



\---



\## Regeln



\- keine interne Core-Versionierung als Hauptlogik

\- keine Lizenzverwaltung

\- keine Update-Verteilung als Hauptlogik

\- Versionen müssen eindeutig und nachvollziehbar sein



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



M1103 enthält \*\*nicht\*\*:



\- technische Plattformkompatibilität als Hauptverantwortung

\- Setup-Zustände

\- Lizenzentscheidungen

\- Backup-/Restore-Orchestrierung



\---



\## Status



\- verbindlich definiert

\- Teil von M1100

\- Codex-tauglich


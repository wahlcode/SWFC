\# M1101 – Distribution



\---



\## 🎯 Zweck



M1101 bildet die \*\*Auslieferung und Bereitstellung des SWFC-Produkts\*\* ab.



Es ist zuständig für:



\- Distributionslogik

\- Bereitstellungspakete

\- Rollout-Bereitstellung

\- Auslieferungskontexte

\- produktbezogene Verteilungsprofile



M1101 beantwortet die Frage:



> \*\*Wie wird SWFC als Produkt bereitgestellt und ausgeliefert?\*\*



\---



\## 🧠 Kernsatz



> M1101 = Auslieferungsschicht des Produkts



\---



\## Inhalt



M1101 enthält:



\- Distributionsprofile

\- Auslieferungsarten

\- Bereitstellungspakete

\- Zielkontexte der Auslieferung

\- Rollout-Vorbereitung

\- Freigabekontext für Distribution



\---



\## Regeln



\- M1101 verteilt das Produkt, bootstrapped aber nicht das System

\- keine DB-Erstinitialisierung

\- keine Setup-Hauptverantwortung

\- keine Lizenzentscheidung als Hauptlogik

\- keine technische Plattformversionsführung



\---



\## Verknüpfungen



\### Zu M1102 – Updates



M1101 arbeitet mit Update-Auslieferung zusammen, ist aber nicht selbst die Update-Entscheidungsinstanz.



\### Zu M1103 – Product Versioning



M1101 nutzt Produktversionsinformationen zur Auslieferung.



\### Zu M1104 – Licensing



M1101 darf lizenzrelevante Freigaben berücksichtigen, verwaltet aber nicht die Lizenzlogik selbst.



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



M1101 enthält \*\*nicht\*\*:



\- internes Setup

\- DB-Bootstrap

\- Plattform-Core

\- Backup-/Restore-Hauptverantwortung

\- Produktbetriebs-Hauptlogik



\---



\## Status



\- verbindlich definiert

\- Teil von M1100

\- Codex-tauglich


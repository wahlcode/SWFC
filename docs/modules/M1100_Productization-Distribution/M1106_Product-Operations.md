\# M1106 – Product Operations



\---



\## 🎯 Zweck



M1106 bildet den \*\*produktbezogenen Betriebs- und Operationskontext\*\* des ausgelieferten SWFC-Produkts ab.



Es ist zuständig für:



\- Kundenbetriebsstatus

\- Betriebsprofile

\- Deployment-Profile

\- produktbezogene Health-Sicht

\- Betriebsfähigkeit im Produktkontext



M1106 beantwortet die Frage:



> \*\*Wie wird das ausgelieferte Produkt operativ betrieben und überwacht?\*\*



\---



\## 🧠 Kernsatz



> M1106 = Produktbetrieb des ausgelieferten Systems



\---



\## Inhalt



M1106 enthält:



\- Betriebsprofile

\- Deployment-Profile

\- Kundenbetriebsstatus

\- produktbezogene Health-Sicht

\- Operations-Status

\- produktbezogene Betriebsnachvollziehbarkeit



\---



\## Regeln



\- nur M1106 trägt die Hauptverantwortung für den Produktbetriebsstatus

\- M1106 ersetzt keine Runtime-Schicht

\- M1106 ersetzt keine Leitwarte

\- M1106 ersetzt keine Sicherheitsentscheidung



\---



\## Harte Trennung



M1106 ist zuständig für:



\- Produktbetrieb

\- Betriebsprofile

\- ausgelieferten Operationskontext



M1106 ist NICHT zuständig für:



\- technische Bootstrap-Logik

\- interne Plattformversionsführung

\- Leitwartensteuerung

\- Authentifizierung

\- fachliche Berechtigungsauswertung



\---



\## Verknüpfungen



\### Zu M1101 – Distribution



M1106 kann Verteilungsprofile im Betriebskontext berücksichtigen.



\### Zu M1102 – Updates



M1106 kann Update-Zustände im Betriebsbild berücksichtigen.



\### Zu M1105 – Backup / Restore



M1106 kann Backup-/Restore-Status im Produktbetrieb sichtbar machen.



\### Zu M500 – Runtime



M1106 darf Runtime-Informationen im Produktbetriebskontext referenzieren, ersetzt aber keine Runtime-Funktion.



\---



\## UI-Regeln



Erlaubt:



\- Razor-Seiten (`.razor`)

\- lokale Styles (`.razor.css`)

\- Nutzung bestehender globaler Styles wie `theme.css`



Verboten:



\- `.rz` generierte Seiten

\- UI-Scaffolding

\- automatisch generierte Seitenstrukturen

\- generierte UI-Artefakte



\---



\## Abgrenzung



M1106 enthält \*\*nicht\*\*:



\- Runtime-Core

\- Leitwarte

\- Setup-State

\- Produktlizenz-Hauptlogik

\- Backup-/Restore-Hauptlogik

\- technische Plattformversionslogik



\---



\## Bedeutung im Gesamtsystem



M1106 ist zentral für:



\- professionellen Produktbetrieb

\- Betriebsübersicht des ausgelieferten Systems

\- klare Trennung zwischen Runtime und Produktoperationssicht

\- nachvollziehbaren Betrieb beim Kunden



\---



\## Status



\- verbindlich definiert

\- Teil von M1100

\- Codex-tauglich


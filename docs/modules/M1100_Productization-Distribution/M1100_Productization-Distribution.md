\# M1100 – Productization / Distribution



\---



\## 🎯 Zweck



M1100 bildet die \*\*Produktisierung, Auslieferung und den Produktbetrieb\*\* des SWFC-Systems ab.



Es ist zuständig für:



\- Auslieferung

\- Update-Verteilung

\- Produktversion beim Kunden

\- Lizenzierung

\- Backup / Restore

\- Produktbetrieb

\- Deployment-Profile / Kundenbetrieb



M1100 beantwortet die Frage:



> \*\*Wie wird SWFC als Produkt ausgeliefert, aktualisiert, lizenziert und betrieben?\*\*



\---



\## 🧠 Kernsatz



> M1100 macht SWFC als Produkt ausliefer- und betreibbar



\---



\## Struktur



M1100 besteht aus:



\- M1101 – Distribution

\- M1102 – Updates

\- M1103 – Product Versioning

\- M1104 – Licensing

\- M1105 – Backup / Restore

\- M1106 – Product Operations



\---



\## Grundprinzipien



\### 1. Trennung von interner Technik und externem Produkt



\- M107 = internes Setup / Bootstrap

\- M1005 = interne technische Versionierung / Plattform-Updatefähigkeit

\- M1100 = Produkt beim Kunden / ausgeliefertes System



\---



\### 2. Keine Doppelungen



M1100 enthält keine interne Setup- oder Bootstrap-Hauptverantwortung.



M1100 enthält keine Plattform-Core-Verantwortung.



\---



\### 3. Produktsicht statt reine Plattformsicht



M1100 beschreibt:



\- was beim Kunden installiert ist

\- was ausgeliefert wird

\- welche Produktversion aktiv ist

\- welche Lizenz gilt

\- wie Produktbetrieb, Backup und Restore erfolgen



\---



\## Harte Trennung



\### M1100 ist zuständig für



\- Produktauslieferung

\- Kunden-Update-Verteilung

\- Produktversion beim Kunden

\- Lizenzstatus

\- Backup-/Restore-Produktkontext

\- Betriebsprofile / Produktbetrieb



\### M1100 ist NICHT zuständig für



\- Erstinstallation als Bootstrap

\- technische DB-Erstinitialisierung

\- Plattform-Core

\- Setup-Wizard-Hauptverantwortung

\- interne technische Versionsführung als Hauptverantwortung



\---



\## Verknüpfungen



\### Zu M1005 – Versioning \& Update Management



M1100 darf M1005 nutzen zur Abfrage von:



\- technischer Version

\- Kompatibilitätsstand

\- Updatefähigkeit

\- Plattformstand



Wichtig:



> M1005 bleibt intern technisch  

> M1100 bleibt extern produktbezogen



\---



\### Zu M107 – Setup / Deployment



M1100 darf Setup-Status nur indirekt lesen, wenn dies für Produktauslieferung oder Betriebsprüfung relevant ist.



Wichtig:



> M1100 übernimmt nicht die Bootstrap-Verantwortung von M107



\---



\### Zu M800 – Security



M1100 darf Sicherheitslogik nicht ersetzen.



Lizenzierung, Produktbetrieb und Restore-Kontexte bleiben trotzdem an M800 und M103 gebunden, wo Authentifizierung, Zugriff und Sicherheitsentscheidung betroffen sind.



\---



\## Aufrufregeln



\### M1100 darf aufrufen



\- M1005 für technische Versions- und Kompatibilitätsinformationen

\- technische Distributionsdienste

\- Lizenzdienste

\- Backup-/Restore-Dienste

\- Produkt-Health- und Betriebsdienste

\- Deployment-/Operations-Dienste im Produktkontext



\---



\### M1100 darf NICHT aufrufen



\- DB-Erstinitialisierung als Hauptverantwortung

\- Bootstrap-Setup-Flow

\- Plattform-Foundation-Ersatz

\- Setup-Logik wie M107

\- fachliche Berechtigungslogik

\- Authentifizierung als Eigenverantwortung



\---



\## Versionierung



M1100 verwaltet die \*\*externe Produktversionierung\*\*.



Beispiele:



\- Welche Version hat Kunde X installiert?

\- Welches Release ist freigegeben?

\- Welches Update darf ausgerollt werden?

\- Welcher Rollout-Stand gilt?



Wichtig:



> interne technische Versionierung bleibt in M1005



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



> Produkt-UI darf existieren, aber nicht generiert werden



\---



\## Abgrenzung



M1100 enthält \*\*nicht\*\*:



\- Fachprozesse aus M200

\- Runtime-Leitwarte aus M500

\- Plattform-Core aus M1000

\- Setup-/Bootstrap-Hauptlogik aus M107

\- technische Sicherheitsentscheidungen aus M800

\- Authentifizierung aus M103



\---



\## Bedeutung im Gesamtsystem



M1100 ist zentral für:



\- kontrollierte Produktauslieferung

\- produktbezogene Updatefähigkeit

\- Lizenzierbarkeit

\- Sicherung und Wiederherstellung im Produktkontext

\- professionellen Produktbetrieb



\---



\## Status



\- verbindlich definiert

\- eigenes Root-Modul

\- getrennt von M107 und M1005

\- Codex-tauglich


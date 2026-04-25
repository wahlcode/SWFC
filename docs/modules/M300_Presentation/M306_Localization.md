\# M306 – Localization / Mehrsprachigkeit



\---



\## 🎯 Zweck



M306 bildet die \*\*sprachabhängige Darstellung des SWFC-Systems\*\* ab.



Es ist zuständig für:



\- Oberflächensprache

\- Übersetzung von UI-Texten

\- Sprachumschaltung

\- Culture / Locale

\- Datums-, Zeit-, Zahlen- und Formatdarstellung

\- sprachabhängige Validierungs- und Standardmeldungen

\- sprachabhängige Modul- und Navigationstexte



M306 beantwortet die Frage:



> \*\*In welcher Sprache und in welchem Darstellungsformat erscheint SWFC für den Benutzer?\*\*



\---



\## 🧠 Kernsatz



> M306 steuert die sprachabhängige Darstellung von SWFC  

> nicht Berechtigung  

> nicht Fachlogik  

> nicht Stammdateninhalt



\---



\## Inhalt



M306 enthält insbesondere:



\- UI-Text-Ressourcen

\- Übersetzungsschlüssel

\- sprachabhängige Label, Titel und Beschriftungen

\- Kultur-/Locale-Logik

\- Datums-, Zeit-, Zahlen- und Währungsformatierung

\- sprachabhängige Validierungs- und Fehlermeldungen

\- Sprachauswahl im UI

\- sprachabhängige Navigation und Modulbezeichnungen



\---



\## Zielbild



M306 sorgt dafür, dass SWFC:



\- in mehreren Sprachen dargestellt werden kann

\- nicht auf hart codierte Oberflächentexte angewiesen ist

\- pro Benutzer oder Systemkontext eine Sprache verwenden kann

\- kulturabhängige Formate korrekt anzeigt

\- konsistente Texte über alle Seiten hinweg bereitstellt



\---



\## Struktur



M306 besteht logisch aus folgenden Bereichen:



\- M306.01 – UI Text Resources

\- M306.02 – Culture \& Formatting

\- M306.03 – Language Selection

\- M306.04 – Validation \& Messages Localization

\- M306.05 – Navigation \& Module Labels



\---



\## M306.01 – UI Text Resources



Zuständig für:



\- zentrale UI-Textschlüssel

\- sprachabhängige Ressourcen

\- Standardtexte der Oberfläche



Beispiele:



\- `Common.Save`

\- `Common.Cancel`

\- `Users.Title`

\- `Auth.Login`

\- `Navigation.Security`



Wichtig:



> UI-Texte sollen nicht unkontrolliert als harte Strings über Razor-Seiten verteilt werden



\---



\## M306.02 – Culture \& Formatting



Zuständig für:



\- Datumsformat

\- Zeitformat

\- Zahlenformat

\- Währungsformat

\- kulturabhängige Darstellung



Beispiele:



\- `de-DE`

\- `en-US`



Wichtig:



> M306 steuert die Darstellung, nicht die fachliche Bedeutung der Daten



\---



\## M306.03 – Language Selection



Zuständig für:



\- Auswahl der Sprache

\- Benutzerbezogene Sprache

\- systemweite Standardsprache

\- sprachabhängige Sitzungskontexte



Wichtig:



> Die eigentliche Konfigurationsspeicherung kann in M105 oder M102 liegen  

> die Sprachlogik und Auswertung gehört nach M306



\---



\## M306.04 – Validation \& Messages Localization



Zuständig für:



\- lokalisierte Validierungsfehler

\- lokalisierte Fehlermeldungen

\- lokalisierte Standardhinweise

\- sprachabhängige Benutzerhinweise



Beispiele:



\- Pflichtfeldmeldungen

\- Formularfehler

\- Standardwarnungen

\- Hinweis- und Erfolgsnachrichten



\---



\## M306.05 – Navigation \& Module Labels



Zuständig für:



\- sprachabhängige Modulnamen

\- Seitentitel

\- Menüeinträge

\- Sidebar- und Header-Texte

\- Navigationstexte



Wichtig:



> Navigation und Modulbezeichnungen sollen zentral sprachfähig sein und nicht pro Seite frei improvisiert werden



\---



\## Verknüpfungen



\### Zu M301 – AppShell



M306 liefert sprachabhängige Texte für:



\- Hauptnavigation

\- Layouttitel

\- globale Shell-Inhalte



\---



\### Zu M303 – Notifications



M306 liefert sprachabhängige Hinweise, Meldungen und Benachrichtigungstexte.



\---



\### Zu M305 – Forms / Input



M306 liefert sprachabhängige:



\- Labels

\- Placeholder

\- Validierungstexte

\- Formularhinweise



\---



\### Zu M102 – Organization / Users



M102 kann benutzerbezogene Sprachpräferenzen halten, die von M306 für die Darstellung genutzt werden.



Wichtig:



> M102 speichert ggf. die Benutzersprache  

> M306 nutzt sie für die UI-Darstellung



\---



\### Zu M105 – Configuration



M105 kann systemweite Spracheinstellungen verwalten, z. B.:



\- Standardsprache

\- erlaubte Sprachen

\- Default-Locale



M306 nutzt diese Konfiguration.



\---



\### Zu M103 – Authentication



Nach erfolgreicher Anmeldung kann M306 die sprachabhängige Darstellung anhand des Benutzerkontexts initialisieren.



\---



\## Harte Trennung



\### M306 ist zuständig für



\- Oberflächensprache

\- Übersetzung

\- Kultur-/Locale-Darstellung

\- sprachabhängige UI-Texte

\- sprachabhängige Navigation

\- sprachabhängige Standardmeldungen



\### M306 ist NICHT zuständig für



\- Berechtigungsentscheidungen

\- Rollen-/Rechteprüfung

\- Login-Logik

\- Fachlogik

\- Geschäftsregeln

\- inhaltliche Übersetzung fachlicher Stammdaten

\- Dokumenteninhalt als Fachverantwortung



\---



\## Regeln



\### 1. Keine unkontrollierte Hartcodierung



Neue UI-Texte sollen nicht unstrukturiert und beliebig über Razor-Seiten verteilt werden.



\### 2. Einheitliche Textquellen



Standardtexte, Buttons, Navigation und allgemeine Meldungen sollen zentralisierbar sein.



\### 3. Kulturabhängige Darstellung sauber trennen



Datums-, Zeit-, Zahlen- und Währungsformate müssen aus der gewählten Kultur ableitbar sein.



\### 4. Benutzerfreundliche Umschaltung



Sprachumschaltung soll kontrolliert und nachvollziehbar erfolgen, nicht zufällig oder pro Seite uneinheitlich.



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

\- unkontrollierte harte Sprachstrings als langfristige Zielstruktur



Grundsatz:



> Sprachfähigkeit wird sauber eingebaut, nicht pro Seite improvisiert



\---



\## Zielzustand



M306 ist fachlich sauber, wenn:



\- SWFC zentral sprachfähig ist

\- UI-Texte konsistent steuerbar sind

\- mindestens mehrere Sprachen möglich sind

\- Datum/Zeit/Zahlen korrekt kulturabhängig angezeigt werden

\- Validierungen und Standardmeldungen sprachfähig sind

\- Navigation und Modultexte sprachfähig sind



\---



\## Bedeutung im Gesamtsystem



M306 ist wichtig für:



\- professionelle Benutzerführung

\- internationale Nutzbarkeit

\- konsistente Oberfläche

\- wartbare UI-Texte

\- spätere Mehrsprachigkeit ohne wilden Umbau



\---



\## Status



\- als Modul vorgesehen

\- sinnvoll als Teil von M300 – Presentation

\- eigenständig abgrenzbar

\- für spätere Mehrsprachigkeit vorbereitet

\- Codex-tauglich


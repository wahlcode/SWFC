\# M401 – Import / Export



\## Zweck

M401 bildet die \*\*strukturierte Import- und Export-Schnittstelle\*\* des SWFC-Systems ab.



Es enthält alle technischen Prozesse rund um:

\- Datenimporte

\- Datenexporte

\- Datei-basierte Übertragung

\- Mapping von externen Daten in SWFC



M401 beantwortet die Frage:



> \*\*Wie gelangen strukturierte Daten in das System hinein und wieder heraus?\*\*



\---



\## Grundprinzip



> M401 enthält keine Fachlogik  

> M401 transportiert und transformiert Daten technisch



\---



\## Inhalt



M401 enthält:



\- Dateiimporte

\- Dateiexporte

\- Mapping externer Datenstrukturen

\- Validierung technischer Importformate

\- Übergabe an Fachmodule



\---



\## Unterstützte Formate



M401 unterstützt insbesondere:



\- CSV

\- Excel



Erweiterungen für weitere Formate sind möglich.



\---



\## Import



M401 unterstützt strukturierte Datenimporte für z. B.:



\- Assets

\- Lagerdaten

\- Energie-Messwerte

\- Stammdaten

\- externe Listen



Wichtig:



> Die fachliche Verarbeitung erfolgt im jeweiligen Fachmodul  

> M401 übernimmt Transport, Format und Mapping



\---



\## Export



M401 unterstützt strukturierte Exporte für z. B.:



\- Berichte

\- Stammdaten

\- Bewegungsdaten

\- Audit-Nachweise

\- Austausch mit Fremdsystemen



\---



\## Mapping



M401 enthält Mapping-Mechanismen für:



\- Spaltenzuordnung

\- Feldzuordnung

\- Formatkonvertierung

\- strukturierte Übergabe an SWFC



Ziel:



> Externe Datenformate dürfen intern nicht die Systemstruktur diktieren



\---



\## Validierung



M401 prüft technische Importaspekte wie:



\- Dateiformat

\- Pflichtspalten

\- Grundstruktur

\- lesbare Datensätze



Wichtig:



> Fachliche Validierung bleibt in den Zielmodulen



\---



\## Fehlerbehandlung



M401 unterstützt:



\- Importfehleranzeige

\- Protokollierung

\- nachvollziehbare Fehlermeldungen

\- teilweises Ablehnen fehlerhafter Datensätze



\---



\## Verknüpfung



M401 arbeitet mit:



\- M200-Fachmodulen

\- M302 – Reporting (Exportnutzung)

\- M104 – Documents (optional bei Dateiablage)



\---



\## Regeln



\### Keine Fachlogik

M401 enthält nicht:



\- Wartungslogik

\- Energieberechnung

\- Lagerlogik

\- Qualitätslogik



\---



\### Technische Verantwortung

M401 ist zuständig für:



\- Übernahme

\- Ausgabe

\- Formatbehandlung

\- Mapping



Nicht zuständig für:



\- fachliche Entscheidungen

\- fachliche Regelprüfung



\---



\## Bedeutung im Gesamtsystem



M401 ist zentral für:



\- Massenimporte

\- Datenaustausch

\- externe Listen

\- technische Übergabe zwischen Systemen

\- Audit- und Berichtsexporte



\---



\## Kernsatz



> \*\*M401 = technische Ein- und Ausfuhr strukturierter Daten\*\*



\---



\## Status

\- fachlich definiert

\- architektonisch geprüft

\- verbindlich für SWFC


\# M202 – Maintenance



\## Zweck

M202 bildet die \*\*Wartungs- und Instandhaltungslogik\*\* des Unternehmens ab.



Es enthält alle fachlichen Prozesse rund um:

\- geplante Wartung

\- ungeplante Wartung (Störungen)

\- Soforteinsätze / Notdienst

\- Wartungshistorie



M202 beantwortet die Frage:



> \*\*Was wurde wann an welcher Anlage gewartet und warum?\*\*



\---



\## Inhalt



M202 enthält:



\- Wartungspläne  

\- Wartungsaufträge  

\- Wartungshistorie  

\- ungeplante Wartung / Störungen  

\- Notdienst / Soforteinsätze  

\- Prioritäten / Kritikalität  

\- Statusmodell (z. B. geplant, in Arbeit, abgeschlossen, abgebrochen)  



\---



\## Verknüpfungen



\### Zu M201 – Assets

Jede Wartung bezieht sich auf:



\- Anlagen  

\- Maschinen  

\- Komponenten  



> Wartung ohne technischen Bezug ist nicht zulässig



\---



\### Zu M204 – Inventory

Material wird nur \*\*referenziert\*\*:



\- Ersatzteile  

\- Verbrauchsmaterial  



Wichtig:



> \*\*M202 verändert niemals Lagerbestände\*\*



\---



\### Zu M104 – Documents

Wartung kann Dokumente verknüpfen:



\- Arbeitsanweisungen  

\- Wartungsberichte  

\- Bilder / Nachweise  

\- Prüfprotokolle  



\---



\### Zu M102 – Organization

Wartung kann zugeordnet werden zu:



\- Personen  

\- Teams  

\- Verantwortlichen  



\---



\## Planung / Logik



M202 enthält die fachliche Logik für:



\- Wartungsintervalle (z. B. Zeit, Nutzung)

\- Fälligkeiten

\- Planung von Wartungen



Wichtig:



> \*\*Die Ausführung (Scheduler etc.) gehört später zu M500\*\*



\---



\## Regeln



\### Datenhaltung

\- keine echte Löschung  

\- nur Historisierung / Statusänderung / Abschluss  



\---



\### Abgrenzung



M202 enthält \*\*nicht\*\*:



\- technische Struktur (→ M201)  

\- Lagerlogik (→ M204)  

\- Energieverbrauch (→ M205)  

\- Live-Daten / Steuerung (→ M500)  



\---



\## Prozessmodell



Ein Wartungsauftrag durchläuft Zustände:



\- geplant  

\- in Arbeit  

\- abgeschlossen  

\- abgebrochen  



Zusätzlich:



\- Priorität / Kritikalität steuerbar  



\---



\## Bedeutung im Gesamtsystem



M202 ist zentral für:



\- IATF-Konformität  

\- Nachvollziehbarkeit  

\- Wartungshistorie  

\- Stillstandsvermeidung  

\- Kostenkontrolle  



\---



\## Kernsatz



> \*\*M202 = vollständige fachliche Wahrheit der Instandhaltung\*\*



\---



\## Status

\- fachlich definiert  

\- architektonisch geprüft  

\- verbindlich für SWFC  


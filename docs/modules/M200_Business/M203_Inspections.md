\# M203 – Inspections



\## Zweck

M203 bildet die \*\*Prüf- und Inspektionslogik\*\* des Unternehmens ab.



Es enthält alle fachlichen Prozesse rund um:

\- Prüfungen

\- Inspektionen

\- Bewertungen

\- Freigaben



M203 beantwortet die Frage:



> \*\*Ist ein Objekt in Ordnung und entspricht es den Anforderungen?\*\*



\---



\## Abgrenzung zu M202



> \*\*M203 prüft und bewertet\*\*  

> \*\*M202 wartet und verändert\*\*



Diese Trennung ist verbindlich.



\---



\## Inhalt



M203 enthält:



\- Prüfpläne  

\- Prüfaufträge / Prüfungen  

\- Prüfergebnisse  

\- wiederkehrende Prüfzyklen  

\- Prüfstatus / Freigabezustände  



\---



\## Ergebnislogik



Prüfungen können Ergebnisse haben wie:



\- bestanden  

\- nicht bestanden  

\- mit Mangel  

\- Nachprüfung erforderlich  



\---



\## Verknüpfungen



\### Zu M201 – Assets

Prüfungen beziehen sich auf:



\- Anlagen  

\- Maschinen  

\- Komponenten  



> Prüfung ohne technischen Bezug ist nicht zulässig



\---



\### Zu M104 – Documents

Prüfungen können Dokumente verknüpfen:



\- Prüfberichte  

\- Bilder  

\- Checklisten  

\- Nachweise  



\---



\### Zu M102 – Organization

Prüfungen können zugeordnet werden zu:



\- Personen  

\- Teams  

\- Verantwortlichen  



\---



\## Folgeaktionen



Ein Prüfergebnis kann Folgeaktionen auslösen:



\- Wartung (M202)  

\- Qualitätsmeldung (M207)  

\- Sicherheitsmaßnahme (M208)  



Wichtig:



> M203 löst aus, übernimmt aber nicht die Logik dieser Module



\---



\## Planung / Logik



M203 enthält die fachliche Logik für:



\- Prüfintervalle  

\- Prüfzyklen  

\- Fälligkeiten  



Wichtig:



> Technische Ausführung (Scheduler etc.) gehört zu M500



\---



\## Regeln



\### Datenhaltung

\- keine echte Löschung  

\- nur Historisierung / Abschluss / Statusänderung  



\---



\### Abgrenzung



M203 enthält \*\*nicht\*\*:



\- Wartungslogik (→ M202)  

\- Qualitätsmanagement insgesamt (→ M207)  

\- Sicherheitslogik (→ M208)  

\- Live-Daten / Steuerung (→ M500)  



\---



\## Bedeutung im Gesamtsystem



M203 ist zentral für:



\- Qualitätsprüfung  

\- Sicherheitsprüfungen  

\- Auditfähigkeit  

\- Nachweisführung  

\- Zustandsbewertung von Anlagen  



\---



\## Kernsatz



> \*\*M203 = fachliche Wahrheit aller Prüfungen und Bewertungen\*\*



\---



\## Status

\- fachlich definiert  

\- architektonisch geprüft  

\- verbindlich für SWFC  


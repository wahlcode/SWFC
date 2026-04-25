\# M201 – Assets



\## Zweck

M201 bildet den \*\*technischen Zwilling der Anlagenstruktur\*\* ab.



Es enthält ausschließlich die technische Struktur des Betriebs, insbesondere:

\- Anlagen

\- Maschinen

\- Baugruppen

\- Komponenten

\- technische Hierarchien



M201 beantwortet die Frage:



> \*\*Was gehört technisch zusammen?\*\*



\---



\## Inhalt



M201 enthält:



\- technische Stammdaten

\- Statusinformationen (z. B. aktiv / inaktiv)

\- Asset-Typen

\- Parent/Child-Hierarchien

\- freie / erweiterbare Felder

\- Verknüpfungen zu organisatorischen Einheiten (M102)

\- optionale Verknüpfungen zu Energieobjekten (M205)



\---



\## Strukturprinzip



M201 bildet eine \*\*echte technische Baumstruktur\*\*.



Beispiel:



\- Anlage

&#x20; - Baugruppe

&#x20;   - Motor

&#x20;   - Pumpe

&#x20;   - Sensor



\### Verbindliche Regeln



\- Ein Asset hat \*\*genau einen Parent\*\*

\- Parent/Child-Beziehungen bilden eine \*\*Baumstruktur\*\*

\- \*\*keine Zyklen\*\* erlaubt

\- Parent nur innerhalb derselben technischen Anlagenstruktur



\---



\## Komponentenregel



Wenn mehrere Anlagen „dieselbe Komponente“ nutzen, bedeutet das \*\*nicht\*\*, dass ein einziges Asset mehreren Anlagen zugeordnet wird.



Stattdessen gilt:



\- jede physische Komponente ist eine \*\*eigene Instanz\*\*

\- gleiche Modelle / Bauarten werden über \*\*Asset-Typen\*\* abgebildet



\### Beispiel



Richtig:



\- Anlage 1

&#x20; - Motor A (Typ: Siemens XYZ)



\- Anlage 2

&#x20; - Motor B (Typ: Siemens XYZ)



Falsch:



\- ein einzelner Motor hängt gleichzeitig an Anlage 1 und Anlage 2



\### Kernsatz



> \*\*Gemeinsamkeit über Typen, nicht über Mehrfachzuordnung derselben Instanz\*\*



\---



\## Verknüpfungen



\### Zu M102 – Organization

Assets dürfen organisatorisch verknüpft werden, z. B. mit:



\- Standort

\- Abteilung

\- Verantwortungsbereich



Wichtig:



> \*\*M102 = organisatorischer Zwilling\*\*  

> \*\*M201 = technischer Zwilling\*\*



Beide dürfen verknüpft, aber \*\*niemals vermischt\*\* werden.



\---



\### Zu M205 – Energy

Assets dürfen optional mit Energieobjekten verknüpft werden, z. B.:



\- Maschine ↔ Zähler

\- Anlage ↔ Energiebezug



Das ermöglicht später:



\- Energieauswertung pro Anlage / Maschine

\- ISO-50001-konforme Zuordnung

\- technische Energieanalysen



\---



\## Regeln



\### Datenhaltung

\- keine echte Löschung

\- nur Deaktivierung / Historisierung



\### Abgrenzung

M201 enthält \*\*nicht\*\*:



\- Wartungslogik

\- Energieverbrauchslogik

\- Lagerbestand

\- Live-Daten

\- Steuerung



Diese gehören in:



\- M202 – Maintenance

\- M205 – Energy

\- M204 – Inventory

\- M500 – Runtime



\---



\## Bedeutung im Gesamtsystem



M201 ist die Grundlage für:



\- Wartung auf Anlagen- und Komponentenebene

\- Energiebezug zu Maschinen / Anlagen

\- Ersatzteil- und Materialbezug

\- spätere Runtime- / Leitwartenstruktur

\- technische Nachvollziehbarkeit



\---



\## Status

\- fachlich definiert

\- architektonisch geprüft

\- verbindlich für SWFC


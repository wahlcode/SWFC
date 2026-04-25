\# M204 – Inventory



\## Zweck

M204 bildet die \*\*Material-, Lager- und Bestandslogik\*\* des Unternehmens ab.



Es enthält alle fachlichen Prozesse rund um:

\- Artikel

\- Lager

\- Bestände

\- Bewegungen

\- Buchungen



M204 beantwortet die Frage:



> \*\*Wo befindet sich welches Material in welcher Menge und warum?\*\*



\---



\## Inhalt



M204 enthält:



\- Artikel  

\- Lager  

\- Lagerorte  

\- Bestände  

\- Bewegungen  

\- Buchungen (z. B. Wartung, Projekt)  

\- Preise / Werte  



\---



\## Zentrale Regel



> \*\*Nur M204 darf Bestände verändern\*\*



Andere Module (z. B. M202, M206, M209):

\- dürfen Material referenzieren

\- dürfen Buchungen auslösen

\- verändern Bestände aber nicht selbst



\---



\## Bewegungsprinzip



Bestände entstehen ausschließlich durch:



\- Zugänge  

\- Abgänge  

\- Umlagerungen  

\- Korrekturen  



> \*\*Kein direktes Setzen von Beständen erlaubt\*\*



\---



\## Verknüpfungen



\### Zu M201 – Assets

Material kann gebucht werden auf:



\- Anlagen  

\- Maschinen  

\- Komponenten  



Beispiel:

\- Ersatzteil wird an Maschine verbaut



\---



\### Zu M202 – Maintenance

Wartung kann Material verwenden:



\- Ersatzteile  

\- Verbrauchsmaterial  



Wichtig:



> M202 referenziert Material, M204 bucht



\---



\### Zu M209 – Projects

Material kann projektspezifisch gebucht werden.



\---



\## Bewertung / Preise



Artikel können enthalten:



\- Preise  

\- Werte  



Ermöglicht:



\- Kostenberechnung  

\- Projektkosten  

\- Wartungskosten  



\---



\## Inventur / Korrektur



M204 erlaubt:



\- Inventurprozesse  

\- Bestandskorrekturen  



Wichtig:



\- jede Korrektur ist nachvollziehbar  

\- Begründung erforderlich  



\---



\## Erweiterungen



System ist vorbereitet für:



\- Barcode  

\- Scanner  

\- mobile Nutzung  



\---



\## Regeln



\### Datenhaltung

\- keine Löschung von Bewegungen  

\- vollständige Historie  



\---



\### Abgrenzung



M204 enthält \*\*nicht\*\*:



\- Wartungslogik (→ M202)  

\- Einkaufslogik (→ M206)  

\- Projektlogik (→ M209)  

\- technische Struktur (→ M201)  



\---



\## Bedeutung im Gesamtsystem



M204 ist zentral für:



\- Materialfluss  

\- Kostenkontrolle  

\- Wartung  

\- Projekte  

\- Nachvollziehbarkeit  



\---



\## Kernsatz



> \*\*M204 = einzige Quelle für alle Bestandsveränderungen\*\*



\---



\## Status

\- fachlich definiert  

\- architektonisch geprüft  

\- verbindlich für SWFC  


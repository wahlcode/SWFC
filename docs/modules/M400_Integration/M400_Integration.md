\# M400 – Integration



\## Zweck

M400 bildet die \*\*Integrationsschicht des SWFC-Systems\*\* ab.



Es enthält alle technischen Schnittstellen zu:

\- externen Systemen

\- Maschinen

\- Datenquellen

\- Kommunikationsdiensten



M400 beantwortet die Frage:



> \*\*Wie kommuniziert SWFC mit der Außenwelt?\*\*



\---



\## Struktur



M400 besteht aus:



\- M401 – Import / Export  

\- M402 – API  

\- M403 – ERP  

\- M404 – IoT / Maschinen  

\- M405 – Messaging / Events  

\- M406 – Identity Integration  

\- M407 – DMS / File Integration  



\---



\## Grundprinzipien



\### 1. Trennung von Fachlogik

\- M400 enthält keine Fachlogik  

\- nur technische Kommunikation  



\---



\### 2. Schnittstellenorientierung

\- alle externen Verbindungen laufen über definierte Module  

\- keine direkten Verbindungen aus Fachmodulen  



\---



\### 3. Erweiterbarkeit

\- neue Systeme können über neue Integrationen angebunden werden  

\- ohne Änderungen an bestehenden Modulen  



\---



\## Abgrenzung



M400 enthält \*\*nicht\*\*:



\- Fachprozesse (→ M200)  

\- UI (→ M300)  

\- Runtime-Steuerung (→ M500)  



\---



\## Bedeutung im Gesamtsystem



M400 ist zentral für:



\- ERP-Anbindung (z. B. SAP)

\- Maschinenintegration

\- Datenimporte/-exporte

\- Systemvernetzung

\- externe Kommunikation



\---



\## Kernsatz



> \*\*M400 = Verbindungsschicht zwischen SWFC und allen externen Systemen\*\*



\---



\## Status

\- vollständig definiert

\- architektonisch geprüft

\- zukunftssicher aufgebaut

\- verbindlich für SWFC


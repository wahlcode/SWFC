# M601 - Roadmap

## Zweck

M601 beschreibt die strategische und operative Entwicklungsreise des SWFC-Systems.
Die Roadmap macht sichtbar, welche fachlichen Ausbaustufen geplant sind, welche Module daran beteiligt sind und welchen Nutzen eine Version für Benutzer und Betrieb liefert.

## Verantwortung

M601 gehört zu Planning.
Das Modul verantwortet Planung, Reihenfolge, Zielbild und nachvollziehbare Entwicklungsphasen.

## Inhalt

- Versionen von v0.1.0 bis v1.0.0
- Ziele und Nutzen je Version
- primäre Module je Version
- erforderliche Module und Abhängigkeiten
- Meilensteine und erwartete Ergebnisse

## Abgrenzung

- Roadmap ist Planung, nicht technische Ausführung.
- Modulübersicht zeigt den aktuellen Modulzustand.
- Audit prüft den realen Stand gegen Code, Dokumentation und Tests.
- Roadmap berechnet keine Modul- oder Auditstatuswerte selbst.

## Regeln

- Roadmapdaten kommen aus `roadmap.json`.
- `modules.json` bleibt die Quelle für Modulstruktur und Plan-WorkItems.
- Auditdaten kommen aus der bestehenden Audit-Prüfung und `status/generated`.
- Änderungen an Roadmap-Zielen müssen nachvollziehbar bleiben.
- Keine Implementierungslogik in der M601-Dokumentation.

## Kernsatz

Roadmap erzählt die Systemreise. Modulübersicht zeigt den Zustand. Audit prüft die Realität.

# M805 – Audit Logging

---

## 🎯 Zweck

M805 bildet die **revisionssichere Nachvollziehbarkeit aller sicherheits- und fachlich relevanten Aktionen** im SWFC-System ab.

Es ist zuständig für:

- AuditLog
- AuditService
- Protokollierung sicherheitsrelevanter Ereignisse
- Nachvollziehbarkeit von Änderungen und Aktionen
- Auth-, Access-, Freigabe- und Konfigurationsereignisse
- revisionsfähige Prüfspur

M805 beantwortet die Frage:

> **Wer hat wann was in welchem Kontext getan oder versucht zu tun?**

---

## 🧠 Kernsatz

> M805 = revisionssichere Nachvollziehbarkeit des Systems

---

## Inhalt

M805 enthält:

- Audit-Einträge
- Audit-Service
- Änderungsprotokollierung
- Aktionsprotokollierung
- Authentifizierungsereignisse
- Autorisierungsereignisse
- Freigabeereignisse
- Konfigurationsereignisse
- Vorher-/Nachher-Kontext, wenn fachlich relevant
- Actor-, Objekt- und Zeitbezug

---

## Pflichtfelder

Ein auditrelevanter Eintrag muss mindestens nachvollziehbar machen:

- wer
- wann
- was
- in welchem Kontext
- an welchem Objekt
- mit welchem Ergebnis

Wenn fachlich relevant zusätzlich:

- vorher
- nachher
- Begründung
- Freigabekontext
- technischer Auslöser

---

## SSO- / Security-Bezug

M805 muss insbesondere auditieren:

- Login erfolgreich
- Login fehlgeschlagen
- Logout
- SSO-Login gestartet
- SSO-Callback erfolgreich / fehlgeschlagen
- externer Benutzerlink erstellt / geändert / gelöst
- Rechteentscheidung für kritische Aktion
- privilegierte Aktion erlaubt / verweigert
- Freigabe erteilt / verweigert
- Provider-Konfiguration geändert
- Sicherheitsrelevante Policy geändert
- Break-Glass-Login verwendet

---

## Verknüpfungen

### Zu M103 – Authentication

M103 liefert Auth-Ereignisse an M805.

### Zu M406 – Identity Integration

M406 liefert Provider-/Identity-Änderungen und kritische Integrationsereignisse an M805.

### Zu M806 – Access Control

M806 liefert kritische Zugriffsentscheidungen, Freigaben und privilegierte Aktionen an M805.

### Zu M105 – Configuration

Sicherheits- oder systemkritische Konfigurationsänderungen müssen auditierbar sein.

### Zu M504 / M500

Steuernde und sicherheitsrelevante Runtime-Aktionen müssen auditierbar sein.

---

## Regeln

### Keine Löschung

Für auditrelevante Daten gilt:

- keine Löschung
- vollständige Nachvollziehbarkeit
- revisionsfähige Speicherung

---

### Pflicht zur Zuordenbarkeit

Jede relevante Aktion muss einer eindeutigen Identität oder einem technischen Auslöser zuordenbar sein.

---

### Kein stilles Überschreiben

Audit darf nicht unbemerkt überschrieben, ersetzt oder versteckt werden.

---

## Harte Trennung

### M805 ist zuständig für

- Audit-Speicherung
- Audit-Struktur
- Nachvollziehbarkeit
- revisionsfähige Ereignisführung

### M805 ist NICHT zuständig für

- Authentifizierung
- Berechtigungsentscheidung
- Provider-Anbindung
- Benutzerstammdatenpflege
- Security Monitoring
- Fachprozessentscheidung

---

## Leitwarten- und Sicherheitsregel

Für kritische Eingriffe gilt:

- Aktion muss auditpflichtig sein
- Benutzerbezug muss vorhanden sein
- Ergebnis muss nachvollziehbar sein
- Freigabekontext muss dokumentiert sein, wenn erforderlich

---

## UI-Regeln

Erlaubt:

- Razor-Seiten (`.razor`)
- lokale Styles (`.razor.css`)
- bestehende globale Styles (`theme.css`)

Verboten:

- generierte Razor-Dateien (`.rz`)
- UI-Scaffolding
- automatisch generierte Audit-Oberflächen

Grundsatz:

> Audit-UI darf existieren, aber nicht generiert werden

---

## Abgrenzung

M805 enthält **nicht**:

- Login-/Session-Hauptlogik
- Allow-/Deny-Hauptlogik
- Provider-Registry
- Benutzerstammdaten
- Alarm-/Monitoring-Hauptlogik

---

## Bedeutung im Gesamtsystem

M805 ist zentral für:

- Revisionssicherheit
- IATF- und ISO-50001-Tauglichkeit
- Nachvollziehbarkeit sicherheitskritischer Aktionen
- Vertrauen in Auth-, Access- und Runtime-Prozesse

---

## Status

- verbindlich definiert
- revisionskritisch
- SSO-/Security-tauglich
- getrennt von M103, M406 und M806
- Codex-tauglich
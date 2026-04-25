# M806 – Access Control

---

## 🎯 Zweck

M806 bildet die **zentrale Berechtigungs- und Zugriffskontrolllogik** des SWFC-Systems ab.

Es ist zuständig für:

- rollenbasierten Zugriff (RBAC)
- attributbasierten Zugriff (ABAC)
- Kontextregeln
- Allow / Deny
- Vererbung
- Sichtbarkeit vs. Aktion
- Freigaben für sicherheitskritische Aktionen
- internes Mapping externer Gruppen-/Rollenhinweise auf SWFC-Berechtigungen

M806 beantwortet die Frage:

> **Darf Benutzer X im aktuellen Kontext Objekt Y sehen oder Aktion Z ausführen?**

---

## 🧠 Kernsatz

> M806 = zentrale Entscheidungsinstanz für Berechtigungen

---

## Inhalt

M806 enthält:

- Zugriffsregeln
- Rollenmodelle
- Attributregeln
- Kontextregeln
- Sichtbarkeitsregeln
- Aktionsregeln
- Freigaberegeln
- Vererbungsregeln
- Allow-/Deny-Auswertung
- internes Gruppen-/Rollenmapping
- Entscheidungsbegründung
- prüfbare Zugriffsergebnisse

---

## Grundregel

M806 ist die **einzige zentrale Instanz** für Berechtigungsentscheidungen.

Wichtig:

> keine verteilte Schattenlogik in Fachmodulen  
> keine UI-eigene Rechteentscheidung  
> keine API-Bypässe

---

## SSO- / OIDC-Bezug

Externe Identity-Provider dürfen Hinweise liefern wie:

- Gruppen
- Rollen
- Claims
- organisatorische Marker

Diese werden in M806 **nicht blind übernommen**.

Stattdessen gilt:

- externe Gruppen/Rollen = Input
- interne SWFC-Mapping-Regeln = Entscheidung
- Rechte = nur nach interner Auswertung gültig

---

## Sichtbarkeit vs. Aktion

M806 muss Sichtbarkeit und Aktion getrennt auswerten.

Beispiele:

- Benutzer darf Anlage sehen, aber nicht bearbeiten
- Benutzer darf Wartungsauftrag lesen, aber nicht abschließen
- Benutzer darf Leitwarte sehen, aber nicht steuernd eingreifen

Diese Trennung ist verbindlich.

---

## Freigaben

M806 unterstützt Freigaben für sicherheitskritische Aktionen.

Beispiele:

- steuernder Eingriff
- sicherheitskritische Leitwartenaktion
- besonders geschützte Konfigurationsänderung

Wichtig:

> Freigabe ersetzt keine Sicherheitsprüfung aus M208  
> Freigabe ersetzt keine Auditpflicht aus M805

---

## Verknüpfungen

### Zu M103 – Authentication

M806 nutzt die authentifizierte Identität, erzeugt sie aber nicht selbst.

Wichtig:

> M103 sagt, wer angemeldet ist  
> M806 sagt, was erlaubt ist

---

### Zu M102 – Organization / Users

M806 darf organisatorische Kontextinformationen nutzen, z. B.:

- Benutzer
- Team
- Verantwortungsbereich
- Abteilung
- Schicht

Wichtig:

> M102 liefert Organisationsdaten  
> M806 bewertet sie im Zugriffsmodell

---

### Zu M406 – Identity Integration

M806 darf externe Gruppen-/Rollenhinweise als Input nutzen.

Wichtig:

> M406 liefert technische Herkunft  
> M806 trifft die SWFC-Entscheidung

---

### Zu M805 – Audit Logging

Berechtigungsentscheidungen und Freigaben müssen auditierbar sein, insbesondere:

- kritische Allow-/Deny-Entscheidungen
- Freigabe erteilt / verweigert
- Mapping-Regeln geändert
- privilegierte Aktion erlaubt
- privilegierte Aktion verweigert

---

## Entscheidungsregeln

M806 muss mindestens folgende Arten von Entscheidungen unterstützen:

- `CanView`
- `CanCreate`
- `CanUpdate`
- `CanDelete` (nur wenn fachlich erlaubt)
- `CanApprove`
- `CanExecute`
- `CanControl`
- `CanAdminister`

---

## Harte Trennung

### M806 ist zuständig für

- Rechteentscheidung
- Sichtbarkeit vs. Aktion
- Rollen-/Attributauswertung
- zentrale Zugriffslogik
- internes Mapping externer Rollenhinweise

### M806 ist NICHT zuständig für

- Authentifizierung
- Session-Erzeugung
- Benutzerstammdatenpflege
- Provider-Anbindung
- Audit-Speicherung
- Sicherheitsüberwachung
- physische Sicherheitsprüfung
- Fachprozesslogik

---

## Leitwartenregel

Für Leitwarte / Runtime gilt verbindlich:

- M504 darf anzeigen und bedienen
- M806 entscheidet, ob die Aktion erlaubt ist
- M208 entscheidet, ob sie sicherheitsseitig zulässig ist
- M805 protokolliert die Aktion

---

## UI-Regeln

Erlaubt:

- Razor-Seiten (`.razor`)
- lokale Styles (`.razor.css`)
- bestehende globale Styles (`theme.css`)

Verboten:

- generierte Razor-Dateien (`.rz`)
- UI-Scaffolding
- automatisch generierte Rechte-Admin-Oberflächen

Grundsatz:

> Rechte-UI darf existieren, aber nicht generiert werden

---

## Abgrenzung

M806 enthält **nicht**:

- Login-Flow
- Provider-Konfiguration
- Benutzerstammdaten-Hauptlogik
- Audit-Storage
- Security Monitoring
- Lizenzlogik

---

## Bedeutung im Gesamtsystem

M806 ist zentral für:

- sichere Modulzugriffe
- Runtime-/Leitwartenfreigaben
- sensible Datenzugriffe
- konsistente Zugriffsentscheidungen
- stabile Trennung zwischen Identität und Berechtigung

---

## Status

- verbindlich definiert
- zentrale Sicherheitsinstanz für Berechtigungen
- SSO-/OIDC-kompatibel
- getrennt von M103, M102, M406 und M805
- Codex-tauglich
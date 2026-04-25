# M406 – Identity Integration

---

## 🎯 Zweck

M406 bildet die **technische Integration externer Identitätssysteme** in SWFC ab.

Es ist zuständig für:

- SSO-Provider-Anbindung
- OIDC-Metadaten
- Issuer-Vertrauen
- Provider-Konfiguration
- externe Identity-Provider wie AD, Azure AD, Keycloak, Okta oder andere OIDC-kompatible Systeme
- Claim-Quellen und technische Claim-Zuordnung
- Benutzerabgleich auf Integrationsseite

M406 beantwortet die Frage:

> **Wie wird ein externes Identitätssystem technisch an SWFC angebunden?**

---

## 🧠 Kernsatz

> M406 = technische Anbindung externer Identitätsquellen  
> nicht Login-Hauptlogik  
> nicht Session-Hauptlogik  
> nicht Berechtigungsentscheidung

---

## Inhalt

M406 enthält:

- Identity-Provider-Konfiguration
- OIDC-Provider-Registry
- Issuer-Definition
- Client-Konfiguration
- Redirect-URI-Kontext
- Post-Logout-Redirect-Kontext
- Scope-Konfiguration
- Claim-Mapping-Konfiguration
- Provider-Metadaten
- technische Verbindung zu AD / Azure AD / OIDC-Providern
- Benutzerabgleich im externen Identitätskontext

---

## Unterstützte Providerlogik

M406 ist provider-neutral.

Das bedeutet:

- keine Microsoft-spezifische Architektur
- keine Okta-spezifische Architektur
- keine Keycloak-spezifische Architektur
- standardisierte OIDC-Integration
- Provider-Austausch ohne Architekturbruch

---

## Claim-Mapping

M406 darf Claim-Mapping-Konfigurationen halten, z. B.:

- `sub`
- `email`
- `name`
- `roles`
- `groups`

Wichtig:

> M406 definiert die technische Herkunft und Zuordnung von Claims  
> die fachliche Rechteentscheidung daraus trifft nicht M406

---

## Verknüpfungen

### Zu M103 – Authentication

M406 liefert M103:

- Providerinformationen
- technische Endpunkte
- technische Metadaten
- Claim-Quellen
- notwendige Integrationsparameter

Wichtig:

> M406 bindet an  
> M103 nutzt die Anbindung im Login-/Session-Kontext

---

### Zu M102.02 – Users

M406 darf den Abgleich externer Identität mit internem Benutzer unterstützen.

Beispiele:

- externer Subject-Identifier
- E-Mail-Abgleich
- technischer Link zu internem User

Wichtig:

> der interne Benutzer bleibt in M102.02  
> M406 ist nicht Benutzerstammdaten-Hauptmodul

---

### Zu M806 – Access Control

M406 darf Hinweise wie Gruppen oder Rollen technisch liefern.

Wichtig:

> M406 entscheidet nicht, welche SWFC-Berechtigung daraus folgt

---

### Zu M805 – Audit Logging

Provider-bezogene Integrationsereignisse müssen auditierbar sein, insbesondere:

- Provider aktiviert / deaktiviert
- Claim-Mapping geändert
- Issuer geändert
- technische Identitätsverknüpfung erstellt / gelöst
- kritische Integrationsfehler

---

## Harte Trennung

### M406 ist zuständig für

- externe Provider-Anbindung
- Identity-Provider-Konfiguration
- technische Claims-Herkunft
- Integrationsmetadaten
- technische Benutzerverknüpfung auf Provider-Seite

### M406 ist NICHT zuständig für

- Login-Session-Hauptverantwortung
- SWFC-Session-Cookies
- Rechteentscheidung
- Rollenvergabe als Hauptverantwortung
- Benutzerstammdatenpflege
- Authentifizierung als Anwendungskontext-Hauptverantwortung

---

## Sicherheitsregeln

M406 muss sicherstellen:

- nur vertrauenswürdige Issuer
- Provider-Konfiguration ist kontrolliert
- Secrets werden nicht unkontrolliert gespeichert
- Secret-/Key-Handling folgt M807
- unsichere oder unvollständige Provider-Konfigurationen werden nicht blind akzeptiert

---

## Setup-Bezug

Die initiale Einrichtung eines Providers kann durch M107 vorbereitet werden.

Wichtig:

> M107 initialisiert  
> M406 betreibt die technische Identity-Integration zur Laufzeit

---

## UI-Regeln

Erlaubt:

- Razor-Seiten (`.razor`)
- lokale Styles (`.razor.css`)
- bestehende globale Styles (`theme.css`)

Verboten:

- generierte Razor-Dateien (`.rz`)
- UI-Scaffolding
- automatisch generierte Provider-Admin-Oberflächen

Grundsatz:

> Integrations-UI darf existieren, aber nicht generiert werden

---

## Abgrenzung

M406 enthält **nicht**:

- M103-Login-Flow-Hauptlogik
- M806-Allow/Deny-Entscheidungslogik
- M102-Benutzerstamm-Hauptlogik
- M805-Audit-Storage-Hauptlogik
- M807-Secrets-Hauptlogik

---

## Bedeutung im Gesamtsystem

M406 ist zentral für:

- austauschbare externe Identitätssysteme
- provider-neutrale SSO-Architektur
- stabile Identity-Anbindung
- saubere Trennung von externer Identität und interner SWFC-Session

---

## Status

- verbindlich definiert
- provider-neutral
- OIDC-fähig
- getrennt von M103, M102, M806 und M805
- Codex-tauglich
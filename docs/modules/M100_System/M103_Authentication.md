\# M103 – Authentication



\---



\## 🎯 Zweck



M103 bildet die \*\*Identitäts- und Anmeldelogik\*\* des SWFC-Systems ab.



Es ist zuständig für:



\- Login

\- Logout

\- SSO-Login

\- Session

\- Current User

\- Claims

\- Passwortregeln

\- MFA-Vorbereitung

\- OIDC-Login-Flow

\- interne Session-Erzeugung nach externer Authentifizierung



M103 beantwortet die Frage:



> \*\*Wer ist der Benutzer und wie wird eine gültige SWFC-Anmeldung hergestellt?\*\*



\---



\## 🧠 Kernsatz



> M103 = Identität und Anmeldung  

> nicht Berechtigung  

> nicht Benutzerstammdaten  

> nicht externe Provider-Integration als Hauptverantwortung



\---



\## Inhalt



M103 enthält:



\- Login-Start

\- Logout

\- lokale Anmeldung (falls aktiviert)

\- SSO-Login

\- Session-Erzeugung

\- Session-Verwaltung

\- Current User Context

\- ClaimsPrincipal-Erzeugung

\- Callback-Verarbeitung nach erfolgreicher externer Authentifizierung

\- ID-Token-Validierung im Authentifizierungsfluss

\- Auth-Fehlerzustände

\- Auth-Timeout / Session-Timeout

\- Break-Glass-Admin-Login, wenn systemseitig freigegeben



\---



\## OIDC / SSO



M103 unterstützt \*\*OIDC-basierte SSO-Anmeldung\*\*.



Dabei gilt verbindlich:



\- Authorization Code Flow

\- PKCE

\- `state` ist verpflichtend

\- `nonce` ist verpflichtend

\- serverseitiger Code-to-Token-Austausch

\- ID Token muss validiert werden

\- unvalidierte Fremdclaims dürfen nicht direkt verwendet werden



\---



\## OIDC-Endpunkte



M103 darf Anwendungsendpunkte enthalten wie:



\- `/auth/oidc/login`

\- `/auth/oidc/callback`

\- `/logout`



Wichtig:



> M103 bildet den Anmeldefluss  

> die externe Provider-Anbindung selbst liegt in M406



\---



\## Session-Regel



Nach erfolgreicher lokaler oder externer Authentifizierung erzeugt SWFC \*\*immer seine eigene interne Session\*\*.



Das bedeutet:



\- Fremdtokens sind nicht automatisch die SWFC-Session

\- Fachmodule arbeiten nicht direkt mit Provider-Tokens

\- Session-Cookie, Timeout und Logout-Verhalten werden intern kontrolliert



\---



\## Verknüpfungen



\### Zu M406 – Identity Integration



M103 nutzt M406 für:



\- Provider-Metadaten

\- OIDC-Endpunkte

\- externe Identitätssysteme

\- technische Provider-Anbindung



Wichtig:



> M103 authentifiziert im Anwendungskontext  

> M406 bindet den externen Provider technisch an



\---



\### Zu M102 – Organization / Users



M103 darf Benutzerbezüge zu internen Usern herstellen oder prüfen.



Wichtig:



> M103 verwaltet nicht die Benutzerstammdaten selbst  

> diese bleiben in M102.02



\---



\### Zu M806 – Access Control



M103 liefert die authentifizierte Identität, aber entscheidet \*\*nicht\*\* über Rechte.



Wichtig:



> authentifiziert ≠ berechtigt



\---



\### Zu M805 – Audit Logging



M103 muss Auth-relevante Ereignisse an M805 melden, insbesondere:



\- Login erfolgreich

\- Login fehlgeschlagen

\- Logout

\- Session erstellt

\- Session beendet

\- SSO-Callback fehlgeschlagen

\- ungültiges oder abgelaufenes Token

\- Break-Glass-Login



\---



\## Harte Trennung



\### M103 ist zuständig für



\- Authentifizierung

\- Session

\- Current User

\- Login / Logout

\- SSO-Anmeldefluss

\- Identitätsbestätigung



\### M103 ist NICHT zuständig für



\- Rollen- / Rechteentscheidung

\- zentrale Access-Regeln

\- Benutzerstammdatenpflege

\- externe Provider-Lifecycle-Verwaltung

\- Audit-Speicherung als Hauptverantwortung

\- Security Monitoring

\- Lizenzierung

\- Produktbetrieb



\---



\## Sicherheitsregeln



M103 muss erzwingen:



\- nur HTTPS im produktiven Einsatz

\- sichere Session-Cookies

\- `HttpOnly`

\- `Secure`

\- geeignetes `SameSite`-Verhalten

\- Schutz gegen CSRF im Login-Flow

\- Schutz gegen Replay/Mix-up durch `state` / `nonce`

\- keine Speicherung von Firmenpasswörtern bei reinem SSO-Betrieb



\---



\## Break-Glass-Regel



Ein lokaler Notfallzugang kann unterstützt werden, aber nur wenn systemseitig freigegeben.



Wichtig:



\- getrennt vom SSO

\- vollständig auditpflichtig

\- nicht als Standardpfad gedacht



\---



\## UI-Regeln



Erlaubt:



\- Razor-Seiten (`.razor`)

\- lokale Styles (`.razor.css`)

\- bestehende globale Styles (`theme.css`)



Verboten:



\- generierte Razor-Dateien (`.rz`)

\- UI-Scaffolding

\- automatisch generierte Login-/Auth-Seiten

\- Tool-generierte Auth-Oberflächen als Standardstruktur



Grundsatz:



> Auth-UI darf existieren, aber nicht generiert werden



\---



\## Abgrenzung



M103 enthält \*\*nicht\*\*:



\- M102-Benutzerstammdaten

\- M806-Berechtigungslogik

\- M406-Provider-Registry als Hauptverantwortung

\- M805-Audit-Storage-Logik

\- M808-Sicherheitsüberwachung



\---



\## Bedeutung im Gesamtsystem



M103 ist zentral für:



\- sicheren Systemzugang

\- interne Session-Kontrolle

\- SSO-Anmeldung

\- Nachvollziehbarkeit des Authentifizierungszustands

\- klare Trennung von Identität und Berechtigung



\---



\## Status



\- verbindlich definiert

\- OIDC-/SSO-fähig

\- getrennt von M102, M406, M805 und M806

\- Codex-tauglich


# M802 – Application Security

---

## 🎯 Zweck

M802 schützt die Anwendung selbst vor Angriffen.

---

## Inhalt

M802 definiert Schutzmaßnahmen gegen:

- Injection (SQL, Command, etc.)
- XSS
- CSRF
- unsichere Eingaben
- unsichere API-Aufrufe

---

## Regeln

- jede Eingabe muss validiert werden
- keine direkte Verarbeitung unvalidierter Daten
- API-Endpunkte müssen abgesichert sein
- keine direkte Nutzung von Fremdclaims ohne Prüfung

---

## Harte Anforderungen

- Input Validation verpflichtend
- Output Encoding verpflichtend
- CSRF-Schutz bei zustandsverändernden Requests
- sichere Fehlerbehandlung (keine Leaks)

---

## Abgrenzung

- keine Authentifizierung → M103
- keine Berechtigung → M806

---

## Status

- verbindlich
- sicherheitskritisch
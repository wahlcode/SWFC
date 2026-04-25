# M807 – Secrets & Key Management

---

## 🎯 Zweck

M807 verwaltet alle sicherheitskritischen Geheimnisse.

---

## Inhalt

- API Keys
- Tokens
- Zertifikate
- Verschlüsselungsschlüssel

---

## Regeln

- Secrets dürfen niemals im Klartext gespeichert werden
- Zugriff ist streng eingeschränkt
- Rotation muss möglich sein

---

## Anforderungen

- Secrets müssen versionierbar sein
- Zugriff muss auditierbar sein
- keine Hardcodierung

---

## Abgrenzung

- keine Authentifizierung → M103
- keine Zugriffsauswertung → M806

---

## Status

- hochkritisch
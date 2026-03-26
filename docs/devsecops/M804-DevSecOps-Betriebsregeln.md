# M804 â€“ DevSecOps Betriebsregeln

## 1. Commit-Regeln

Erlaubte Formate:

- eat(Mxxx.xx): description
- ix(Mxxx.xx): description
- chore(Mxxx.xx): description
- efactor(Mxxx.xx): description
- 	est(Mxxx.xx): description

## 2. Branch-Regeln

Erlaubte Formate:

- eature/Mxxx.xx-description
- ix/Mxxx.xx-description
- chore/Mxxx.xx-description
- efactor/Mxxx.xx-description
- 	est/Mxxx.xx-description

## 3. Pull-Request-Regeln

Jeder PR muss enthalten:

- Task-ID
- Modul
- CORE / EXT
- Beschreibung
- Risiko
- betroffene Layer
- Checkliste

## 4. Pflicht-Checks

- Build
- Test
- Commit Message Check
- Branch Name Check
- Structure Check
- Architecture Check
- Secret Scan
- Dependency Check

## 5. Verhalten bei Fehlern

- Kein Merge bei roten Checks
- Keine Ausnahmen im Alltag
- Fehler zuerst beheben, dann erneut pushen
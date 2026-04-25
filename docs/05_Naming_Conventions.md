# SWFC – NAMING CONVENTIONS & CODE PATTERNS

---

# ZIEL

Diese Datei definiert:

- verbindliche Benennungsregeln
- Code-Strukturregeln
- Muster für Module, Dateien und Klassen
- einheitliche Schreibweise im gesamten Projekt

Ziel:
- klare Lesbarkeit
- keine Zufallsnamen
- keine inkonsistente Struktur
- bessere Wartbarkeit

---

# 1. ALLGEMEINE GRUNDSÄTZE

## 1.1 Sprache
- Code, Klassen, Dateien und Properties in Englisch
- UI-Texte dürfen Deutsch sein
- Modulnamen bleiben in SWFC-Notation (`M201-Assets`, `M205-Energy`)

## 1.2 Schreibweise
- Klassen: `PascalCase`
- Methoden: `PascalCase`
- Properties: `PascalCase`
- private Felder: `_camelCase`
- Parameter: `camelCase`
- lokale Variablen: `camelCase`

## 1.3 Dateiname = Hauptinhalt
- Dateiname muss zum wichtigsten Typ passen
- keine Fantasienamen
- keine Sammelnamen außer bewusst für Bündelung

Beispiele:
- `MachineComponent.cs`
- `CreateMaintenanceOrder.cs`
- `EnergyMeterDtos.cs`

---

# 2. MODULNAMEN

## 2.1 Modulordner
Format:

`Mxxx-Name`

Beispiele:
- `M101-Foundation`
- `M102-Organization`
- `M201-Assets`
- `M205-Energy`
- `M806-AccessControl`

## 2.2 Unterordner
Unterordner fachlich benennen, nicht technisch aufblasen.

Beispiele:
- `Machines`
- `MachineComponents`
- `MaintenanceOrders`
- `EnergyMeters`

Nicht:
- `Helpers`
- `Stuff`
- `Misc`
- `Temp`

---

# 3. DOMAIN NAMING

## 3.1 Entities
Format:
`<Thing>.cs`

Beispiele:
- `Machine.cs`
- `MachineComponent.cs`
- `MaintenanceOrder.cs`
- `EnergyMeter.cs`

## 3.2 Value Objects
Format:
`<Thing><Meaning>.cs`

Beispiele:
- `MachineName.cs`
- `MachineComponentName.cs`
- `OrganizationUnitCode.cs`

## 3.3 Enums
Format:
`<Thing>Status.cs`
`<Thing>Type.cs`
`<Thing>Mode.cs`

Beispiele:
- `MaintenanceOrderStatus.cs`
- `EnergyMediumType.cs`
- `CaptureMode.cs`

## 3.4 Exceptions
Format:
`<BusinessMeaning>Exception.cs`

Beispiel:
- `InvalidMachineComponentHierarchyException.cs`

## 3.5 Rules
Format:
`<Thing>Rules.cs`

Beispiel:
- `MachineComponentHierarchyRules.cs`

Nur wenn echte Regelklasse nötig.

---

# 4. APPLICATION NAMING

## 4.1 Grundregel
Application ist feature-orientiert, nicht technisch zerschnitten.

Keine Hauptordner wie:
- `Commands`
- `Queries`
- `Handlers`
- `Validators`

## 4.2 UseCases
Format:
- `Create<Thing>.cs`
- `Update<Thing>.cs`
- `Get<Things>.cs`
- `Get<Thing>ById.cs`
- `Set<Thing>ActiveState.cs`
- `Move<Thing>.cs`
- `Delete<Thing>.cs` (nur wenn fachlich erlaubt)

Beispiele:
- `CreateMachineComponent.cs`
- `UpdateMachineComponent.cs`
- `GetMachineComponentsByMachine.cs`
- `SetMachineComponentAreaActiveState.cs`

## 4.3 DTO-Dateien
Format:
`<Thing>Dtos.cs`

Beispiele:
- `MachineComponentDtos.cs`
- `MaintenanceOrderDtos.cs`
- `EnergyMeterDtos.cs`

## 4.4 Interface-Dateien
Format:
- `I<Thing>Repositories.cs`
- `I<Thing>Services.cs`
- `I<Thing>Resolver.cs`

Beispiele:
- `IMachineComponentRepositories.cs`
- `IAccessRuleRepositories.cs`
- `IVisibilityResolver.cs`

## 4.5 Resolver / Orchestrierung
Format:
- `<Thing>Resolver.cs`
- `<Thing>Contracts.cs`

Beispiele:
- `VisibilityResolver.cs`
- `VisibilityResolverContracts.cs`

---

# 5. INFRASTRUCTURE NAMING

## 5.1 DbContext
Format:
`AppDbContext.cs`

## 5.2 EF Configurations
Format:
`<Thing>Configuration.cs`

Beispiele:
- `MachineComponentConfiguration.cs`
- `EnergyMeterConfiguration.cs`
- `AccessRuleConfiguration.cs`

## 5.3 Repositories
Format:
- `<Thing>ReadRepository.cs`
- `<Thing>WriteRepository.cs`

Beispiele:
- `MachineReadRepository.cs`
- `MachineComponentWriteRepository.cs`
- `AccessRuleReadRepository.cs`

## 5.4 Services
Format:
`<Meaning>Service.cs`

Beispiele:
- `VisibilitySubjectResolver.cs`
- `AuditService.cs`

---

# 6. WEB NAMING

## 6.1 Pages
Format:
- `<Things>.razor`
- `<Thing>Detail.razor`
- `Edit<Thing>.razor`
- `Create<Thing>.razor`

Beispiele:
- `Machines.razor`
- `MachineDetail.razor`
- `EditMaintenanceOrder.razor`
- `EnergyMeters.razor`

## 6.2 Komponenten
Format:
- `<Thing>Section.razor`
- `<Thing>Form.razor`
- `<Thing>Tree.razor`
- `<Thing>Card.razor`
- `<Thing>Dialog.razor`

Beispiele:
- `MachineComponentsSection.razor`
- `MachineComponentForm.razor`
- `EnergyMeterHistoryCard.razor`

## 6.3 CSS-Dateien
Format:
gleicher Name wie Razor-Datei

Beispiele:
- `MachineDetail.razor.css`
- `EnergyMeters.razor.css`

---

# 7. UI / THEME NAMING

## 7.1 Theme-Dateien
- `theme.css` = global
- `variables.css` = optional
- `.razor.css` = lokal

## 7.2 CSS-Klassen
Format:
- fachlich
- lesbar
- komponentennah

Beispiele:
- `.machine-detail`
- `.machine-detail__header`
- `.energy-meter-card`
- `.maintenance-order-form`

Empfohlen:
- BEM-nahe Struktur

---

# 8. TEST NAMING

## 8.1 Testdateien
Format:
`<Thing>Tests.cs`

Beispiele:
- `MachineComponentTests.cs`
- `VisibilityResolverTests.cs`
- `EnergyMeterTests.cs`

## 8.2 Testmethoden
Format:
`<Method>_<Condition>_<ExpectedResult>`

Beispiele:
- `Move_WhenParentIsSameMachine_ShouldSucceed`
- `Create_WhenNameIsEmpty_ShouldFail`

---

# 9. CODE PATTERNS

## 9.1 Dateigröße
Richtwerte:
- Domain: max. 300 Zeilen
- Application: max. 250 Zeilen
- Razor: max. 300–400 Zeilen

## 9.2 Bündelung
Zusammenfassen erlaubt:
- kleine DTOs
- kleine Interfaces
- kleine Enums mit engem Bezug

Getrennt halten:
- Entities
- große UseCases
- Resolver
- zentrale Domainregeln

## 9.3 God Components verboten
Eine Razor-Datei darf nicht gleichzeitig sein:
- Page
- Dialoghost
- Businesslogik
- Formular
- Tree
- Sichtbarkeitspanel
- mehrere Workflows

## 9.4 UI-Logik
Erlaubt in Razor:
- Anzeige
- Binding
- einfache Zustände
- UseCase-Aufruf

Verboten:
- Fachlogik
- Sicherheitslogik
- Rechenlogik
- Bestandslogik
- Wartungsregeln
- Sichtbarkeitsentscheidungen

---

# 10. BENENNUNGSREGELN FÜR SECURITY

## M103 Authentication
- `CurrentUserContext`
- `LocalAuthProvider`
- `SsoAuthProvider`
- `ClaimsPrincipalFactory`

## M800 Security
- `AccessRule`
- `AccessTargetType`
- `AccessSubjectType`
- `AccessRuleMode`
- `VisibilityResolver`
- `AuditEntry`
- `SecretReference`
- `SecurityPolicy`

Keine Vermischung von:
- Auth
- Authorization
- Visibility
- Audit

---

# 11. BENENNUNGSREGELN FÜR ENERGY

## Domain
- `EnergyMeter`
- `EnergyReading`
- `EnergyMediumType`
- `CaptureMode`
- `MeterReadingSource`

## Application
- `CreateEnergyMeter.cs`
- `CreateEnergyReading.cs`
- `GetEnergyReadings.cs`

## Optional später
- `EnergyTelemetry`
- `EnergyAlarm`
- `EnergyKpi`

---

# 12. BENENNUNGSREGELN FÜR MAINTENANCE

## Domain
- `MaintenanceOrder`
- `MaintenancePlan`
- `MaintenanceOrderStatus`
- `MaintenanceOrderType`

## Application
- `CreateMaintenanceOrder.cs`
- `UpdateMaintenanceOrder.cs`
- `GetMaintenanceOrders.cs`
- `SetMaintenanceOrderStatus.cs`

---

# 13. BENENNUNGSREGELN FÜR RUNTIME

## Scheduler / Automation
- `SchedulerJob`
- `AutomationRule`
- `JobExecutionService`

## Leitwarte / Control
- `ControlCommand`
- `ControlSession`
- `ControlPermission`
- `RealTimeState`

---

# 14. BENENNUNGSREGELN FÜR INTELLIGENCE

- `PredictionModel`
- `OptimizationResult`
- `AnomalyEvent`
- `ForecastInput`
- `IntelligenceFinding`

---

# 15. BENENNUNGSREGELN FÜR PLATFORM

- `PluginDefinition`
- `ExtensionState`
- `SystemVersion`
- `UpdatePackage`
- `MarketplaceEntry`

---

# 16. VERBOTENE BENENNUNGEN

❌ `Helper.cs`  
❌ `Manager.cs` (wenn unklar)  
❌ `Processor.cs` (wenn unklar)  
❌ `Util.cs`  
❌ `Common.cs`  
❌ `Misc.cs`  
❌ `Stuff.cs`  
❌ `DataService.cs` (wenn fachlich unklar)  
❌ `Temp.cs`  
❌ `NewFile.cs`  

---

# 17. GRUNDSATZ

> Ein Name muss sofort zeigen:
> - in welchem Modul etwas liegt
> - welche Verantwortung es hat
> - ob es Domain, UseCase, Repository oder UI ist

Wenn der Name das nicht zeigt:
→ falsch benannt

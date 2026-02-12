# Variant Coding Provider Architecture

## Class Diagram

```
┌─────────────────────────────────────┐
│  IVariantCodingDataProvider         │
│  (Interface)                        │
├─────────────────────────────────────┤
│ + CanReadVariantCoding: bool        │
│ + CanWriteVariantCoding: bool       │
│ + ReadVariantCoding(service): byte[]│
│ + WriteVariantCoding(req, svc): void│
└─────────────────────────────────────┘
                   ▲
                   │ implements
                   │
         ┌─────────┴──────────┐
         │                    │
┌────────┴──────────┐  ┌──────┴─────────────────┐
│ECUConnectionData  │  │MockVariantCodingData   │
│Provider           │  │Provider                │
├───────────────────┤  ├────────────────────────┤
│- _connection      │  │- _currentVariantCoding │
│                   │  │- _canRead              │
│                   │  │- _canWrite             │
└───────────────────┘  └────────────────────────┘
         │                        │
         │                        │
         └────────────────────────┘
                   │
                   │ used by
                   │
         ┌─────────▼──────────┐
         │      VCForm        │
         ├────────────────────┤
         │+ DataProvider      │
         │+ ReadService       │
         │+ WriteService      │
         │+ VCValue           │
         └────────────────────┘
                   │
                   │ calls
                   │
         ┌─────────▼──────────┐
         │  VariantCoding     │
         │  (static class)    │
         ├────────────────────┤
         │+ DoVariantCoding() │
         └────────────────────┘
```

## Sequence Diagram - Production Flow

```
MainForm          ECUConnection     ECUConnectionDataProvider     VCForm           VariantCoding
   │                    │                      │                     │                    │
   │─── new ────────────┤                      │                     │                    │
   │                    │                      │                     │                    │
   │─── new ────────────┼──────────────────────┤                     │                    │
   │                    │                      │                     │                    │
   │─── new ────────────┼──────────────────────┼─────────────────────┤                    │
   │                    │                      │                     │                    │
   │                    │                      │◄──── CanRead? ──────┤                    │
   │                    │                      │                     │                    │
   │                    │                      │──── Check state ────┤                    │
   │                    │◄─────────────────────┤                     │                    │
   │                    │                      │                     │                    │
   │                    │                      │◄── ReadVC(service) ─┤                    │
   │                    │◄─────────────────────┤                     │                    │
   │                    │─── SendDiagRequest ──►                     │                    │
   │                    │◄─── response ────────┤                     │                    │
   │                    │──────────────────────►                     │                    │
   │                    │                      │──── VC data ────────►                    │
   │                    │                      │                     │                    │
   │──── User clicks Apply ────────────────────┼─────────────────────┤                    │
   │                    │                      │                     │                    │
   │──── DoVariantCoding ───────────────────────────────────────────────────────────────►│
   │                    │                      │                     │                    │
   │                    │                      │◄───── WriteVC ──────┼────────────────────┤
   │                    │◄─────────────────────┤                     │                    │
   │                    │── ExecUserDiagJob ───►                     │                    │
   │                    │                      │                     │                    │
```

## Sequence Diagram - Test Flow

```
TestCode          MockVariantCodingDataProvider     VCForm           VariantCoding
   │                          │                         │                    │
   │─── new (test data) ──────┤                         │                    │
   │                          │                         │                    │
   │─── new (mock provider) ──┼─────────────────────────┤                    │
   │                          │                         │                    │
   │                          │◄──── CanRead? ──────────┤                    │
   │                          │──── returns true ───────►                    │
   │                          │                         │                    │
   │                          │◄── ReadVC(service) ─────┤                    │
   │                          │─── mock response ───────►                    │
   │                          │                         │                    │
   │                          │                         │─── displays VC ────►User
   │                          │                         │                    │
   │──── simulate Apply ──────┼─────────────────────────┤                    │
   │                          │                         │                    │
   │──── DoVariantCoding ─────────────────────────────────────────────────────►
   │                          │                         │                    │
   │                          │◄───── WriteVC ──────────┼────────────────────┤
   │                          │─── stores in memory ────►                    │
   │                          │                         │                    │
   │─── verify written data ──┤                         │                    │
   │◄── CurrentVariantCoding ─┤                         │                    │
   │                          │                         │                    │
```

## Key Benefits Illustrated

### Before Refactoring
```
VCForm ──[direct dependency]──► ECUConnection ──► Physical Device
   │                                                      │
   └──────────────────────────────────────────────────────┘
                    Cannot test without device
```

### After Refactoring
```
                    ┌─────────────────────────────────────┐
                    │   IVariantCodingDataProvider        │
                    │         (Interface)                 │
                    └─────────────────────────────────────┘
                                    ▲
                                    │
                    ┌───────────────┴────────────────┐
                    │                                │
         ┌──────────┴───────────┐       ┌───────────┴──────────┐
         │ ECUConnectionData    │       │ MockVariantCoding    │
         │ Provider             │       │ DataProvider         │
         └──────────┬───────────┘       └──────────────────────┘
                    │                              │
         ┌──────────▼───────────┐                  │
         │   Physical Device    │                  │
         └──────────────────────┘                  │
                                                    │
                                         ┌──────────▼───────────┐
                                         │  In-Memory Test Data │
                                         └──────────────────────┘

VCForm works with both providers!
```

## Component Responsibilities

### IVariantCodingDataProvider (Interface)
- **Role**: Abstraction layer for data access
- **Responsibility**: Define contract for reading/writing VC data
- **Benefits**: Enables different implementations

### ECUConnectionDataProvider (Production)
- **Role**: Real hardware communication
- **Responsibility**: Delegate to ECUConnection
- **Benefits**: No change to production behavior

### MockVariantCodingDataProvider (Testing)
- **Role**: Simulated data source
- **Responsibility**: Provide test data without hardware
- **Benefits**: Enables isolated testing

### VCForm (Consumer)
- **Role**: User interface
- **Responsibility**: Display and edit VC data
- **Benefits**: No longer coupled to ECUConnection

### VariantCoding (Business Logic)
- **Role**: VC processing logic
- **Responsibility**: Prepare and validate VC writes
- **Benefits**: Testable independently

## Data Flow

### Read Operation
```
User Action → VCForm → IVariantCodingDataProvider → Read Implementation
                │                                            │
                │                                            ▼
                │                                    ┌───────────────┐
                │                                    │  Production:  │
                │                                    │  ECUConnection│
                │                                    │  └─► Device   │
                │                                    │               │
                │                                    │  Test:        │
                │                                    │  Memory Array │
                │                                    └───────────────┘
                │                                            │
                ◄────────────── VC Data ────────────────────┘
```

### Write Operation
```
User Action → VCForm → VariantCoding → IVariantCodingDataProvider → Write Implementation
                                                                             │
                                                                             ▼
                                                                     ┌───────────────┐
                                                                     │  Production:  │
                                                                     │  ECUConnection│
                                                                     │  └─► Device   │
                                                                     │               │
                                                                     │  Test:        │
                                                                     │  Memory Array │
                                                                     └───────────────┘
```

## Testing Strategy

### Unit Tests (with Mock Provider)
```
Test Setup → Create Mock Provider → Create VCForm → Verify Behavior
                                                            │
                                                            └─► No hardware needed
```

### Integration Tests (with ECU Provider)
```
Test Setup → Create ECU Provider → Create VCForm → Verify with Simulated Device
                                                            │
                                                            └─► Uses SimulatedDevice class
```

### Manual Tests (flexible)
```
Developer → Choose Provider → Create VCForm → Test scenarios
                │                                    │
                ├─► Mock: Fast iteration            │
                └─► ECU: Real hardware validation   │
```

# Variant Coding Data Provider - Testing Guide

## Overview

The variant coding pane has been refactored to use Dependency Injection (DI) through the `IVariantCodingDataProvider` interface. This allows testing variant coding functionality without requiring a physical ECU connection.

## Architecture

### Interface: `IVariantCodingDataProvider`

The main abstraction that defines how variant coding data is read and written:

```csharp
public interface IVariantCodingDataProvider
{
    bool CanReadVariantCoding { get; }
    bool CanWriteVariantCoding { get; }
    byte[] ReadVariantCoding(DiagService readService);
    void WriteVariantCoding(byte[] request, DiagService writeService);
}
```

### Implementations

#### 1. `ECUConnectionDataProvider` (Production)

Wraps the existing `ECUConnection` to communicate with physical devices:

```csharp
var connection = new ECUConnection(...);
var provider = new ECUConnectionDataProvider(connection);
```

#### 2. `MockVariantCodingDataProvider` (Testing)

Simulates variant coding operations without requiring hardware:

```csharp
// Create a mock provider with initial VC data
byte[] initialVC = new byte[] { 0x01, 0x02, 0x03, 0x04 };
var mockProvider = new MockVariantCodingDataProvider(initialVC);

// Or create one that can't write (simulation mode)
var readOnlyProvider = new MockVariantCodingDataProvider(
    initialVC, 
    canRead: true, 
    canWrite: false
);
```

#### 3. `RecordingVariantCodingDataProvider` (Recording)

Wraps another provider to record all interactions for later replay:

```csharp
// Wrap any provider with recording
var ecuProvider = new ECUConnectionDataProvider(connection);
var recordingProvider = new RecordingVariantCodingDataProvider(ecuProvider);

// Use normally, then save the recording
recordingProvider.SaveRecording("session.txt");
```

#### 4. `ReplayVariantCodingDataProvider` (Replay)

Replays previously recorded sessions without requiring hardware:

```csharp
// Load a recording and replay it
var replayProvider = new ReplayVariantCodingDataProvider("session.txt");
// Use like any other provider - responses come from the recording
```

See `RecordingReplayGuide.md` for detailed documentation on recording and replay providers.

## Usage Examples

### Example 1: Testing VCForm with Mock Data

```csharp
// Setup
CaesarContainer container = LoadContainer("path/to/cbf");
byte[] testVCData = new byte[] { 0x12, 0x34, 0x56, 0x78 };
var mockProvider = new MockVariantCodingDataProvider(testVCData);

// Create VCForm with mock provider
var vcForm = new VCForm(
    container, 
    "ECU_NAME", 
    "VARIANT_NAME", 
    "VC_DOMAIN_NAME", 
    mockProvider
);

// The form will now load with the test data without needing a physical device
vcForm.ShowDialog();
```

### Example 2: Testing Variant Coding Write Operations

```csharp
// Create mock provider
var mockProvider = new MockVariantCodingDataProvider(
    initialData: new byte[] { 0x00, 0x00, 0x00, 0x00 },
    canRead: true,
    canWrite: true
);

// Create and show VCForm
var vcForm = new VCForm(container, ecuName, variantName, vcDomain, mockProvider);
if (vcForm.ShowDialog() == DialogResult.OK)
{
    // Execute the write operation
    VariantCoding.DoVariantCoding(vcForm, writesEnabled: true);
    
    // Verify the written data
    byte[] writtenData = mockProvider.CurrentVariantCoding;
    Console.WriteLine($"Written VC: {BitUtility.BytesToHex(writtenData, true)}");
}
```

### Example 3: Testing Read-Only Simulation Mode

```csharp
// Create a provider that can read but not write
var readOnlyProvider = new MockVariantCodingDataProvider(
    initialData: testData,
    canRead: true,
    canWrite: false
);

var vcForm = new VCForm(container, ecuName, variantName, vcDomain, readOnlyProvider);

// The Apply button will be disabled since canWrite is false
// This simulates the behavior when connection is not established
```

### Example 4: Production Usage (No Changes Required)

The existing production code automatically uses the ECU connection:

```csharp
// In MainForm.cs - this is already updated
IVariantCodingDataProvider dataProvider = new ECUConnectionDataProvider(Connection);
VCForm vcForm = new VCForm(container, ecuName, variantName, domainName, dataProvider);
```

## Benefits

1. **Testability**: Can now test variant coding logic without physical hardware
2. **Isolation**: Can test different scenarios (read-only, write failures, etc.)
3. **Development**: Developers can work on UI and logic without ECU
4. **CI/CD**: Automated tests can verify variant coding functionality
5. **Minimal Changes**: Existing production code flow remains unchanged

## Migration Notes

- The `VCForm` constructor now accepts `IVariantCodingDataProvider` instead of `ECUConnection`
- The `VariantCoding.DoVariantCoding` method no longer requires the `ECUConnection` parameter
- All variant coding read/write operations go through the provider interface

## Future Enhancements

Potential improvements:
- Add validation logic to mock provider
- Simulate various error conditions
- Record and replay real ECU interactions
- Add unit test framework integration

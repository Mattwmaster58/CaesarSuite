# Recording and Replay Providers Guide

## Overview

The Recording and Replay providers enable you to capture real ECU interactions and replay them later for testing and development without requiring physical hardware.

## Key Concepts

### Recording Provider (`RecordingVariantCodingDataProvider`)

A **wrapper provider** that:
- Wraps any other provider (typically `ECUConnectionDataProvider`)
- Records all requests and responses to memory
- Can save recordings to a file for later replay
- Supports multiple responses for the same request (sequential recording)

### Replay Provider (`ReplayVariantCodingDataProvider`)

A **playback provider** that:
- Loads recordings from a file
- Replays responses for matching requests
- Loops through sequential responses in order
- Enables testing without hardware

## Use Cases

### 1. Capture Real ECU Sessions
Record actual interactions with a physical ECU for later analysis or testing.

### 2. Offline Development
Develop and test variant coding features without requiring ECU hardware.

### 3. Automated Testing
Create repeatable test scenarios from real ECU data.

### 4. Training & Demonstration
Use recordings to demonstrate features without risking real hardware.

### 5. Debugging
Capture problematic sessions for detailed analysis.

## Recording Workflow

### Step 1: Wrap Your Provider

```csharp
// Create your normal provider (e.g., ECU connection)
var ecuProvider = new ECUConnectionDataProvider(connection);

// Wrap it with recording provider
var recordingProvider = new RecordingVariantCodingDataProvider(ecuProvider);
```

### Step 2: Use Normally

```csharp
// Use the recording provider exactly like any other provider
var vcForm = new VCForm(container, ecuName, variantName, vcDomain, recordingProvider);
vcForm.ShowDialog();

// All interactions are being recorded transparently
if (vcForm.DialogResult == DialogResult.OK)
{
    VariantCoding.DoVariantCoding(vcForm, writesEnabled: true);
}
```

### Step 3: Save the Recording

```csharp
// Save to file when done
recordingProvider.SaveRecording("my_session.txt");
```

## Replay Workflow

### Step 1: Load Recording

```csharp
// Create replay provider from file
var replayProvider = new ReplayVariantCodingDataProvider("my_session.txt");
```

### Step 2: Use as Normal Provider

```csharp
// Use exactly like any other provider
var vcForm = new VCForm(container, ecuName, variantName, vcDomain, replayProvider);
vcForm.ShowDialog();

// The form will receive the same responses as in the original recording
```

## Recording File Format

The recording file is a human-readable text format:

```
# Variant Coding Recording
# Created: 2026-02-12 10:30:00
# Total Read Requests: 2
# Total Write Requests: 1

## READ RECORDINGS
REQUEST:2201
RESPONSE:620101020304
RESPONSE:620105060708

REQUEST:2202
RESPONSE:6202AABBCCDD

## WRITE RECORDINGS
WRITE:2E0101020304
COUNT:1
```

### Format Details

- **REQUEST:** - Hex representation of the request bytes
- **RESPONSE:** - Hex representation of the response bytes
- Multiple RESPONSE lines for the same REQUEST represent sequential responses
- **WRITE:** - Hex representation of write request bytes
- **COUNT:** - Number of times this write was called

## Sequential Response Looping

When multiple responses are recorded for the same request, the replay provider loops through them:

```csharp
// Recording captured 3 responses for same request
REQUEST:2201
RESPONSE:6201010203  // First read returns this
RESPONSE:6201040506  // Second read returns this
RESPONSE:6201070809  // Third read returns this

// Replay behavior:
// 1st call: returns 6201010203
// 2nd call: returns 6201040506
// 3rd call: returns 6201070809
// 4th call: returns 6201010203 (loops back)
// 5th call: returns 6201040506
// ...and so on
```

### Reset Replay Indices

```csharp
var replayProvider = new ReplayVariantCodingDataProvider("session.txt");

// Read several times
replayProvider.ReadVariantCoding(service); // Returns 1st response
replayProvider.ReadVariantCoding(service); // Returns 2nd response

// Start over from beginning
replayProvider.ResetIndices();

replayProvider.ReadVariantCoding(service); // Returns 1st response again
```

## Advanced Features

### Inspect Recording Stats

```csharp
var recordingProvider = new RecordingVariantCodingDataProvider(innerProvider);

// ... perform operations ...

// Get recorded data
var responses = recordingProvider.GetRecordedResponses();
var writeCounts = recordingProvider.GetWriteCallCounts();

Console.WriteLine($"Recorded {responses.Count} request patterns");
foreach (var kvp in responses)
{
    Console.WriteLine($"Request {kvp.Key}: {kvp.Value.Count} responses");
}
```

### Check Replay Coverage

```csharp
var replayProvider = new ReplayVariantCodingDataProvider("session.txt");

// Check if a specific request has recorded responses
int count = replayProvider.GetRecordedResponseCount(requestBytes);
if (count > 0)
{
    Console.WriteLine($"This request has {count} recorded responses");
}
else
{
    Console.WriteLine("This request is not in the recording");
}
```

### Layered Recording

You can layer recording providers for advanced scenarios:

```csharp
// Record a mock session
var mockProvider = new MockVariantCodingDataProvider(testData);
var recording1 = new RecordingVariantCodingDataProvider(mockProvider);

// Use it...
recording1.ReadVariantCoding(service);

// Then wrap the replay in another recording
var replay1 = new ReplayVariantCodingDataProvider("session1.txt");
var recording2 = new RecordingVariantCodingDataProvider(replay1);

// This allows you to modify/filter recordings
```

## Common Patterns

### Pattern 1: Development Workflow

```csharp
// ONE-TIME: Record from real ECU
var ecuProvider = new ECUConnectionDataProvider(connection);
var recording = new RecordingVariantCodingDataProvider(ecuProvider);
// ... use VCForm ...
recording.SaveRecording("dev_session.txt");

// DAILY: Develop using replay
var replay = new ReplayVariantCodingDataProvider("dev_session.txt");
// ... use VCForm without ECU ...
```

### Pattern 2: Automated Testing

```csharp
// Test setup
var replayProvider = new ReplayVariantCodingDataProvider("test_case_1.txt");
var vcForm = new VCForm(container, ecu, variant, domain, replayProvider);

// Run test
vcForm.Show();
// ... assert expected behavior ...
vcForm.Close();
```

### Pattern 3: Regression Testing

```csharp
// Capture baseline behavior
var baseline = new RecordingVariantCodingDataProvider(actualProvider);
// ... perform operations ...
baseline.SaveRecording("baseline.txt");

// Later: Compare new behavior
var newRecording = new RecordingVariantCodingDataProvider(actualProvider);
// ... perform same operations ...
newRecording.SaveRecording("new.txt");

// Compare files to detect regressions
```

## Safety Features

### Write Protection in Replay

By default, replay providers have writing disabled:

```csharp
// Safe: Cannot write
var replay = new ReplayVariantCodingDataProvider("session.txt");

// Throws exception if you try to write
replay.WriteVariantCoding(data, service); // ❌ InvalidOperationException

// Enable writing if needed (for testing only!)
var replay = new ReplayVariantCodingDataProvider("session.txt", canWrite: true);
replay.WriteVariantCoding(data, service); // ✅ Allowed (but doesn't actually write)
```

### Error Handling

```csharp
var replay = new ReplayVariantCodingDataProvider("session.txt");

try
{
    byte[] response = replay.ReadVariantCoding(service);
}
catch (InvalidOperationException ex)
{
    // Request not found in recording
    Console.WriteLine($"Request not in recording: {ex.Message}");
}
```

## Tips & Best Practices

### 1. Descriptive File Names
Use descriptive names for recording files:
- ✅ `ecu_ezs_default_vc_read.txt`
- ✅ `w204_variant_change_session.txt`
- ❌ `recording1.txt`

### 2. Version Control
Consider storing important recordings in version control for team sharing.

### 3. Documentation
Add comments in recording files (lines starting with #) to document context:
```
# Recording of W204 EZS variant coding change
# Date: 2026-02-12
# Performed on: Bench ECU serial 12345
# Purpose: Capture baseline for automated tests
```

### 4. Separate Recording Sessions
Create separate recordings for different test scenarios rather than one large recording.

### 5. Validate Recordings
After creating a recording, test it immediately with a replay to ensure it captured correctly.

### 6. Keep Recordings Small
Only record what you need - large recordings take longer to load and are harder to debug.

## Troubleshooting

### Issue: "No recorded response for request"

**Cause:** The replay provider doesn't have a recording for the request being made.

**Solution:** 
1. Check that you're using the correct recording file
2. Verify the request bytes match exactly
3. Re-record the session if needed

### Issue: Responses seem out of order

**Cause:** Sequential responses are looping as designed.

**Solution:**
- Use `ResetIndices()` to start from the beginning
- Check that your test is calling in the expected order

### Issue: Recording file is empty

**Cause:** Forgot to call `SaveRecording()` or no operations were performed.

**Solution:**
- Ensure operations were actually performed before saving
- Call `SaveRecording()` before the provider goes out of scope

## Comparison with Mock Provider

| Feature | Mock Provider | Recording/Replay |
|---------|--------------|------------------|
| Data Source | Hardcoded test data | Real ECU sessions |
| Setup Effort | Manual | Automatic capture |
| Realism | Low | High (actual ECU data) |
| Flexibility | High | Medium |
| Best For | Unit tests | Integration tests |

## See Also

- `VariantCodingProviderGuide.md` - Overview of all providers
- `Examples/RecordingReplayExample.cs` - Working code examples
- `QuickStartGuide.md` - General provider usage

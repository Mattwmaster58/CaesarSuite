# Variant Coding Test Examples

This directory contains example code demonstrating how to use the variant coding provider pattern for testing without requiring a physical ECU connection.

## Overview

- **VariantCodingTestExample.cs** - 6 examples showing mock provider usage
- **RecordingReplayExample.cs** - 8 examples showing recording and replay functionality

## VariantCodingTestExample.cs

### Example 1: Basic Mock Provider
Shows how to create a basic mock provider with initial data.

### Example 2: Simulate Read
Demonstrates simulating a variant coding read operation.

### Example 3: Simulate Write
Shows how to simulate a variant coding write operation and verify the result.

### Example 4: Read-Only Mode
Demonstrates creating a read-only provider (simulates no ECU connection).

### Example 5: Using Mock Provider with VCForm
Shows how to create and use VCForm with a mock provider.

### Example 6: Comparing Providers
Compares the production provider with the test provider.

## RecordingReplayExample.cs

### Example 1: Recording from ECU
Shows how to wrap an ECU connection with recording to capture real interactions.

### Example 2: Recording from Mock
Demonstrates recording from a mock provider for testing purposes.

### Example 3: Replaying Recordings
Shows how to replay previously recorded sessions.

### Example 4: Sequential Response Replay
Demonstrates how replay loops through multiple responses for the same request.

### Example 5: Resetting Replay Indices
Shows how to reset replay to start from the beginning.

### Example 6: Complete Workflow
Complete end-to-end example: record from ECU, then replay for testing.

### Example 7: Inspecting Recording Statistics
Shows how to inspect recording file contents and statistics.

### Example 8: Debug Recording
Demonstrates accessing recorded data without saving to file.

## How to Use

These examples are meant to be used as reference when:
1. Writing unit tests for variant coding functionality
2. Developing UI without requiring physical hardware
3. Testing edge cases and error conditions
4. Understanding the provider pattern implementation
5. Capturing real ECU sessions for offline development
6. Creating repeatable test scenarios from real data

## Integration

To use these examples in your code:

```csharp
using Diogenes.Examples;

// Mock provider examples
VariantCodingTestExample.Example1_BasicMockProvider();

// Recording/Replay examples
RecordingReplayExample.Example1_RecordFromECU(connection, container, ecu, variant, domain);
```

## Note

These examples are not automatically executed tests. They are reference implementations showing how to use the providers. To create actual unit tests, integrate with a testing framework like NUnit or xUnit.

## See Also

- `docs/VariantCodingProviderGuide.md` - Comprehensive guide to all providers
- `docs/RecordingReplayGuide.md` - Detailed recording/replay documentation
- `docs/REFACTORING_SUMMARY.md` - Summary of the refactoring changes

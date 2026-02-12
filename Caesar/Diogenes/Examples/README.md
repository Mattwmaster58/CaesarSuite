# Variant Coding Test Examples

This directory contains example code demonstrating how to use the variant coding provider pattern for testing without requiring a physical ECU connection.

## Overview

The `VariantCodingTestExample.cs` file contains 6 examples showing different use cases for the mock provider.

## Examples Included

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

## How to Use

These examples are meant to be used as reference when:
1. Writing unit tests for variant coding functionality
2. Developing UI without requiring physical hardware
3. Testing edge cases and error conditions
4. Understanding the provider pattern implementation

## Integration

To use these examples in your code:

```csharp
using Diogenes.Examples;

// Call any example method
VariantCodingTestExample.Example1_BasicMockProvider();
```

## Note

These examples are not automatically executed tests. They are reference implementations showing how to use the mock provider. To create actual unit tests, integrate with a testing framework like NUnit or xUnit.

## See Also

- `docs/VariantCodingProviderGuide.md` - Comprehensive guide
- `docs/REFACTORING_SUMMARY.md` - Summary of the refactoring changes

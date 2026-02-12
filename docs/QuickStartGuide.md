# Quick Start Guide - Testing Variant Coding Without Hardware

This guide shows you how to quickly start testing variant coding functionality without requiring a physical ECU connection.

## Prerequisites

- CaesarSuite codebase with the DI refactoring applied
- A CBF file loaded into a `CaesarContainer`
- Knowledge of the ECU name, variant name, and VC domain you want to test

## Quick Start: 5 Minutes to Testing

### Step 1: Load Your CBF Container

```csharp
// Load your CBF file
byte[] cbfData = File.ReadAllBytes("path/to/your.cbf");
CaesarContainer container = new CaesarContainer(cbfData);
```

### Step 2: Create Mock Provider with Test Data

```csharp
// Option A: Use default/empty data
var mockProvider = new MockVariantCodingDataProvider();

// Option B: Use specific test data
byte[] testVCData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
var mockProvider = new MockVariantCodingDataProvider(testVCData);

// Option C: Use realistic data from CBF defaults
ECUVariant variant = container.GetECUVariantByName("YOUR_VARIANT_NAME");
VCDomain vcDomain = variant.GetVCDomainByName("YOUR_VC_DOMAIN");
byte[] defaultVC = vcDomain.DefaultData.FirstOrDefault(x => x.Item1.ToLower() == "default")?.Item2;
var mockProvider = new MockVariantCodingDataProvider(defaultVC);
```

### Step 3: Create and Show VCForm

```csharp
var vcForm = new VCForm(
    container, 
    "YOUR_ECU_NAME",
    "YOUR_VARIANT_NAME", 
    "YOUR_VC_DOMAIN_NAME", 
    mockProvider
);

// Show the form
vcForm.ShowDialog();
```

### Step 4: Test Write Operations (Optional)

```csharp
// If you want to test the write functionality
if (vcForm.ShowDialog() == DialogResult.OK)
{
    // This will use the mock provider to "write" the data
    VariantCoding.DoVariantCoding(vcForm, writesEnabled: true);
    
    // Verify what was written
    byte[] writtenData = mockProvider.CurrentVariantCoding;
    Console.WriteLine($"Written: {BitUtility.BytesToHex(writtenData, true)}");
}
```

## Complete Example

Here's a complete working example:

```csharp
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Caesar;
using Diogenes;

public class TestVariantCoding
{
    public static void TestWithoutHardware()
    {
        // 1. Load CBF
        byte[] cbfData = File.ReadAllBytes(@"C:\path\to\your.cbf");
        CaesarContainer container = new CaesarContainer(cbfData);
        
        // 2. Define what you want to test
        string ecuName = "EZS_219";
        string variantName = "EZS_219_05";
        string vcDomainName = "VC_DOMAIN_EZSMM";
        
        // 3. Get default VC data from CBF
        ECUVariant variant = container.GetECUVariantByName(variantName);
        VCDomain vcDomain = variant.GetVCDomainByName(vcDomainName);
        byte[] defaultVC = vcDomain.DefaultData
            .FirstOrDefault(x => x.Item1.ToLower() == "default")?.Item2
            ?? new byte[vcDomain.DumpSize];
        
        // 4. Create mock provider with the default data
        var mockProvider = new MockVariantCodingDataProvider(
            defaultVC,
            canRead: true,
            canWrite: true
        );
        
        // 5. Create and show the form
        var vcForm = new VCForm(
            container, 
            ecuName, 
            variantName, 
            vcDomainName, 
            mockProvider
        );
        
        // 6. Show dialog and handle result
        if (vcForm.ShowDialog() == DialogResult.OK)
        {
            Console.WriteLine("User accepted changes");
            
            // Perform the write (to mock provider)
            VariantCoding.DoVariantCoding(vcForm, writesEnabled: true);
            
            // Verify the result
            byte[] finalVC = mockProvider.CurrentVariantCoding;
            Console.WriteLine($"Final VC: {BitUtility.BytesToHex(finalVC, true)}");
        }
        else
        {
            Console.WriteLine("User cancelled");
        }
    }
}
```

## Common Scenarios

### Scenario 1: Test UI Without Any Connection

```csharp
// No hardware, no connection needed
var mockProvider = new MockVariantCodingDataProvider();
var vcForm = new VCForm(container, ecuName, variantName, vcDomain, mockProvider);
vcForm.ShowDialog();
```

### Scenario 2: Test Read-Only Mode

```csharp
// Simulates what happens when ECU is not connected
var readOnlyProvider = new MockVariantCodingDataProvider(
    testData,
    canRead: true,
    canWrite: false  // Apply button will be disabled
);
var vcForm = new VCForm(container, ecuName, variantName, vcDomain, readOnlyProvider);
vcForm.ShowDialog();
```

### Scenario 3: Verify VC Changes

```csharp
byte[] originalVC = new byte[] { 0x00, 0x00, 0x00, 0x00 };
var mockProvider = new MockVariantCodingDataProvider(originalVC);

var vcForm = new VCForm(container, ecuName, variantName, vcDomain, mockProvider);
if (vcForm.ShowDialog() == DialogResult.OK)
{
    VariantCoding.DoVariantCoding(vcForm, writesEnabled: true);
    
    byte[] newVC = mockProvider.CurrentVariantCoding;
    
    // Compare
    if (!originalVC.SequenceEqual(newVC))
    {
        Console.WriteLine("VC was modified!");
        Console.WriteLine($"Old: {BitUtility.BytesToHex(originalVC, true)}");
        Console.WriteLine($"New: {BitUtility.BytesToHex(newVC, true)}");
    }
}
```

### Scenario 4: Automated Testing Loop

```csharp
// Test multiple VC configurations
var testConfigurations = new[]
{
    new byte[] { 0x00, 0x00, 0x00, 0x00 },
    new byte[] { 0xFF, 0xFF, 0xFF, 0xFF },
    new byte[] { 0x01, 0x02, 0x03, 0x04 }
};

foreach (var testConfig in testConfigurations)
{
    var mockProvider = new MockVariantCodingDataProvider(testConfig);
    var vcForm = new VCForm(container, ecuName, variantName, vcDomain, mockProvider);
    
    // Test that form loads without errors
    try
    {
        vcForm.Show();
        vcForm.Close();
        Console.WriteLine($"Config {BitUtility.BytesToHex(testConfig, true)} loaded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Config {BitUtility.BytesToHex(testConfig, true)} failed: {ex.Message}");
    }
}
```

## Comparing with Production Code

### Production Code (MainForm.cs)
```csharp
// Uses real ECU connection
IVariantCodingDataProvider dataProvider = new ECUConnectionDataProvider(Connection);
VCForm vcForm = new VCForm(container, ecuName, variantName, domainName, dataProvider);
```

### Test Code
```csharp
// Uses mock provider
IVariantCodingDataProvider dataProvider = new MockVariantCodingDataProvider(testData);
VCForm vcForm = new VCForm(container, ecuName, variantName, domainName, dataProvider);
```

**Note**: The only difference is which provider implementation you use!

## Troubleshooting

### Issue: VCForm closes immediately
**Solution**: Check that the ReadService and WriteService are found in the variant. Add null checks:
```csharp
ECUVariant variant = container.GetECUVariantByName(variantName);
if (variant == null)
{
    Console.WriteLine($"Variant '{variantName}' not found");
    return;
}

VCDomain vcDomain = variant.GetVCDomainByName(vcDomainName);
if (vcDomain == null)
{
    Console.WriteLine($"VC Domain '{vcDomainName}' not found");
    return;
}
```

### Issue: Apply button is disabled
**Solution**: Check the mock provider's `canWrite` parameter:
```csharp
// Make sure canWrite is true
var mockProvider = new MockVariantCodingDataProvider(
    testData,
    canRead: true,
    canWrite: true  // Must be true for Apply button to be enabled
);
```

### Issue: Can't see what was written
**Solution**: Use the `CurrentVariantCoding` property:
```csharp
VariantCoding.DoVariantCoding(vcForm, writesEnabled: true);
byte[] result = mockProvider.CurrentVariantCoding;
Console.WriteLine($"Written: {BitUtility.BytesToHex(result, true)}");
```

## Next Steps

1. **Add Unit Tests**: Create a test project and add unit tests using the mock provider
2. **Test Edge Cases**: Use the mock provider to test various error conditions
3. **Document Test Cases**: Keep track of which configurations you've tested
4. **Integrate with CI/CD**: Add automated tests to your build pipeline

## Further Reading

- `docs/VariantCodingProviderGuide.md` - Comprehensive guide
- `docs/REFACTORING_SUMMARY.md` - Details of the changes made
- `docs/ArchitectureDiagram.md` - Visual architecture documentation
- `Caesar/Diogenes/Examples/VariantCodingTestExample.cs` - More examples

## Summary

With the DI refactoring, you can now:

✅ Test variant coding UI without hardware  
✅ Develop new features faster  
✅ Verify VC changes programmatically  
✅ Simulate various scenarios  
✅ Write automated tests  

The key is using `MockVariantCodingDataProvider` instead of `ECUConnectionDataProvider`!

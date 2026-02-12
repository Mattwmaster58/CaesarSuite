using System;
using Caesar;
using Diogenes;

namespace Diogenes.Examples
{
    /// <summary>
    /// Example demonstrating how to use MockVariantCodingDataProvider for testing
    /// variant coding functionality without requiring a physical ECU connection.
    /// </summary>
    public class VariantCodingTestExample
    {
        /// <summary>
        /// Example 1: Basic mock provider usage
        /// </summary>
        public static void Example1_BasicMockProvider()
        {
            Console.WriteLine("=== Example 1: Basic Mock Provider ===");
            
            // Create initial variant coding data (example: 50 bytes)
            byte[] initialVC = new byte[50];
            for (int i = 0; i < initialVC.Length; i++)
            {
                initialVC[i] = (byte)(i % 256);
            }

            // Create mock provider
            var mockProvider = new MockVariantCodingDataProvider(initialVC);

            Console.WriteLine($"Can Read: {mockProvider.CanReadVariantCoding}");
            Console.WriteLine($"Can Write: {mockProvider.CanWriteVariantCoding}");
            Console.WriteLine($"Initial VC Data: {BitUtility.BytesToHex(initialVC, true)}");
        }

        /// <summary>
        /// Example 2: Simulating variant coding read
        /// </summary>
        public static void Example2_SimulateRead(CaesarContainer container, string variantName, string vcDomainName)
        {
            Console.WriteLine("=== Example 2: Simulate Read ===");

            // Get the variant and VC domain
            ECUVariant variant = container.GetECUVariantByName(variantName);
            VCDomain vcDomain = variant.GetVCDomainByName(vcDomainName);
            DiagService readService = variant.GetDiagServiceByName(vcDomain.ReadServiceName);

            // Create test data
            byte[] testVCData = new byte[vcDomain.DumpSize];
            for (int i = 0; i < testVCData.Length; i++)
            {
                testVCData[i] = 0xAA; // Fill with test pattern
            }

            // Create mock provider with test data
            var mockProvider = new MockVariantCodingDataProvider(testVCData);

            // Simulate reading
            byte[] readData = mockProvider.ReadVariantCoding(readService);
            Console.WriteLine($"Read VC Data: {BitUtility.BytesToHex(readData, true)}");
        }

        /// <summary>
        /// Example 3: Simulating variant coding write
        /// </summary>
        public static void Example3_SimulateWrite(CaesarContainer container, string variantName, string vcDomainName)
        {
            Console.WriteLine("=== Example 3: Simulate Write ===");

            // Get the variant and VC domain
            ECUVariant variant = container.GetECUVariantByName(variantName);
            VCDomain vcDomain = variant.GetVCDomainByName(vcDomainName);
            DiagService writeService = variant.GetDiagServiceByName(vcDomain.WriteServiceName);

            // Create mock provider
            var mockProvider = new MockVariantCodingDataProvider(new byte[vcDomain.DumpSize]);

            // Prepare write request
            byte[] writeRequest = new byte[vcDomain.DumpSize + 10];
            for (int i = 0; i < writeRequest.Length; i++)
            {
                writeRequest[i] = (byte)(i % 256);
            }

            Console.WriteLine($"Writing VC Data: {BitUtility.BytesToHex(writeRequest, true)}");
            
            // Simulate writing
            mockProvider.WriteVariantCoding(writeRequest, writeService);

            // Verify the write
            byte[] writtenData = mockProvider.CurrentVariantCoding;
            Console.WriteLine($"Data stored in mock: {BitUtility.BytesToHex(writtenData, true)}");
        }

        /// <summary>
        /// Example 4: Testing read-only mode (simulates no ECU connection)
        /// </summary>
        public static void Example4_ReadOnlyMode()
        {
            Console.WriteLine("=== Example 4: Read-Only Mode ===");

            byte[] defaultData = new byte[] { 0x00, 0x40, 0x33, 0x10 };
            
            // Create a read-only provider (simulates no connection for writes)
            var readOnlyProvider = new MockVariantCodingDataProvider(
                defaultData,
                canRead: true,
                canWrite: false
            );

            Console.WriteLine($"Can Read: {readOnlyProvider.CanReadVariantCoding}");
            Console.WriteLine($"Can Write: {readOnlyProvider.CanWriteVariantCoding}");
            Console.WriteLine("This mode simulates VCForm behavior when ECU is not connected");
            Console.WriteLine("The Apply button would be disabled in this mode");
        }

        /// <summary>
        /// Example 5: Using mock provider with VCForm
        /// </summary>
        public static void Example5_WithVCForm(CaesarContainer container, string ecuName, string variantName, string vcDomainName)
        {
            Console.WriteLine("=== Example 5: Using Mock Provider with VCForm ===");

            // Get the variant to determine VC data size
            ECUVariant variant = container.GetECUVariantByName(variantName);
            VCDomain vcDomain = variant.GetVCDomainByName(vcDomainName);

            // Create test VC data
            byte[] testVCData = new byte[vcDomain.DumpSize];
            // Initialize with some test pattern
            for (int i = 0; i < testVCData.Length; i++)
            {
                testVCData[i] = (byte)((i * 7) % 256);
            }

            // Create mock provider
            var mockProvider = new MockVariantCodingDataProvider(testVCData);

            // Create VCForm with mock provider
            // Note: This would normally be shown with ShowDialog()
            var vcForm = new VCForm(container, ecuName, variantName, vcDomainName, mockProvider);

            Console.WriteLine("VCForm created successfully with mock provider");
            Console.WriteLine("The form can now be tested without a physical ECU connection");
            Console.WriteLine($"Apply button enabled: {mockProvider.CanWriteVariantCoding}");
            
            // To actually show the form:
            // if (vcForm.ShowDialog() == DialogResult.OK)
            // {
            //     VariantCoding.DoVariantCoding(vcForm, writesEnabled: true);
            // }
        }

        /// <summary>
        /// Example 6: Comparing production vs test provider
        /// </summary>
        public static void Example6_ComparingProviders(ECUConnection connection)
        {
            Console.WriteLine("=== Example 6: Comparing Providers ===");

            // Production provider
            var productionProvider = new ECUConnectionDataProvider(connection);
            Console.WriteLine($"Production Provider - Can Read: {productionProvider.CanReadVariantCoding}");
            Console.WriteLine($"Production Provider - Can Write: {productionProvider.CanWriteVariantCoding}");

            // Test provider
            var testProvider = new MockVariantCodingDataProvider(new byte[50]);
            Console.WriteLine($"Test Provider - Can Read: {testProvider.CanReadVariantCoding}");
            Console.WriteLine($"Test Provider - Can Write: {testProvider.CanWriteVariantCoding}");

            Console.WriteLine("\nBoth implement IVariantCodingDataProvider interface");
            Console.WriteLine("Production code uses ECUConnectionDataProvider");
            Console.WriteLine("Test code can use MockVariantCodingDataProvider");
        }
    }
}

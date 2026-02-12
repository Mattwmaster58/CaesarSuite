using System;
using System.IO;
using Caesar;
using Diogenes;

namespace Diogenes.Examples
{
    /// <summary>
    /// Examples demonstrating how to use the Recording and Replay providers
    /// for capturing and replaying variant coding interactions.
    /// </summary>
    public class RecordingReplayExample
    {
        /// <summary>
        /// Example 1: Recording variant coding interactions from a real ECU connection
        /// </summary>
        public static void Example1_RecordFromECU(ECUConnection connection, CaesarContainer container, 
            string ecuName, string variantName, string vcDomainName)
        {
            Console.WriteLine("=== Example 1: Recording from ECU ===");

            // Create the production provider
            var ecuProvider = new ECUConnectionDataProvider(connection);

            // Wrap it with a recording provider
            var recordingProvider = new RecordingVariantCodingDataProvider(ecuProvider);

            // Use the recording provider with VCForm
            var vcForm = new VCForm(container, ecuName, variantName, vcDomainName, recordingProvider);

            // User interacts with the form
            if (vcForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // If user makes changes, they'll be recorded
                VariantCoding.DoVariantCoding(vcForm, writesEnabled: true);
            }

            // Save the recording to a file
            string recordingPath = Path.Combine(Path.GetTempPath(), "vc_recording.txt");
            recordingProvider.SaveRecording(recordingPath);

            Console.WriteLine($"Recording saved to: {recordingPath}");
            Console.WriteLine("This recording can now be used with ReplayVariantCodingDataProvider");
        }

        /// <summary>
        /// Example 2: Recording from a mock provider (for testing)
        /// </summary>
        public static void Example2_RecordFromMock(CaesarContainer container, 
            string ecuName, string variantName, string vcDomainName)
        {
            Console.WriteLine("=== Example 2: Recording from Mock ===");

            // Create test data
            byte[] testVCData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var mockProvider = new MockVariantCodingDataProvider(testVCData);

            // Wrap with recording provider
            var recordingProvider = new RecordingVariantCodingDataProvider(mockProvider);

            // Use with VCForm
            var vcForm = new VCForm(container, ecuName, variantName, vcDomainName, recordingProvider);
            vcForm.ShowDialog();

            // Save the recording
            string recordingPath = "test_vc_recording.txt";
            recordingProvider.SaveRecording(recordingPath);

            Console.WriteLine($"Test recording saved to: {recordingPath}");
        }

        /// <summary>
        /// Example 3: Replaying previously recorded interactions
        /// </summary>
        public static void Example3_ReplayRecording(CaesarContainer container, 
            string ecuName, string variantName, string vcDomainName, string recordingFilePath)
        {
            Console.WriteLine("=== Example 3: Replay Recording ===");

            // Create replay provider from recording file
            var replayProvider = new ReplayVariantCodingDataProvider(recordingFilePath, canWrite: false);

            // Use with VCForm - it will replay the recorded responses
            var vcForm = new VCForm(container, ecuName, variantName, vcDomainName, replayProvider);
            vcForm.ShowDialog();

            Console.WriteLine("Replayed variant coding session from recording");
        }

        /// <summary>
        /// Example 4: Sequential response replay
        /// Demonstrates how the replay provider loops through multiple responses for the same request
        /// </summary>
        public static void Example4_SequentialReplay(CaesarContainer container, 
            string variantName, string vcDomainName)
        {
            Console.WriteLine("=== Example 4: Sequential Response Replay ===");

            // First, create a recording with multiple reads
            ECUVariant variant = container.GetECUVariantByName(variantName);
            VCDomain vcDomain = variant.GetVCDomainByName(vcDomainName);
            DiagService readService = variant.GetDiagServiceByName(vcDomain.ReadServiceName);

            // Create mock with changing data
            var mockProvider = new MockVariantCodingDataProvider(new byte[] { 0x01, 0x02 });
            var recordingProvider = new RecordingVariantCodingDataProvider(mockProvider);

            // Simulate multiple reads that would return different data over time
            for (int i = 0; i < 3; i++)
            {
                // Update mock data
                mockProvider.SetVariantCoding(new byte[] { (byte)i, (byte)(i + 1) });
                byte[] response = recordingProvider.ReadVariantCoding(readService);
                Console.WriteLine($"Read {i + 1}: {BitUtility.BytesToHex(response, true)}");
            }

            // Save recording
            string recordingPath = "sequential_recording.txt";
            recordingProvider.SaveRecording(recordingPath);

            // Now replay - it will loop through the responses
            var replayProvider = new ReplayVariantCodingDataProvider(recordingPath);

            Console.WriteLine("\nReplaying (will loop through responses):");
            for (int i = 0; i < 5; i++) // Read more times than recorded
            {
                byte[] response = replayProvider.ReadVariantCoding(readService);
                Console.WriteLine($"Replay {i + 1}: {BitUtility.BytesToHex(response, true)}");
            }
        }

        /// <summary>
        /// Example 5: Resetting replay indices
        /// </summary>
        public static void Example5_ResetReplay(string recordingFilePath, DiagService readService)
        {
            Console.WriteLine("=== Example 5: Reset Replay Indices ===");

            var replayProvider = new ReplayVariantCodingDataProvider(recordingFilePath);

            // Read a few times
            Console.WriteLine("First set of reads:");
            for (int i = 0; i < 3; i++)
            {
                byte[] response = replayProvider.ReadVariantCoding(readService);
                Console.WriteLine($"  Read {i + 1}: {BitUtility.BytesToHex(response, true)}");
            }

            // Reset and start over
            replayProvider.ResetIndices();
            Console.WriteLine("\nAfter reset:");
            for (int i = 0; i < 3; i++)
            {
                byte[] response = replayProvider.ReadVariantCoding(readService);
                Console.WriteLine($"  Read {i + 1}: {BitUtility.BytesToHex(response, true)}");
            }
        }

        /// <summary>
        /// Example 6: Complete workflow - Record from real ECU, then replay for testing
        /// </summary>
        public static void Example6_CompleteWorkflow(ECUConnection connection, CaesarContainer container,
            string ecuName, string variantName, string vcDomainName)
        {
            Console.WriteLine("=== Example 6: Complete Workflow ===");

            string recordingPath = "production_vc_session.txt";

            // Step 1: Record a real session with ECU
            Console.WriteLine("\nStep 1: Recording from real ECU...");
            var ecuProvider = new ECUConnectionDataProvider(connection);
            var recordingProvider = new RecordingVariantCodingDataProvider(ecuProvider);

            var vcFormRecord = new VCForm(container, ecuName, variantName, vcDomainName, recordingProvider);
            if (vcFormRecord.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                VariantCoding.DoVariantCoding(vcFormRecord, writesEnabled: true);
            }

            recordingProvider.SaveRecording(recordingPath);
            Console.WriteLine($"Session recorded to: {recordingPath}");

            // Step 2: Now test/develop without ECU using replay
            Console.WriteLine("\nStep 2: Replaying for testing (no ECU needed)...");
            var replayProvider = new ReplayVariantCodingDataProvider(recordingPath);

            var vcFormReplay = new VCForm(container, ecuName, variantName, vcDomainName, replayProvider);
            vcFormReplay.ShowDialog();

            Console.WriteLine("Replay completed - same behavior as real ECU!");
        }

        /// <summary>
        /// Example 7: Inspecting recording statistics
        /// </summary>
        public static void Example7_InspectRecording(string recordingFilePath)
        {
            Console.WriteLine("=== Example 7: Inspect Recording ===");

            // Load and inspect the recording
            var replayProvider = new ReplayVariantCodingDataProvider(recordingFilePath);

            // The provider logs the recording statistics when loading
            // You can also check specific requests if needed

            Console.WriteLine("Recording loaded and ready for replay");
            Console.WriteLine("Check console output above for detailed statistics");
        }

        /// <summary>
        /// Example 8: Using recording provider without saving (for debugging)
        /// </summary>
        public static void Example8_DebugRecording(IVariantCodingDataProvider innerProvider, DiagService readService)
        {
            Console.WriteLine("=== Example 8: Debug Recording ===");

            var recordingProvider = new RecordingVariantCodingDataProvider(innerProvider);

            // Perform some operations
            byte[] response = recordingProvider.ReadVariantCoding(readService);

            // Access recorded data directly (without saving to file)
            var recordedResponses = recordingProvider.GetRecordedResponses();
            var writeCounts = recordingProvider.GetWriteCallCounts();

            Console.WriteLine($"Recorded {recordedResponses.Count} unique request patterns");
            Console.WriteLine($"Tracked {writeCounts.Count} write request patterns");

            foreach (var kvp in recordedResponses)
            {
                Console.WriteLine($"  Request: {kvp.Key}");
                Console.WriteLine($"    Responses: {kvp.Value.Count}");
            }
        }
    }
}

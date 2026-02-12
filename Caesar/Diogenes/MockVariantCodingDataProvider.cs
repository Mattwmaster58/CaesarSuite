using Caesar;
using System;
using System.Linq;

namespace Diogenes
{
    /// <summary>
    /// Mock implementation of IVariantCodingDataProvider for testing purposes.
    /// This provider simulates variant coding operations without requiring a physical device.
    /// </summary>
    public class MockVariantCodingDataProvider : IVariantCodingDataProvider
    {
        private byte[] _currentVariantCoding;
        private readonly bool _canRead;
        private readonly bool _canWrite;

        /// <summary>
        /// Initializes a new instance of MockVariantCodingDataProvider.
        /// </summary>
        /// <param name="initialData">Initial variant coding data. If null, a default empty array is used.</param>
        /// <param name="canRead">Whether the provider should allow reading.</param>
        /// <param name="canWrite">Whether the provider should allow writing.</param>
        public MockVariantCodingDataProvider(byte[] initialData = null, bool canRead = true, bool canWrite = true)
        {
            _currentVariantCoding = initialData ?? new byte[0];
            _canRead = canRead;
            _canWrite = canWrite;
        }

        public bool CanReadVariantCoding => _canRead;

        public bool CanWriteVariantCoding => _canWrite;

        /// <summary>
        /// Gets the currently stored variant coding data (for testing/verification).
        /// </summary>
        public byte[] CurrentVariantCoding => _currentVariantCoding?.ToArray();

        public byte[] ReadVariantCoding(DiagService readService)
        {
            if (!CanReadVariantCoding)
            {
                throw new InvalidOperationException("Mock provider configured to not allow reading.");
            }

            if (readService == null)
            {
                throw new ArgumentNullException(nameof(readService));
            }

            // Simulate a realistic response: prepend response code and copy the stored data
            // In real scenarios, the response would include headers and the VC data at a specific offset
            // For now, we'll return a simple response that mimics the structure
            byte[] response = new byte[_currentVariantCoding.Length + 3];
            response[0] = 0x62; // Positive response for read (0x22 + 0x40)
            response[1] = 0x01; // Data identifier high byte
            response[2] = 0x10; // Data identifier low byte
            Array.Copy(_currentVariantCoding, 0, response, 3, _currentVariantCoding.Length);

            return response;
        }

        public void WriteVariantCoding(byte[] request, DiagService writeService)
        {
            if (!CanWriteVariantCoding)
            {
                throw new InvalidOperationException("Mock provider configured to not allow writing.");
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (writeService == null)
            {
                throw new ArgumentNullException(nameof(writeService));
            }

            // Extract the variant coding data from the request
            // In a real write request, the VC data would be at a specific offset
            // For simplicity, we'll store the entire request as the new VC data
            _currentVariantCoding = request.ToArray();

            Console.WriteLine($"Mock: Variant coding written: {BitUtility.BytesToHex(_currentVariantCoding, true)}");
        }

        /// <summary>
        /// Updates the stored variant coding data (for testing purposes).
        /// </summary>
        /// <param name="data">The new variant coding data to store.</param>
        public void SetVariantCoding(byte[] data)
        {
            _currentVariantCoding = data?.ToArray() ?? new byte[0];
        }
    }
}

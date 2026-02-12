using Caesar;

namespace Diogenes
{
    /// <summary>
    /// Interface for providing variant coding data, abstracting the source of the data.
    /// This allows testing without requiring a physical ECU connection.
    /// </summary>
    public interface IVariantCodingDataProvider
    {
        /// <summary>
        /// Indicates whether the provider can read variant coding data from the source.
        /// </summary>
        bool CanReadVariantCoding { get; }

        /// <summary>
        /// Indicates whether the provider can write variant coding data to the source.
        /// </summary>
        bool CanWriteVariantCoding { get; }

        /// <summary>
        /// Reads variant coding data using the specified diagnostic service.
        /// </summary>
        /// <param name="readService">The diagnostic service to use for reading.</param>
        /// <returns>The variant coding data read from the source.</returns>
        byte[] ReadVariantCoding(DiagService readService);

        /// <summary>
        /// Writes variant coding data using the specified diagnostic service.
        /// </summary>
        /// <param name="request">The request bytes to send.</param>
        /// <param name="writeService">The diagnostic service to use for writing.</param>
        void WriteVariantCoding(byte[] request, DiagService writeService);
    }
}

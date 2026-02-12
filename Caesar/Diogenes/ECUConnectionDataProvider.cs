using Caesar;
using System;

namespace Diogenes
{
    /// <summary>
    /// Concrete implementation of IVariantCodingDataProvider that uses an ECUConnection
    /// to communicate with a physical device.
    /// </summary>
    public class ECUConnectionDataProvider : IVariantCodingDataProvider
    {
        private readonly ECUConnection _connection;

        public ECUConnectionDataProvider(ECUConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public bool CanReadVariantCoding
        {
            get
            {
                return _connection.State >= ECUConnection.ConnectionState.ChannelConnectedPendingEcuContact;
            }
        }

        public bool CanWriteVariantCoding
        {
            get
            {
                return _connection.State >= ECUConnection.ConnectionState.ChannelConnectedPendingEcuContact;
            }
        }

        public byte[] ReadVariantCoding(DiagService readService)
        {
            if (!CanReadVariantCoding)
            {
                throw new InvalidOperationException("Cannot read variant coding: ECU connection not established.");
            }

            return _connection.SendDiagRequest(readService);
        }

        public void WriteVariantCoding(byte[] request, DiagService writeService)
        {
            if (!CanWriteVariantCoding)
            {
                throw new InvalidOperationException("Cannot write variant coding: ECU connection not established.");
            }

            _connection.ExecUserDiagJob(request, writeService);
        }
    }
}

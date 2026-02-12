using Caesar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Diogenes
{
    /// <summary>
    /// Recording implementation of IVariantCodingDataProvider that wraps another provider
    /// and records all requests and responses for later replay.
    /// </summary>
    public class RecordingVariantCodingDataProvider : IVariantCodingDataProvider
    {
        private readonly IVariantCodingDataProvider _innerProvider;
        private readonly Dictionary<string, List<byte[]>> _recordedResponses;
        private readonly Dictionary<string, int> _writeCallCounts;

        public RecordingVariantCodingDataProvider(IVariantCodingDataProvider innerProvider)
        {
            _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
            _recordedResponses = new Dictionary<string, List<byte[]>>();
            _writeCallCounts = new Dictionary<string, int>();
        }

        public bool CanReadVariantCoding => _innerProvider.CanReadVariantCoding;

        public bool CanWriteVariantCoding => _innerProvider.CanWriteVariantCoding;

        public byte[] ReadVariantCoding(DiagService readService)
        {
            // Call the inner provider
            byte[] response = _innerProvider.ReadVariantCoding(readService);

            // Record the request and response
            string requestKey = GetRequestKey(readService.RequestBytes);
            if (!_recordedResponses.ContainsKey(requestKey))
            {
                _recordedResponses[requestKey] = new List<byte[]>();
            }
            _recordedResponses[requestKey].Add(response);

            Console.WriteLine($"Recording: Read request {requestKey} -> {BitUtility.BytesToHex(response, true)}");

            return response;
        }

        public void WriteVariantCoding(byte[] request, DiagService writeService)
        {
            // Call the inner provider
            _innerProvider.WriteVariantCoding(request, writeService);

            // Record the write call
            string requestKey = GetRequestKey(request);
            if (!_writeCallCounts.ContainsKey(requestKey))
            {
                _writeCallCounts[requestKey] = 0;
            }
            _writeCallCounts[requestKey]++;

            Console.WriteLine($"Recording: Write request {requestKey} (count: {_writeCallCounts[requestKey]})");
        }

        /// <summary>
        /// Saves the recorded requests and responses to a file.
        /// </summary>
        /// <param name="filePath">Path to the file where recordings will be saved.</param>
        public void SaveRecording(string filePath)
        {
            StringBuilder sb = new StringBuilder();

            // Write header
            sb.AppendLine("# Variant Coding Recording");
            sb.AppendLine($"# Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# Total Read Requests: {_recordedResponses.Count}");
            sb.AppendLine($"# Total Write Requests: {_writeCallCounts.Count}");
            sb.AppendLine();

            // Write read recordings
            sb.AppendLine("## READ RECORDINGS");
            foreach (var kvp in _recordedResponses)
            {
                sb.AppendLine($"REQUEST:{kvp.Key}");
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    sb.AppendLine($"RESPONSE:{BitUtility.BytesToHex(kvp.Value[i], true)}");
                }
                sb.AppendLine();
            }

            // Write write recordings (just counts for now)
            sb.AppendLine("## WRITE RECORDINGS");
            foreach (var kvp in _writeCallCounts)
            {
                sb.AppendLine($"WRITE:{kvp.Key}");
                sb.AppendLine($"COUNT:{kvp.Value}");
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString());
            Console.WriteLine($"Recording saved to: {filePath}");
        }

        /// <summary>
        /// Gets the recorded responses for debugging/testing purposes.
        /// </summary>
        public Dictionary<string, List<byte[]>> GetRecordedResponses()
        {
            return new Dictionary<string, List<byte[]>>(_recordedResponses);
        }

        /// <summary>
        /// Gets the write call counts for debugging/testing purposes.
        /// </summary>
        public Dictionary<string, int> GetWriteCallCounts()
        {
            return new Dictionary<string, int>(_writeCallCounts);
        }

        /// <summary>
        /// Creates a unique key for a request based on its byte content.
        /// </summary>
        private string GetRequestKey(byte[] request)
        {
            return BitUtility.BytesToHex(request, true);
        }
    }
}

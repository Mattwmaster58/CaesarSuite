using Caesar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Diogenes
{
    /// <summary>
    /// Replay implementation of IVariantCodingDataProvider that replays previously recorded
    /// requests and responses from a file. For each request, it loops through recorded responses
    /// in sequential order.
    /// </summary>
    public class ReplayVariantCodingDataProvider : IVariantCodingDataProvider
    {
        private readonly Dictionary<string, List<byte[]>> _recordedResponses;
        private readonly Dictionary<string, int> _responseIndices;
        private readonly bool _canWrite;

        /// <summary>
        /// Initializes a new instance of ReplayVariantCodingDataProvider from a recording file.
        /// </summary>
        /// <param name="filePath">Path to the recording file.</param>
        /// <param name="canWrite">Whether to allow write operations (default: false for safety).</param>
        public ReplayVariantCodingDataProvider(string filePath, bool canWrite = false)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Recording file not found: {filePath}");
            }

            _recordedResponses = new Dictionary<string, List<byte[]>>();
            _responseIndices = new Dictionary<string, int>();
            _canWrite = canWrite;

            LoadRecording(filePath);
        }

        public bool CanReadVariantCoding => _recordedResponses.Count > 0;

        public bool CanWriteVariantCoding => _canWrite;

        public byte[] ReadVariantCoding(DiagService readService)
        {
            if (!CanReadVariantCoding)
            {
                throw new InvalidOperationException("Replay provider has no recorded responses.");
            }

            string requestKey = GetRequestKey(readService.RequestBytes);

            if (!_recordedResponses.ContainsKey(requestKey))
            {
                throw new InvalidOperationException($"No recorded response for request: {requestKey}");
            }

            // Get the list of responses for this request
            List<byte[]> responses = _recordedResponses[requestKey];

            // Initialize index if not present
            if (!_responseIndices.ContainsKey(requestKey))
            {
                _responseIndices[requestKey] = 0;
            }

            // Get current index and loop if necessary
            int index = _responseIndices[requestKey];
            byte[] response = responses[index % responses.Count];

            // Increment index for next call
            _responseIndices[requestKey] = (index + 1) % responses.Count;

            Console.WriteLine($"Replay: Request {requestKey} -> Response[{index % responses.Count}]: {BitUtility.BytesToHex(response, true)}");

            return response;
        }

        public void WriteVariantCoding(byte[] request, DiagService writeService)
        {
            if (!CanWriteVariantCoding)
            {
                throw new InvalidOperationException("Replay provider configured to not allow writing.");
            }

            string requestKey = GetRequestKey(request);
            Console.WriteLine($"Replay: Write request {requestKey} (simulated, no actual write)");
        }

        /// <summary>
        /// Gets the number of recorded responses for a given request.
        /// </summary>
        /// <param name="request">The request bytes.</param>
        /// <returns>Number of recorded responses, or 0 if no recordings exist for this request.</returns>
        public int GetRecordedResponseCount(byte[] request)
        {
            string requestKey = GetRequestKey(request);
            return _recordedResponses.ContainsKey(requestKey) ? _recordedResponses[requestKey].Count : 0;
        }

        /// <summary>
        /// Resets the replay indices for all requests, starting from the beginning.
        /// </summary>
        public void ResetIndices()
        {
            _responseIndices.Clear();
            Console.WriteLine("Replay: All indices reset");
        }

        /// <summary>
        /// Loads the recording from a file.
        /// </summary>
        private void LoadRecording(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            string currentRequest = null;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                // Skip empty lines and comments
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                {
                    continue;
                }

                // Skip section headers
                if (trimmedLine.StartsWith("##"))
                {
                    continue;
                }

                if (trimmedLine.StartsWith("REQUEST:"))
                {
                    currentRequest = trimmedLine.Substring(8);
                    if (!_recordedResponses.ContainsKey(currentRequest))
                    {
                        _recordedResponses[currentRequest] = new List<byte[]>();
                    }
                }
                else if (trimmedLine.StartsWith("RESPONSE:") && currentRequest != null)
                {
                    string responseHex = trimmedLine.Substring(9);
                    byte[] responseBytes = BitUtility.BytesFromHex(responseHex);
                    _recordedResponses[currentRequest].Add(responseBytes);
                }
                else if (trimmedLine.StartsWith("WRITE:"))
                {
                    // Write recordings are acknowledged but not used for replay
                    // (they're just for record keeping)
                    continue;
                }
                else if (trimmedLine.StartsWith("COUNT:"))
                {
                    // Write counts are acknowledged but not used for replay
                    continue;
                }
            }

            Console.WriteLine($"Replay: Loaded {_recordedResponses.Count} recorded request patterns from {filePath}");
            foreach (var kvp in _recordedResponses)
            {
                Console.WriteLine($"  Request {kvp.Key}: {kvp.Value.Count} response(s)");
            }
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

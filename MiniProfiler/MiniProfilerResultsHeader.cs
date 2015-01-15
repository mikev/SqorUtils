using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Profiling;

namespace Sqor.Utils.MiniProfiler
{
    public class MiniProfilerResultsHeader
    {
        public const string HeaderName = "MiniProfilerResults";

        /// <summary>
        /// The header namespace used to serialize the data
        /// </summary>
        public const string HeaderNamespace = "StackExchange.Profiling.Wcf";

        /// <summary>
        /// Gets or sets the profiler results.
        /// </summary>
        public StackExchange.Profiling.MiniProfiler ProfilerResults { get; set; }

        /// <summary>
        /// Convert the supplied compressed and encoded string into a profile header.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The <c>deserialized</c> instance.</returns>
        public static MiniProfilerResultsHeader FromHeaderText(string text)
        {
            byte[] buffer = Convert.FromBase64String(text);
            buffer = Decompress(buffer);
            return Read(buffer);
        }

        /// <summary>
        /// Convert the header to a text string suitable for placement in an HTTP header.
        /// </summary>
        /// <returns>The compressed and encoded string of serialized data</returns>
        public string ToHeaderText()
        {
            byte[] buffer = Write(this);
            buffer = Compress(buffer);
            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// Read a mini profiler results header from the supplied buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The results header.</returns>
        private static MiniProfilerResultsHeader Read(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();
                return  JsonConvert.DeserializeObject<MiniProfilerResultsHeader>(text);
            }
        }

        /// <summary>
        /// Write the header to a byte stream.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns>The serialized data.</returns>
        private static byte[] Write(MiniProfilerResultsHeader header)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.Unicode))
                {
                    var serializeObject = JsonConvert.SerializeObject(header);
                    writer.Write(serializeObject);
                    writer.Flush();
                    byte[] buffer = stream.GetBuffer();
                    Array.Resize(ref buffer, (int)stream.Length);
                    return buffer;
                }
            }            
        }

        /// <summary>
        /// Compress the supplied array of bytes into a smaller packet of data.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The compressed data.</returns>
        private static byte[] Compress(byte[] buffer)
        {
            using (var outStream = new MemoryStream())
            {
                using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                using (var stream = new MemoryStream(buffer))
                {
                    stream.CopyTo(tinyStream);
                }
                
                return outStream.ToArray();
            }            
        }

        /// <summary>
        /// decompress the supplied buffer into a smaller packet of data.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The uncompressed data.</returns>
        private static byte[] Decompress(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            using (var zipStream = new GZipStream(stream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                zipStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }         
    }
}
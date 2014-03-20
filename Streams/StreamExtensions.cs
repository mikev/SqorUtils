using System.IO;
using System.Threading.Tasks;

namespace Sqor.Utils.Streams
{
    public static class StreamExtensions
    {
        public static int Read(this Stream stream, byte[] buffer)
        {
            return stream.Read(buffer, 0, buffer.Length);
        }

        public static byte[] ReadBytesToEnd(this Stream stream)
        {
            using (stream)
            {
                var buffer = new byte[1024 * 10];
                var output = new MemoryStream();
                for (int amountRead = stream.Read(buffer); amountRead > 0; amountRead = stream.Read(buffer))
                {
                    output.Write(buffer, 0, amountRead);
                }
                return output.ToArray();
            }
        }        

        public static async Task<byte[]> ReadBytesToEndAsync(this Stream stream)
        {
            using (stream)
            {
                var buffer = new byte[1024 * 10];
                var output = new MemoryStream();
                for (int amountRead = await stream.ReadAsync(buffer, 0, buffer.Length); amountRead > 0; amountRead = await stream.ReadAsync(buffer, 0, buffer.Length))
                {
                    await output.WriteAsync(buffer, 0, amountRead);
                }
                return output.ToArray();
            }
        }        
    }
}

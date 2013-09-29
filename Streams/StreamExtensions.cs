using System.IO;

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
                byte[] buffer = new byte[1024 * 10];
                MemoryStream output = new MemoryStream();
                for (int amountRead = stream.Read(buffer); amountRead > 0; amountRead = stream.Read(buffer))
                {
                    output.Write(buffer, 0, amountRead);
                }
                return output.ToArray();
            }
        }        
         
    }
}
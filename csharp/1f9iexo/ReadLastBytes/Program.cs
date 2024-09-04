namespace ReadLastBytes
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var responsec = new byte[1000154];

            responsec[responsec.Length - 1] = 50;

            var response = new MemoryStream(responsec);

            // Defines the buffer size to 1Mb.
            // Increase the buffer size to read more fast the response,
            // but becareful that it will use temporary more memory
            // to read all the stream.
            const int BufferSize = 1024 * 1024;

            var buffer = new byte[BufferSize];
            byte? lastByte = null;

            while (true)
            {
                var count = await response.ReadAsync(buffer);

                if (count == 0)
                {
                    break;
                }

                lastByte = buffer[count - 1];
            }

            // Here, use the lastByte (if null, it is mean the response contains no byte).
            if (lastByte is null)
            {
                // The response was empty (no bytes)
            }
            else
            {
                // lastByte.Value contains the last byte.
            }
        }
    }
}

namespace Client.Utility
{
    public class FileHelper
    {
        public static System.IO.MemoryStream ReadFileByPart(System.IO.Stream stream)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            const int bufferSize = 2048;
            int bytesRead;
            var buffer = new byte[bufferSize];
            do
            {
                bytesRead = stream.Read(buffer, 0, bufferSize);

                if (bytesRead == 0) break;

                memoryStream.Write(buffer, 0, bytesRead);
               System.Threading.Thread.Sleep(25);
            } while (true);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            return memoryStream;
        }
    }
}

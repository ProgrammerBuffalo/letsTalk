using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace letsTalk
{
    // ИНТЕРФЕЙС РАБОТАЕТ НА БАЗЕ ПРОТОКОЛА HTTP

    // Интерфейс, который отвечает за работу файлов

    // MessageContract -> Сообщение структурируется в SOAP
    // MessageBodyMember -> Всё равно что DataMember

    // Для общения с методами данного интерфейса используется адрес http://localhost:8301/File
    [ServiceContract(Name = "File", Namespace = "letsTalk.IFileService")] 
    public interface IFileService
    {
        [OperationContract]
        DownloadFileInfo AvatarDownload(DownloadRequest request); // Отправка клиенту аватарки

        [OperationContract]
        [FaultContract(typeof(StreamExceptionFault))]
        void AvatarUpload(UploadFileInfo uploadRequest); // Отправка серверу от клиента аватарки
    }

    [MessageContract]
    public class DownloadRequest
    {
        [MessageBodyMember]
        public int Requested_UserSqlId;
    }

    [MessageContract]
    public class DownloadFileInfo : IDisposable
    {
        [MessageHeader(MustUnderstand = true)]
        public string FileExtension = "";

        [MessageHeader(MustUnderstand = true)]
        public long Length = 0;

        [MessageBodyMember(Order = 1)]
        public Stream FileStream = Stream.Null;

        public void Dispose()
        {
            if (FileStream != null)
            {
                FileStream.Close();
                FileStream = null;
            }
        }
    }

    [MessageContract]
    public class UploadFileInfo : IDisposable
    {
        [MessageHeader(MustUnderstand = true)]
        public int Responsed_UserSqlId;

        [MessageHeader(MustUnderstand = true)]
        public string FileExtension;

        [MessageBodyMember(Order = 1)]
        public Stream FileStream;

        public void Dispose()
        {
            if (FileStream != null)
            {
                FileStream.Close();
                FileStream = null;
            }
        }
    }
}

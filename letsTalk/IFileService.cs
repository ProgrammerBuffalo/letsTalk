using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace letsTalk
{
    [ServiceContract(Name = "File", Namespace = "letsTalk.IFileService")]
    public interface IFileService
    {
        [OperationContract]
        DownloadFileInfo AvatarDownload(DownloadRequest request);

        [OperationContract]
        void AvatarUpload(UploadFileInfo uploadRequest);
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
        public string FileExtension;

        [MessageHeader(MustUnderstand = true)]
        public long Length;

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

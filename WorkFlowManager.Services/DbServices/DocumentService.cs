using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Tables;

namespace WorkFlowManager.Services.DbServices
{
    public class DocumentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DocumentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public void Add(Document document)
        {
            Document tmpDocument;

            if (document.Id == 0) //Yeni ekleniyor
            {
                tmpDocument = new Document();

                tmpDocument.OwnerId = document.OwnerId;
                tmpDocument.Size = document.Size;
                tmpDocument.FileName = document.FileName;
                tmpDocument.MediaName = document.MediaName;
                tmpDocument.MimeType = document.MimeType;
                tmpDocument.Extension = document.Extension;
                tmpDocument.FileType = document.FileType;

                _unitOfWork.Repository<Document>().Add(tmpDocument);

                _unitOfWork.Complete();

            }


        }

        public void Remove(Document document)
        {
            if (document.Id != 0) //Yeni ekleniyor
            {
                _unitOfWork.Repository<Document>().Remove(document.Id);
                _unitOfWork.Complete();
            }
        }
    }
}

using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkFlowManager.Common.ViewModels
{
    public class FileUpload
    {
        public int OwnerId { get; private set; }
        public FileType FileType { get; private set; }

        public string ComponentName { get; private set; }
        public string TemplateId { get; private set; }

        public ICollection<Document> FileList { get; set; }

        public FileUpload(ICollection<Document> fileList, FileType fileType, int ownerId)
        {
            ComponentName = Guid.NewGuid().ToString();
            TemplateId = Guid.NewGuid().ToString();

            FileList = fileList != null ? fileList.Where(t => t.FileType == fileType).ToList() : null;
            FileType = fileType;
            OwnerId = ownerId;
        }

    }
}

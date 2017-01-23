using WorkFlowManager.Common.Enums;

namespace WorkFlowManager.Common.Tables
{
    public class Document : BaseTable
    {
        public int OwnerId { get; set; }
        public BaseTable Owner { get; set; }

        public FileType FileType { get; set; }
        public string FileName { get; set; }

        public string MediaName { get; set; }
        public string MimeType { get; set; }
        public string Extension { get; set; }
        public int Size { get; set; }

    }
}

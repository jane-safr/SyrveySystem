using System;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class UploadedFileDTO : BasePropertiesDTO
    {
        public Guid UploadedFileId { get; set; }

        public Guid ApplicationUserId { get; set; }

        public string PathFile { get; set; }

        public string FileName { get; set; }

        public int FileSize { get; set; }

        //Тип файла 1-докуиенты, 2 - инструкции
        public int FileType { get; set; }
    }
}

using System;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IUploadedFileService
    {
        Task<UploadedFileDTO> GetByIdAsync(Guid uploadedFileId);
        Task<OperationDetails> CreateAsync(UploadedFileDTO model);
        Task<OperationDetails> DeleteAsync(Guid uploadedFileId);
    }
}

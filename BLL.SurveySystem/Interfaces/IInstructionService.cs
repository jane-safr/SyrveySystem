using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IInstructionService
    {
        Task<IEnumerable<InstructionDTO>> GetAllAsync();
        Task<InstructionDTO> GetByIdAsync(Guid id);
        Task<InstructionDTO> GetByCodeAsync(int instrCode);
        Task<IEnumerable<InstructionDTO>> FindByFilterAsync(List<FilterModels> filterModels);
        Task<OperationDetails> CreateAsync(InstructionDTO model);
        Task<OperationDetails> UpdateAsync(InstructionDTO model);
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> UploadFileAsync(Guid instructionId, Guid uploadFileId, bool isRus);
        Task<OperationDetails> DeleteFileAsync(Guid documentId, bool isRus);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface ISurveyService
    {
        Task<OperationDetails> CreateAsync(SurveyDTO model);
        Task<OperationDetails> UpdateAsync(SurveyDTO model);
        Task<SurveyDTO> GetByIdAsync(Guid id);
        Task<SurveyDTO> GetByCodeAsync(int code);
        Task<OperationDetails> GenerateLinkAsync(int codeId);
        Task<IEnumerable<SurveyDTO>> FindByFilterAsync(List<FilterModels> filterModels);
        Task<IEnumerable<SurveyDTO>> GetAllAsync();
        Task<OperationDetails> DeleteAsync(Guid id);
    }
}
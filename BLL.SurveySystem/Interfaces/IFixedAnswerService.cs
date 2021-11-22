using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IFixedAnswerService
    {
        Task<OperationDetails> UpdateAsync(FixedAnswerDTO model);
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> CreateAsync(FixedAnswerDTO model);
        Task<FixedAnswerDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<FixedAnswerDTO>> GetAllAsync();
        Task<IEnumerable<FixedAnswerDTO>> FindByFilterAsync(List<FilterModels> filterModels);
    }
}
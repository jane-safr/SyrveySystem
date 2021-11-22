using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IAnswerService
    {
        Task<OperationDetails> UpdateAsync(AnswerDTO model);
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> CreateAsync(AnswerDTO model);
        Task<AnswerDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<AnswerDTO>> GetAllAsync();
        Task<IEnumerable<AnswerDTO>> FindByFilterAsync(List<FilterModels> filterModels);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IQuestionService
    {
        Task<OperationDetails> UpdateAsync(QuestionDTO model);
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> CreateAsync(QuestionDTO model);
        Task<QuestionDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<QuestionDTO>> GetAllAsync();
        Task<IEnumerable<QuestionDTO>> FindByFilterAsync(List<FilterModels> filterModels);
        Task<IEnumerable<QuestionDTO>> GetBySurveyCodeAsync(int code);
    }
}
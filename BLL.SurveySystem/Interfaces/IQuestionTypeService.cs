using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IQuestionTypeService
    {
        Task<OperationDetails> UpdateAsync(QuestionTypeDTO model);
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> CreateAsync(QuestionTypeDTO model);
        Task<QuestionTypeDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<QuestionTypeDTO>> GetAllAsync();
        Task<IEnumerable<QuestionTypeDTO>> FindByFilterAsync(List<FilterModels> filterModels);
    }
}
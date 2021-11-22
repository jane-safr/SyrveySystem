using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
   public interface ISurveyTypeService
    {
        Task<OperationDetails> UpdateAsync(SurveyTypeDTO model);
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> CreateAsync(SurveyTypeDTO model);
        Task<SurveyTypeDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<SurveyTypeDTO>> GetAllAsync();
        Task<IEnumerable<SurveyTypeDTO>> FindByFilterAsync(List<FilterModels> filterModels);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
  public interface ICriterionService
    {
        Task<OperationDetails> UpdateAsync(CriterionDTO model);
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> CreateAsync(CriterionDTO model);
        Task<CriterionDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<CriterionDTO>> GetAllAsync();
        Task<IEnumerable<CriterionDTO>> FindByFilterAsync(List<FilterModels> filterModels);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IIndicatorService
    {
        Task<OperationDetails> UpdateAsync(IndicatorDTO model);
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> CreateAsync(IndicatorDTO model);
        Task<IndicatorDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<IndicatorDTO>> GetAllAsync();
        Task<IEnumerable<IndicatorDTO>> FindByFilterAsync(List<FilterModels> filterModels);
    }
}
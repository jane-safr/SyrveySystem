using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IParameterService
    {
        Task<OperationDetails> UpdateAsync(ParameterDTO model);
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> CreateAsync(ParameterDTO model);
        Task<ParameterDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<ParameterDTO>> GetAllAsync();
        Task<IEnumerable<ParameterDTO>> FindByFilterAsync(List<FilterModels> filterModels);
    }
}
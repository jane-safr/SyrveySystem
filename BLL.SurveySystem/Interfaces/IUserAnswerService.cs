using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IUserAnswerService
    {
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> CreateAsync(UserAnswerDTO model);
        Task<OperationDetails> UpdateAsync(UserAnswerDTO model);
        Task<OperationDetails> UpdateFixedAsync(IEnumerable<UserAnswerDTO> model);
        Task<OperationDetails> CreateByInvitationAsync(Guid invitationId);
        Task<UserAnswerDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<UserAnswerDTO>> GetAllAsync();
        Task<IEnumerable<UserAnswerDTO>> FindByFilterAsync(List<FilterModels> filterModels);
    }
}

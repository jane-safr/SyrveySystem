using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IInvitationService
    {
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<OperationDetails> CreateAsync(InvitationDTO model);
        Task<OperationDetails> UpdateAsync(InvitationDTO model);
        Task<OperationDetails> CreateMultipeAsync(List<InvitationDTO> model);
        Task<InvitationDTO> GetByIdAsync(Guid id);
        Task<InvitationDTO> GetByCodeAsync(int codeId);
        Task<IEnumerable<InvitationDTO>> GetAllAsync();
        Task<IEnumerable<InvitationDTO>> FindByFilterAsync(List<FilterModels> filterModels);
        Task<IEnumerable<InvitationDTO>> GetBySurveyCodeAsync(int code);
        Task<OperationDetails> InvitationResendAsync(Guid invitationId, string username);
        Task<OperationDetails> CreateAnonAsync(int surveyCode);
    }
}
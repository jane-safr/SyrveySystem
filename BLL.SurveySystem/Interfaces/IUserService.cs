using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUserDTO> GetUserByEmail(string email);
        Task<ApplicationUserDTO> GetUserById(string id);
        Task<IEnumerable<ApplicationUserDTO>> GetUsers();
        Task<ApplicationUserDTO> GetDomainUsersByName(string name);
        Task<IEnumerable<ApplicationUserDTO>> GetDomainUsersByNameList(string userName);
        Task<ApplicationUserDTO> GetDomainUserById(Guid id);
        Task<OperationDetails> IsAuthenticated(string login, string pass);
        Task<OperationDetails> VerifyAndCreateIdentity(ApplicationUserDTO user, string password);
        Task<OperationDetails> UpdateUserAsync(ApplicationUserDTO model);
        Task<OperationDetails> CreateUserAsync(ApplicationUserDTO user, string password);
    }
}
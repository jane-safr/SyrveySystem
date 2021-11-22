using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
   public interface IRoleService
    {
        Task<ApplicationRoleDTO> GetRoleNameAsync(string name);
        Task<ApplicationRoleDTO> GetRoleByIdAsync(string id);
        Task<IEnumerable<ApplicationRoleDTO>> GetRolesAsync();
        Task<OperationDetails> AddUserToRoleAsync(string userId, string roleId);
        Task<List<string>> GetRoleByUserId(string userId);
        Task<OperationDetails> DeleteUserAllRolesAsync(string userId);
    }
}
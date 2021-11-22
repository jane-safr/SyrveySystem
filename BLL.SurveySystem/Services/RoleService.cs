using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;

namespace BLL.SurveySystem.Services
{
    public class RoleService : IRoleService
    {
        IUnitOfWork Database { get; set; }
        readonly ILoggerService<RoleService> loggingService;
        public RoleService(IUnitOfWork uow, ILoggerService<RoleService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }

        public async Task<OperationDetails> DeleteUserAllRolesAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    loggingService.Error("Delete Role User empty userId");
                    return new OperationDetails(false, "Отсутствует Id пользователя / UserId is empty", string.Empty);
                }
                var userDb = await Database.UserManager.FindByIdAsync(userId);
                if (userDb == null)
                {
                    loggingService.Error("User not found");
                    return new OperationDetails(false, "Пользователь не найден / User not found", string.Empty);
                }
                //get roleUserId
                var roles = (await Database.UserManager.GetRolesAsync(userDb.Id)).ToArray();
                if (roles.Any())
                {
                    await Database.UserManager.RemoveFromRolesAsync(userDb.Id, roles);
                }
                return new OperationDetails(true, "Роль удалена / Role Removed", string.Empty);
            }
            catch (Exception ex)
            {
                loggingService.Error($"{ex.Message}");
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", string.Empty);
            }
        }
        public async Task<List<string>> GetRoleByUserId(string userId)
        {
            try
            {
                var roles = new List<string>();
                if (string.IsNullOrEmpty(userId))
                {
                    loggingService.Error("getRole empty userId");
                    return roles;
                }
                var userDb = await Database.UserManager.FindByIdAsync(userId);
                if (userDb == null)
                {
                    loggingService.Error("getRole user not found");
                    return roles;
                }
                //get roleUserId
                roles = (await Database.UserManager.GetRolesAsync(userDb.Id)).ToList();
                return roles;
            }
            catch (Exception e)
            {
                loggingService.Error($"{e.Message}");
                return new List<string>();
            }
        }
        public async Task<IEnumerable<ApplicationRoleDTO>> GetRolesAsync()
        {
            try
            {
                var roles = await Database.RoleManager.Roles.Where(r => r.IsShown).OrderBy(r => r.Name).ToListAsync();
                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<ApplicationRole, ApplicationRoleDTO>().MaxDepth(1)
                        .ForMember(x => x.Users, x => x.Ignore());
                });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();
                var results = mapper.Map<IEnumerable<ApplicationRole>, List<ApplicationRoleDTO>>(roles);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }
        public async Task<ApplicationRoleDTO> GetRoleNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return null;
                var role = await Database.RoleManager.FindByNameAsync(name);
                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<ApplicationRole, ApplicationRoleDTO>().MaxDepth(1)
                        .ForMember(x => x.Users, x => x.Ignore());
                });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();
                var result = mapper.Map<ApplicationRole, ApplicationRoleDTO>(role);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }
        public async Task<ApplicationRoleDTO> GetRoleByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return null;
                var role = await Database.RoleManager.FindByIdAsync(id);
                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<ApplicationRole, ApplicationRoleDTO>().MaxDepth(1)
                        .ForMember(x => x.Users, x => x.Ignore());
                });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();
                var result = mapper.Map<ApplicationRole, ApplicationRoleDTO>(role);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }
        public async Task<OperationDetails> AddUserToRoleAsync(string userId, string roleId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleId))
                {
                    loggingService.Error("AddUserToRole userId or roleId Empty");
                    return new OperationDetails(false, "Отсутствует пользователь или роль / User or Role is empty", "AddUserToRole");
                }
                var user = await Database.UserManager.FindByIdAsync(userId.Trim());
                var role = await Database.RoleManager.FindByIdAsync(roleId.Trim());
                if (user != null && role != null)
                {
                    var existRole = await Database.UserManager.IsInRoleAsync(user.Id, role.Name);
                    if (existRole)
                    {
                        return new OperationDetails(false, "Роль существует у пользователя / Role already exists", "AddUserToRole");
                    }
                    var resultRole = await Database.UserManager.AddToRoleAsync(user.Id, role.Name);
                    if (resultRole.Succeeded)
                    {
                        return new OperationDetails(true, "Успешно обновлено / Completed", "AddUserToRole");
                    }
                    else
                    {
                        return new OperationDetails(false, "Не обновлено / Not Completed", "AddUserToRole");
                    }
                }
                loggingService.Error("AddUserToRole: user or role Empty");
                return new OperationDetails(false, "Ошибка добавления в роль / Add User To Role error", "AddUserToRole");
            }
            catch (Exception ex)
            {
                loggingService.Error($"AddUserToRole {ex.Message}");
                return new OperationDetails(false, $"Error: {ex.Message}", "AddUserToRole");
            }
        }

        public async Task<List<string>> GetUserRolesByEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    loggingService.Error("Empty userID");
                    return null;
                }

                var user = await Database.UserManager.FindByEmailAsync(email);
                if (string.IsNullOrEmpty(user?.Id))
                {
                    loggingService.Error("Empty user DB Get RolesUser");
                    return null;
                }
                return (await Database.UserManager.GetRolesAsync(user.Id)).ToList();
            }
            catch (Exception ex)
            {
                loggingService.Error("GetUserRoles " + ex.Message);
                return null;
            }
        }
        public async Task<OperationDetails> DeleteUserRoleAsync(string userId, string roleName)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(roleName))
                {
                    var user = await Database.UserManager.FindByIdAsync(userId);
                    var role = await Database.RoleManager.FindByNameAsync(roleName);
                    if (user != null && role != null)
                    {
                        var deleteRole = await Database.UserManager.RemoveFromRoleAsync(user.Id, roleName);
                        if (deleteRole.Succeeded)
                        {
                            return new OperationDetails(true, $"Роль для {userId} успешно удалена / Role deleted for {userId}", "DeleteUserRoleAsync");
                        }
                        else
                        {
                            return new OperationDetails(false, "Роль не удалена / Error delete role", "DeleteUserRoleAsync");
                        }
                    }
                }
                return new OperationDetails(false, "Роль не удалена / Error delete role", "DeleteUserRoleAsync");
            }
            catch (Exception ex)
            {
                loggingService.Error($"DeleteUserRoleAsync для {userId}" + ex.Message);
                return new OperationDetails(false, "DeleteUserRoleAsync " + ex.Message, "DeleteUserRoleAsync");
            }
        }
    }
}
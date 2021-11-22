using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Helpers;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;

namespace BLL.SurveySystem.Services
{
    public class UserService : IUserService
    {
        IUnitOfWork Database { get; set; }
        readonly ILoggerService<UserService> loggingService;
        public UserService(IUnitOfWork uow, ILoggerService<UserService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }

        private async Task<List<ApplicationUserDTO>> FindUsers(string searchString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchString))
                {
                    loggingService.Error("No parameters found user domain");
                    return new List<ApplicationUserDTO>();
                }
                var filterText = $"*{searchString.Trim()}*";
                var users = new BlockingCollection<ApplicationUserDTO>();
                var domainSettings = (await Database.Settings.GetAllAsync()).ToList();
                if (!domainSettings.Any())
                {
                    loggingService.Error("No Domain in Settings");
                    return new List<ApplicationUserDTO>();
                }
                var domain = domainSettings.FirstOrDefault(ds => ds.Name == "DomainT2");
                if (string.IsNullOrEmpty(domain?.Value))
                {
                    loggingService.Error("No Domain in Settings");
                    return new List<ApplicationUserDTO>();
                }
                var domainDc = domainSettings.FirstOrDefault(ds => ds.Name == "DomainDC");
                var domainUser = domainSettings.FirstOrDefault(ds => ds.Name == "UserConnectionDomain");
                var domainUserPass = domainSettings.FirstOrDefault(ds => ds.Name == "UserPassConnectionDomain");
                var pass = HelperBll.Base64StringDecode(domainUserPass?.Value.Trim());
                //d1
                using (var ad = new PrincipalContext(ContextType.Domain, domain.Value.Trim(), string.IsNullOrEmpty(domainDc?.Value) ? string.Empty : domainDc.Value,
                    string.IsNullOrEmpty(domainUser?.Value) ? null : domainUser.Value.Trim(), string.IsNullOrEmpty(pass) ? null : pass.Trim()))
                {
                    using (var searchMaskDisplayname = new UserPrincipal(ad) { Enabled = true, DisplayName = filterText })
                    using (var searchMaskUsername = new UserPrincipal(ad) { Enabled = true, SamAccountName = filterText })
                    using (var searcherDisplayname = new PrincipalSearcher(searchMaskDisplayname))
                    using (var searcherUsername = new PrincipalSearcher(searchMaskUsername))
                    using (var taskDisplayname = System.Threading.Tasks.Task.Run<PrincipalSearchResult<Principal>>(() => searcherDisplayname.FindAll()))
                    using (var taskUsername = System.Threading.Tasks.Task.Run<PrincipalSearchResult<Principal>>(() => searcherUsername.FindAll()))
                    {
                        foreach (var principal in (await taskDisplayname).Take(15).Union(await taskUsername).Take(15))
                        {
                            var userPrincipal = (UserPrincipal)principal;
                            using (userPrincipal)
                            {
                                if (userPrincipal.Guid == null || string.IsNullOrEmpty(userPrincipal.DisplayName) ||
                                    string.IsNullOrEmpty(userPrincipal.EmailAddress)) continue;
                                if (!users.IsAddingCompleted)
                                    users.Add(new ApplicationUserDTO
                                    {
                                        Id = userPrincipal.Guid != null ? userPrincipal.Guid.ToString() : string.Empty,
                                        DisplayName = string.IsNullOrEmpty(userPrincipal.DisplayName) ? string.Empty : userPrincipal.DisplayName.Trim(),
                                        UserName = string.IsNullOrEmpty(userPrincipal.DisplayName) ? string.Empty : userPrincipal.DisplayName.Trim(),
                                        Email = string.IsNullOrEmpty(userPrincipal.EmailAddress) ? string.Empty : userPrincipal.EmailAddress.Trim()
                                    });
                            }
                        }
                    }
                }
                //d2
                //Доп домен.
                var domanDmz = domainSettings.FirstOrDefault(ds => ds.Name == "DomainDmz");
                var domainUserDmz = domainSettings.FirstOrDefault(ds => ds.Name == "UserConnectionDomainDmz");
                var domainUserPassDmz = domainSettings.FirstOrDefault(ds => ds.Name == "UserPassConnectionDomainDmz");
                if (!string.IsNullOrEmpty(domanDmz?.Value))
                {
                    using (var adDmzName = new PrincipalContext(ContextType.Domain, domanDmz.Value.Trim(),
                        string.IsNullOrEmpty(domainUserDmz?.Value) ? null : domainUserDmz.Value.Trim(),
                        string.IsNullOrEmpty(domainUserPassDmz?.Value) ? null : HelperBll.Base64StringDecode(domainUserPassDmz.Value.Trim())))
                    {
                        using (var searchMaskDisplaynameD2 = new UserPrincipal(adDmzName) { Enabled = true, DisplayName = filterText })
                        using (var searchMaskUsernameD2 = new UserPrincipal(adDmzName) { Enabled = true, SamAccountName = filterText })
                        using (var searcherDisplaynameD2 = new PrincipalSearcher(searchMaskDisplaynameD2))
                        using (var searcherUsernameD2 = new PrincipalSearcher(searchMaskUsernameD2))
                        using (var taskDisplaynameD2 = Task.Run(() => searcherDisplaynameD2.FindAll()))
                        using (var taskUsernameD2 = Task.Run(() => searcherUsernameD2.FindAll()))
                        {
                            foreach (var principalD2 in (await taskDisplaynameD2).Take(15).Union(await taskUsernameD2).Take(15))
                            {
                                var userPrincipalD2 = (UserPrincipal)principalD2;
                                using (userPrincipalD2)
                                {
                                    if (userPrincipalD2.Guid == null || string.IsNullOrEmpty(userPrincipalD2.DisplayName) ||
                                        string.IsNullOrEmpty(userPrincipalD2.EmailAddress)) continue;
                                    if (!users.IsAddingCompleted)
                                        users.Add(new ApplicationUserDTO
                                        {
                                            Id = userPrincipalD2.Guid != null ? userPrincipalD2.Guid.ToString() : string.Empty,
                                            DisplayName = string.IsNullOrEmpty(userPrincipalD2.DisplayName) ? string.Empty : userPrincipalD2.DisplayName.Trim(),
                                            UserName = string.IsNullOrEmpty(userPrincipalD2.SamAccountName) ? "No SamAccountName" : userPrincipalD2.SamAccountName.Trim(),
                                            Email = string.IsNullOrEmpty(userPrincipalD2.EmailAddress) ? "No email" : userPrincipalD2.EmailAddress.Trim()
                                        });
                                }
                            }
                        }
                    }
                }
                users.CompleteAdding();
                return users.Distinct().ToList();
            }
            catch (Exception e)
            {
                loggingService.Error("FindUsers in domen" + e.Message);
                return null;
            }
        }

        public async Task<IEnumerable<ApplicationUserDTO>> GetDomainUsersByNameList(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    return new List<ApplicationUserDTO>();
                }
                return await FindUsers(userName.Replace("(titan2.ru)", string.Empty).Replace("(sep-spb.ru)", string.Empty).
                    Replace("(titan2.fi)", string.Empty).Replace("(msk.titan2.ru)", string.Empty).Replace("(ictas.com.tr)", string.Empty)
                    .Replace("(testmail.ru)", string.Empty).Replace("(sem.titan2.ru)", string.Empty).Replace("(t2ic.com)", string.Empty).Trim());
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<ApplicationUserDTO>();
            }
        }

        public async Task<OperationDetails> IsAuthenticated(string login, string pass)
        {
            try
            {
                bool isAuthenticated;
                if (string.IsNullOrEmpty(login))
                {
                    return new OperationDetails(false, "IsAuthenticated- Error: Empty login", "IsAuthenticated");
                }
                if (string.IsNullOrEmpty(pass))
                {
                    return new OperationDetails(false, "IsAuthenticated- Error: Empty pass", "IsAuthenticated");
                }
                //Основной домен
                var domanT2 = await Database.Settings.GetNameAsync("DomainT2");
                if (string.IsNullOrEmpty(domanT2?.Value))
                {
                    return new OperationDetails(false, "Not found Domain or Empty in Setting", "IsAuthenticated");
                }
                using (var adT2 = new PrincipalContext(ContextType.Domain, domanT2.Value))
                {
                    isAuthenticated = adT2.ValidateCredentials(login.Trim(), pass.Trim());
                }
                if (isAuthenticated)
                {
                    loggingService.Info("DomainT2 - IsAuthenticated -ok");
                    return new OperationDetails(true, "DomainT2 - IsAuthenticated", "IsAuthenticated");
                }
                else
                {
                    //Доп. домен
                    var domanDmz = await Database.Settings.GetNameAsync("DomainDmz");
                    if (string.IsNullOrEmpty(domanDmz?.Value))
                    {
                        loggingService.Error("Not found Domain or Empty in Setting");
                        return new OperationDetails(false, "Not found Domain", "IsAuthenticated");
                    }
                    using (var adDmz = new PrincipalContext(ContextType.Domain, domanDmz?.Value))
                    {
                        isAuthenticated = adDmz.ValidateCredentials(login.Trim(), pass.Trim());
                    }
                    if (isAuthenticated)
                    {
                        loggingService.Info("DomainDMZ - IsAuthenticated -ok");
                        return new OperationDetails(true, "DomainDmz - IsAuthenticated", "IsAuthenticated");
                    }
                    else
                    {
                        loggingService.Info("DomainDMZ - IsAuthenticated -fatal");
                        return new OperationDetails(false, "Invalid login or password", "IsAuthenticated");
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"IsAuthenticated - Error: {ex.Message}", "IsAuthenticated");
            }
        }

        public async Task<IEnumerable<ApplicationUserDTO>> GetUsers()
        {
            try
            {
                var users = await Database.UserManager.Users.ToListAsync();
                if (users.Any())
                {
                    var mapper = MapperAll.MapperConfigDto();
                    var dataRes = mapper.Map<IEnumerable<ApplicationUser>, List<ApplicationUserDTO>>(users);
                    return dataRes;
                }
                return null;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }

        public async Task<ApplicationUserDTO> GetDomainUserById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return null;
                }
                var userApp = await Database.UserManager.FindByIdAsync(id.ToString());
                if (userApp != null)
                {
                    loggingService.Info("User found AppUser");
                    return new ApplicationUserDTO
                    {
                        Id = userApp.Id,
                        DisplayName = userApp.DisplayName,
                        UserName = userApp.UserName,
                        Email = userApp.Email
                    };
                }
                var domanSettings = (await Database.Settings.GetAllAsync()).ToList();
                if (!domanSettings.Any())
                {
                    loggingService.Error("No Domain in Settings");
                    return new ApplicationUserDTO();
                }
                var doman = domanSettings.FirstOrDefault(ds => ds.Name == "DomainT2");
                if (string.IsNullOrEmpty(doman?.Value))
                {
                    loggingService.Error("No Domain in Settings");
                    return new ApplicationUserDTO();
                }
                var domainDc = domanSettings.FirstOrDefault(ds => ds.Name == "DomainDC");
                var domainUser = domanSettings.FirstOrDefault(ds => ds.Name == "UserConnectionDomain");
                var domainUserPass = domanSettings.FirstOrDefault(ds => ds.Name == "UserPassConnectionDomain");
                var pass = domainUserPass != null && !string.IsNullOrWhiteSpace(domainUserPass.Value) ? HelperBll.Base64StringDecode(domainUserPass.Value.Trim()) : null;
                UserPrincipal foundUserBasic;
                using (var ad = new PrincipalContext(ContextType.Domain, doman.Value.Trim(), string.IsNullOrEmpty(domainDc?.Value) ? string.Empty : domainDc.Value,
                    string.IsNullOrEmpty(domainUser?.Value) ? null : domainUser.Value.Trim(), string.IsNullOrEmpty(pass) ? null : pass.Trim()))
                {
                    foundUserBasic = UserPrincipal.FindByIdentity(ad, IdentityType.Guid, id.ToString());
                }
                if (foundUserBasic != null && !foundUserBasic.IsAccountLockedOut())
                {
                    loggingService.Info("User found D1");
                    return new ApplicationUserDTO
                    {
                        Id = foundUserBasic.Guid != null ? foundUserBasic.Guid.ToString() : string.Empty,
                        DisplayName = string.IsNullOrEmpty(foundUserBasic.DisplayName) ? string.Empty : foundUserBasic.DisplayName.Trim(),
                        UserName = string.IsNullOrEmpty(foundUserBasic.SamAccountName) ? string.Empty : foundUserBasic.SamAccountName.Trim(),
                        Email = string.IsNullOrEmpty(foundUserBasic.EmailAddress) ? string.Empty : foundUserBasic.EmailAddress.Trim()
                    };
                }
                //Доп домен.
                var domanDmz = domanSettings.FirstOrDefault(ds => ds.Name == "DomainDmz");
                var domainUserDmz = domanSettings.FirstOrDefault(ds => ds.Name == "UserConnectionDomainDmz");
                var domainUserPassDmz = domanSettings.FirstOrDefault(ds => ds.Name == "UserPassConnectionDomainDmz");
                loggingService.Info("Domain DMZ");
                if (!string.IsNullOrEmpty(domanDmz?.Value))
                {
                    UserPrincipal foundUserDmz;
                    using (var adDmzName = new PrincipalContext(ContextType.Domain, domanDmz.Value.Trim(),
                        string.IsNullOrEmpty(domainUserDmz?.Value) ? null : domainUserDmz.Value.Trim(),
                        string.IsNullOrEmpty(domainUserPassDmz?.Value) ? null : HelperBll.Base64StringDecode(domainUserPassDmz.Value.Trim())))
                    {
                        foundUserDmz = UserPrincipal.FindByIdentity(adDmzName, IdentityType.Guid, id.ToString());
                    }
                    if (foundUserDmz != null && !foundUserDmz.IsAccountLockedOut())
                    {
                        loggingService.Info("User found DMZ");
                        return new ApplicationUserDTO
                        {
                            Id = foundUserDmz.Guid != null ? foundUserDmz.Guid.ToString() : string.Empty,
                            DisplayName = string.IsNullOrEmpty(foundUserDmz.DisplayName) ? string.Empty : foundUserDmz.DisplayName.Trim(),
                            UserName = string.IsNullOrEmpty(foundUserDmz.SamAccountName) ? string.Empty : foundUserDmz.SamAccountName.Trim(),
                            Email = string.IsNullOrEmpty(foundUserDmz.EmailAddress) ? string.Empty : foundUserDmz.EmailAddress.Trim()
                        };
                    }
                }
                loggingService.Error("User by id not found");
                return null;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }
       
        public async Task<ApplicationUserDTO> GetDomainUsersByName(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    loggingService.Error("EmptyUserName");
                    return null;
                }
                //основной домен Т2
                var domanSettings = (await Database.Settings.GetAllAsync()).ToList();
                if (!domanSettings.Any())
                {
                    loggingService.Error("В настройках не найден домен");
                    return new ApplicationUserDTO();
                }
                var doman = domanSettings.FirstOrDefault(ds => ds.Name == "DomainT2");
                if (string.IsNullOrEmpty(doman?.Value))
                {
                    loggingService.Error("В настройках не найден домен");
                    return new ApplicationUserDTO();
                }
                var domainDc = domanSettings.FirstOrDefault(ds => ds.Name == "DomainDC");
                if(string.IsNullOrEmpty(domainDc?.Value))
                {
                    loggingService.Error("No domainDc in Settings");
                    return new ApplicationUserDTO();
                }

                var domainUser = domanSettings.FirstOrDefault(ds => ds.Name == "UserConnectionDomain");
                var domainUserPass = domanSettings.FirstOrDefault(ds => ds.Name == "UserPassConnectionDomain");
                var pass = domainUserPass != null && !string.IsNullOrWhiteSpace(domainUserPass.Value) ? HelperBll.Base64StringDecode(domainUserPass.Value.Trim()) : null;
                UserPrincipal foundUserBasic;
                using (var ad = new PrincipalContext(ContextType.Domain, doman.Value.Trim(), string.IsNullOrEmpty(domainDc?.Value) ? string.Empty : domainDc.Value,
                    string.IsNullOrEmpty(domainUser?.Value) ? null : domainUser.Value.Trim(), string.IsNullOrEmpty(pass) ? null : pass.Trim()))
                {
                    foundUserBasic = UserPrincipal.FindByIdentity(ad, IdentityType.SamAccountName, userName.Trim());
                }
                if (foundUserBasic != null && !foundUserBasic.IsAccountLockedOut())
                {
                    loggingService.Info("User found D1");
                    return new ApplicationUserDTO
                    {
                        Id = foundUserBasic.Guid != null ? foundUserBasic.Guid.ToString() : string.Empty,
                        DisplayName = string.IsNullOrEmpty(foundUserBasic.DisplayName) ? string.Empty : foundUserBasic.DisplayName.Trim(),
                        UserName = string.IsNullOrEmpty(foundUserBasic.SamAccountName) ? string.Empty : foundUserBasic.SamAccountName.Trim(),
                        Email = string.IsNullOrEmpty(foundUserBasic.EmailAddress) ? string.Empty : foundUserBasic.EmailAddress.Trim()
                    };
                }
                //Доп домен.
                var domanDmz = domanSettings.FirstOrDefault(ds => ds.Name == "DomainDmz");
                // var domainDcDmz = domanSettings.FirstOrDefault(ds => ds.Name == "DomainDCdmz");
                var domainUserDmz = domanSettings.FirstOrDefault(ds => ds.Name == "UserConnectionDomainDmz");
                var domainUserPassDmz = domanSettings.FirstOrDefault(ds => ds.Name == "UserPassConnectionDomainDmz");
                loggingService.Info("Domain DMZ");
                if (!string.IsNullOrEmpty(domanDmz?.Value))
                {
                    UserPrincipal foundUserDmz;
                    using (var adDmzName = new PrincipalContext(ContextType.Domain, domanDmz.Value.Trim(), string.IsNullOrEmpty(domainUserDmz?.Value) ? null : domainUserDmz.Value.Trim(), string.IsNullOrEmpty(domainUserPassDmz?.Value) ? null : HelperBll.Base64StringDecode(domainUserPassDmz.Value.Trim())))
                    {
                        foundUserDmz = UserPrincipal.FindByIdentity(adDmzName, IdentityType.SamAccountName, userName.Trim());
                    }
                    if (foundUserDmz != null && !foundUserDmz.IsAccountLockedOut())
                    {
                        loggingService.Info("User found DMZ");
                        return new ApplicationUserDTO
                        {
                            Id = foundUserDmz.Guid != null ? foundUserDmz.Guid.ToString() : string.Empty,
                            DisplayName = string.IsNullOrEmpty(foundUserDmz.DisplayName) ? string.Empty : foundUserDmz.DisplayName.Trim(),
                            UserName = string.IsNullOrEmpty(foundUserDmz.SamAccountName) ? string.Empty : foundUserDmz.SamAccountName.Trim(),
                            Email = string.IsNullOrEmpty(foundUserDmz.EmailAddress) ? string.Empty : foundUserDmz.EmailAddress.Trim()
                        };
                    }
                }
                loggingService.Error("Пользователь не найден в доменах");
                return new ApplicationUserDTO();
            }
            catch (Exception ex)
            {
                loggingService.Error("Error: User get name domain " + ex.Message + " " + ex.StackTrace);
                return new ApplicationUserDTO();
            }
        }

        public async Task<ApplicationUserDTO> GetUserByEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || !HelperBll.ValidateMail(email.Trim()))
                    return null;
                var user = await Database.UserManager.Users.SingleAsync(u => u.Email.Contains(email.Trim()) || u.UserName.Contains(email.Trim()));
                if (user != null)
                {
                    var result = new ApplicationUserDTO
                    {
                        Id = user.Id,
                        Email = string.IsNullOrEmpty(user.Email) ? string.Empty : user.Email.Trim(),
                        UserName = string.IsNullOrEmpty(user.UserName) ? string.Empty : user.UserName.Trim()
                    };
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }

        public async Task<ApplicationUserDTO> GetUserById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return null;
                var user = await Database.UserManager.FindByIdAsync(id.Trim());
                var mapper = MapperAll.MapperConfigDto();
                var dataRes = mapper.Map<ApplicationUser, ApplicationUserDTO>(user);
                return dataRes;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }

        public async Task<OperationDetails> UpdateUserAsync(ApplicationUserDTO model)
        {
            try
            {
                if (model == null)
                {
                    return new OperationDetails(false, "Отсутствует модель / Empty model", "UpdateUserAsync");
                }
                if (string.IsNullOrWhiteSpace(model.DisplayName))
                {
                    return new OperationDetails(false, "Введите отображаемое имя / Input display name", "CreateUser");
                }
                if (string.IsNullOrWhiteSpace(model.Email) || ! HelperBll.ValidateMail(model.Email))
                {
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", "CreateUser");
                }
                var user = await Database.UserManager.Users.FirstOrDefaultAsync(uu => uu.Id == model.Id);
                if (user != null)
                {
                    user.DisplayName = model.DisplayName.Trim();
                    user.Email = string.IsNullOrEmpty(model.Email) ? string.Empty : model.Email.Trim();
                    var up = await Database.UserManager.UpdateAsync(user);
                    if (up.Succeeded)
                    {
                        return new OperationDetails(true, "Успешно обновлено / Updated", "IsAuthenticated");
                    }
                    else
                    {
                        return new OperationDetails(false, "Данные не обновлены / Not completed", "IsAuthenticated");
                    }
                }
                return new OperationDetails(false, "Данные не обновлены / Not completed", "IsAuthenticated");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"UpdateUserAsync - Error: {ex.Message}", "IsAuthenticated");
            }
        }

        public async Task<OperationDetails> VerifyAndCreateIdentity(ApplicationUserDTO user, string password)
        {
            try
            {
                var isOk = false;
                if (user == null)
                {
                    return new OperationDetails(isOk, "⛔️ Empty User Model", "VerifyAndCreatedIdentity");
                }
                loggingService.Info($"Verify UserId{user.Id}");
                loggingService.Info($"Verify UserName{user.UserName}");
                loggingService.Info($"Verify UserSaccAcc{user.SecurityStamp}");
                loggingService.Info($"Verify UserEmail{user.Email}");
                //Verify
                if (string.IsNullOrEmpty(user.Id))
                {
                    loggingService.Error("UserId is empty");
                    return new OperationDetails(isOk, "Неверный id / User invalid", "VerifyAndCreatedIdentity");
                }
                if (string.IsNullOrWhiteSpace(user.UserName))
                {
                    loggingService.Error("UserName is empty");
                    return new OperationDetails(isOk, "Неверное имя пользователя / User invalid", "VerifyAndCreatedIdentity");
                }
                if (string.IsNullOrWhiteSpace(user.Email) || !HelperBll.ValidateMail(user.Email))
                {
                    loggingService.Error("UserName invalid email");
                    return new OperationDetails(isOk, "Неверный email пользователя / User invalid", "VerifyAndCreatedIdentity");
                }
                //Проверяем есть ли пользователь в Identity
                var userIdentity = await Database.UserManager.FindByIdAsync(user.Id);
                if (userIdentity == null)
                {
                    //Add user Identity
                    var userSave = new ApplicationUser
                    {
                        Id = user.Id,
                        DisplayName = !string.IsNullOrWhiteSpace(user.DisplayName) ? user.DisplayName.Trim() : string.Empty,
                        UserName = user.UserName.Trim(),
                        Email = user.Email.Trim(),
                        EmailConfirmed = true,
                        LockoutEnabled = false
                    };
                    //Добавляем пользователя и роль
                    var resultUser = await Database.UserManager.CreateAsync(userSave, string.IsNullOrEmpty(password) ? "AuditSystem123456!_" : password.Trim());
                    if (resultUser.Succeeded)
                    {
                        var verifyRole = await VerifyRole("user");
                        if (verifyRole.Succedeed)
                        {
                            //Add User Role
                            var addRes = await Database.UserManager.AddToRoleAsync(userSave.Id, "user");
                            if (addRes.Succeeded)
                            {
                                isOk = true;
                            }
                            else
                            {
                                loggingService.Error($"Ошибка при создании роли - user, сотрудник : {userSave.UserName}");
                            }
                        }
                    }
                    else
                    {
                        loggingService.Error($"Ошибка при создании user, сотрудник : {userSave.UserName}");
                    }
                }
                //Пользователь существует в БД 
                //Проверяем есть ли роли у сотрудника
                else
                {
                    if (userIdentity.LockoutEnabled)
                    {
                        loggingService.Error($"User {user.Email} - not added to database");
                        return new OperationDetails(isOk, "⛔️ Доступ запрещен / Access is denied", "VerifyAndCreatedIdentity");
                    }
                    var userRoles = (await Database.UserManager.GetRolesAsync(user.Id)).ToList();
                    if (!userRoles.Any())
                    {
                        loggingService.Error($"User {user.Email} - Role not added");
                        return new OperationDetails(isOk, "⛔️ Роль не найдена / Role not found", "VerifyAndCreatedIdentity");
                    }
                    isOk = true;
                }
                if (isOk)
                {
                    return new OperationDetails(true, "✔️ Ok", "VerifyAndCreatedIdentity");
                }
                else
                {
                    return new OperationDetails(false, "⛔️ Доступ запрещен / Access is denied", "VerifyAndCreatedIdentity");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"⛔️ VerifyAndCreateIdentity - Error: {ex.Message}", "VerifyAndCreatedIdentity");
            }
        }

        //Роль
        private async Task<OperationDetails> VerifyRole(string roleName)
        {
            try
            {
                if (!string.IsNullOrEmpty(roleName))
                {
                    loggingService.Info($"Проверяем роль:{roleName}");
                    //Получаем роль
                    var roleExists = await Database.RoleManager.RoleExistsAsync(roleName.ToLower().Trim());
                    //Роли нет
                    if (!roleExists)
                    {
                        loggingService.Info($"Создаем роль:{roleName}");
                        var role = new ApplicationRole { Name = roleName.ToLower().Trim() };
                        var roleAddRes = await Database.RoleManager.CreateAsync(role);
                        if (roleAddRes.Succeeded)
                        {
                            loggingService.Info($"Роль создана {roleName}");
                            return new OperationDetails(true, "Роль создана", "VerifyRole");
                        }
                        else
                        {
                            loggingService.Error($"Ошибка при создании роли {roleName}");
                            return new OperationDetails(false, "Ошибка при создании роли", "VerifyRole");
                        }
                    }
                    //роль найдена
                    else
                    {
                        return new OperationDetails(true, "Роль определена", "VerifyRole");
                    }
                }
                return new OperationDetails(false, "Введите наименование роли", "VerifyRole");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"VerifyRole - Error: {ex.Message}", "VerifyRole");
            }
        }

        public async Task<OperationDetails> CreateUserAsync(ApplicationUserDTO user, string password)
        {
            if (user == null)
            {
                return new OperationDetails(false, "⛔️ Empty model user", "CreateUser");
            }
            if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                return new OperationDetails(false, "⛔️ Введите отображаемое имя / Empty display name", "CreateUser");
            }
            if (string.IsNullOrEmpty(password))
            {
                return new OperationDetails(false, "⛔️ Отсутствует пароль / Empty password", "CreateUser");
            }
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email))
            {
                return new OperationDetails(false, "⛔️ Заполнены не все обязательные поля / Not all required fields are filled in", "CreateUser");
            }
            var userDublicat = await Database.UserManager.FindByEmailAsync(user.Email);
            if (userDublicat != null)
            {
                return new OperationDetails(false, "⛔️ Пользователь уже существует в БД / User exists", "CreateUser");
            }
            var saveModel = new ApplicationUser
            {
                Id = user.Id,
                UserName = user.UserName.Trim(),
                DisplayName = user.DisplayName.Trim(),
                Email = user.Email.Trim(),
                EmailConfirmed = true,
                IsExternal = user.IsExternal,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = false
            };
            //Создаем пользователя
            var resultSave = await Database.UserManager.CreateAsync(saveModel, password);
            if (resultSave.Errors != null && resultSave.Errors.Any())
                return new OperationDetails(false, resultSave.Errors.FirstOrDefault(), "");
            // добавляем роль
            if (user.Roles == null || !user.Roles.Any())
            {
                var resultRole = await Database.UserManager.AddToRoleAsync(user.Id, "user");
                if (resultRole.Succeeded)
                {
                    loggingService.Info("Completed UserRole");
                    return new OperationDetails(true, "✔️ Успешно добавлено  / Completed", "CreateUser");
                }
                else
                {
                    loggingService.Error(string.Join(" | ", resultRole.Errors != null && resultRole.Errors.Any() ? (IEnumerable)resultRole.Errors.ToArray() : new List<string>()));
                }
            }
            return new OperationDetails(true, "✔️ Успешно добавлено / Completed", "CreatedUser");
        }
    }
}

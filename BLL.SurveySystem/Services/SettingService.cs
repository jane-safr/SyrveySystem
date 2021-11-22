using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Helpers;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;

namespace BLL.SurveySystem.Services
{
    public class SettingService : ISettingService
    {
        IUnitOfWork Database { get; set; }
        readonly ILoggerService<SettingService> loggingService;
        public SettingService(IUnitOfWork uow, ILoggerService<SettingService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }
        public async Task<SettingDTO> GetSetting(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new SettingDTO();
                var setting = await Database.Settings.GetAsync(id);
                if (setting != null)
                {
                    var mapper = MapperAll.MapperConfigSettings();
                    var result = mapper.Map<Setting, SettingDTO>(setting);
                    return result;
                }
                loggingService.Error($"No Settings Data at Id={id}");
                return new SettingDTO();
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new SettingDTO();
            }
        }
        public async Task<SettingDTO> GetSettingName(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    loggingService.Error("Empty parameter to filter Settings");
                    return new SettingDTO();
                }
                var setting = await Database.Settings.GetNameAsync(name);
                if (setting != null)
                {
                    var mapper = MapperAll.MapperConfigSettings();
                    var result = mapper.Map<Setting, SettingDTO>(setting);
                    return result;
                }
                loggingService.Error($"No Settings Data at Name={name}");
                return new SettingDTO();
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new SettingDTO();
            }
        }

        public async Task<IEnumerable<SettingDTO>> GetSettings()
        {
            try
            {
                var mapper = MapperAll.MapperConfigSettings();
                var result = await Database.Settings.GetAllAsync();
                var settings = mapper.Map<IEnumerable<Setting>, List<SettingDTO>>(result);
                return settings;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<SettingDTO>();
            }
        }

        public async Task<OperationDetails> UpdateSetting(SettingDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("Setting update model empty");
                    return new OperationDetails(false, "Пустая модуль / Empty model", "UpdateSetting");
                }
                var setting = await Database.Settings.GetAsync(model.SettingId);
                if (setting == null)
                {
                    loggingService.Error($"No Setting in Db with ID = {model.SettingId}");
                    return new OperationDetails(false, "Настройка не найдена / Data not found", "UpdateSetting");
                }
                if (string.IsNullOrWhiteSpace(model.Description))
                {
                    return new OperationDetails(false, "Введите описание / Enter description", "UpdateSetting");
                }
                loggingService.Info($"Update: ValueOld:{setting.Value} -> ValueNew:{model.Value}");
                setting.Value = string.IsNullOrEmpty(model.Value) ? null : model.Value.Trim();
                setting.Description = model.Description.Trim();
                Database.Settings.Update(setting);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Настройки успешно обновлены / Completed", "UpdateSetting");
                }
                else
                {
                    return new OperationDetails(false, "Настройки не обновлены / Not Completed", "UpdateSetting");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateSetting");
            }
        }
    }
}

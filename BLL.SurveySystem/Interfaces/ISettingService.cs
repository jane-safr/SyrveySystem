using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface ISettingService
    {
        Task<OperationDetails> UpdateSetting(SettingDTO model);
        Task<SettingDTO> GetSetting(Guid id);
        Task<SettingDTO> GetSettingName(string name);
        Task<IEnumerable<SettingDTO>> GetSettings();
    }
}
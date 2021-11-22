using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface INotificationTypeService
    {
        Task<OperationDetails> CreateAsync(NotificationTypeDTO model);
        Task<OperationDetails> UpdateNotificationType(NotificationTypeDTO model);
        Task<NotificationTypeDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<NotificationTypeDTO>> GetAllAsync();
        Task<OperationDetails> DeleteAsync(Guid id);
        Task<IEnumerable<NotificationTypeDTO>> FindByFilterAsync(List<FilterModels> filterModels);
    }
}
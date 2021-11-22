using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface INotificationService
    {
        Task<OperationDetails> CreateAsync(NotificationDTO model);
        Task<OperationDetails> CancelAsync(Guid notifyId);
        Task<IEnumerable<NotificationDTO>> GetNotSentNotifications();
        Task<IEnumerable<NotificationDTO>> FindByFilterAsync(List<FilterModels> filterModels);
        Task<IEnumerable<NotificationDTO>> GetNotificationById(Guid id);
        Task<IEnumerable<NotificationDTO>> GetNotificationBySurveyCode(int taskCode);
    }
}
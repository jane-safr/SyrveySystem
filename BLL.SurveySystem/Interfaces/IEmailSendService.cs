using System.Threading.Tasks;
using BLL.SurveySystem.Infrastructure;

namespace BLL.SurveySystem.Interfaces
{
    public interface IEmailSendService
    {
        Task<OperationDetails> SendEmailsAsync();
    }
}
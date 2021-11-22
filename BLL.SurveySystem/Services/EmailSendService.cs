using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO.Base;
using BLL.SurveySystem.Helpers;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BLL.SurveySystem.Services
{
    public class EmailSendService : IEmailSendService
    {
        private IUnitOfWork Database { get; set; }
        private readonly ILoggerService<EmailSendService> loggingService;
        public EmailSendService(IUnitOfWork uow, ILoggerService<EmailSendService> logServ)
        {
            this.Database = uow;
            this.loggingService = logServ;
        }
        public async Task<OperationDetails> SendEmailsAsync()
        {
            try
            {
                // список сообщений из Notification 
                var listNotifications = (await Database.Notifications.FindAsync(x => x.IsActive && x.IsSend == false)).OrderBy(x => x.CreatedOn).Take(20).ToList();
                if (!listNotifications.Any())
                {
                    loggingService.Info("No data to send");
                    return new OperationDetails(false, "Нет данных для отправки / No data to send", "SendEmailsAsync");
                }
                // настройки
                var settings = (await Database.Settings.GetAllAsync()).ToList();
                if (!settings.Any())
                {
                    loggingService.Error("Settings cannot be loaded");
                    return new OperationDetails(false, "Настройки не загружены / Settings cannot be loaded", string.Empty);
                }
                var addressEmail = settings.FirstOrDefault(x => x.Name == "AddressEmail");
                if (string.IsNullOrEmpty(addressEmail?.Value))
                {
                    loggingService.Error("AddressEmail is empty");
                    return new OperationDetails(false, "Пустой адрес отправителя / AddressEmail is empty", string.Empty);
                }
                var smtpAddress = settings.FirstOrDefault(x => x.Name == "SmtpAddress");
                if (string.IsNullOrEmpty(smtpAddress?.Value))
                {
                    loggingService.Error("No smtpAddress in DataBase");
                    return new OperationDetails(false, "Не указан smtpAddress / No smtpAddress", string.Empty);
                }
                var smtpPort = settings.FirstOrDefault(x => x.Name == "SmptPort");
                if (string.IsNullOrEmpty(smtpPort?.Value))
                {
                    loggingService.Error("No SmptPort");
                    return new OperationDetails(false, "Не указан SmptPort / No SmptPort", string.Empty);
                }
                var smtpEmailPass = settings.FirstOrDefault(x => x.Name == "SmtpEmailPass");
                if (smtpEmailPass != null && string.IsNullOrEmpty(smtpEmailPass.Value))
                {
                    smtpEmailPass.Value = string.Empty;
                }
                var secureSocketOptions = settings.FirstOrDefault(x => x.Name == "SmtpSecureSocketOptions");
                if (string.IsNullOrWhiteSpace(secureSocketOptions?.Value))
                {
                    loggingService.Error("Invalid SmtpSecureSocketOptions");
                    return new OperationDetails(false, "Не указан SmtpSecureSocketOptions / Invalid SmtpSecureSocketOptions", string.Empty);
                }
                // добавляем проверенные настройки в экз класса
                var validSet = new ValidSetting
                {
                    AddressEmail = addressEmail.Value.Trim(),
                    SmtpAddress = smtpAddress.Value,
                    SmtpPort = smtpPort.Value,
                    SmtpEmailPass = smtpEmailPass?.Value,
                    SecSocketOptions = secureSocketOptions.Value
                };
                loggingService.Info("Settings Loaded");
                //Много потчная отправка
                await Task.WhenAll(listNotifications.Select(message => Task.Run(() => SendMessagesAsync(message, validSet))));
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, $"Emails sent = {res}", "SendMessageEmail");
                }
                else
                {
                    return new OperationDetails(false, "Error Save db", "SendMessageEmail");
                }
            }
            catch (Exception e)
            {
                loggingService.Error($"Error: {e.Message}");
                return new OperationDetails(false, $"Ошибка при отправке сообщения: {e.Message}", "SendMessageEmail");
            }
        }

        private async Task<OperationDetails> SendMessagesAsync(Notification notify, ValidSetting setting)
        {
            try
            {
                loggingService.Info($"Start Task:{Task.CurrentId}");
                if (notify == null)
                {
                    loggingService.Info("Notify null");
                    return new OperationDetails(false, "Notify null", string.Empty);
                }
                if (notify.NotificationId == Guid.Empty)
                {
                    loggingService.Info("NotifyId empty");
                    return new OperationDetails(false, "NotifyId empty", string.Empty);
                }
                if (setting == null)
                {
                    loggingService.Info("Email send Setting null");
                    return new OperationDetails(false, "Email send Setting null", string.Empty);
                }
                //Text Mess
                if (string.IsNullOrWhiteSpace(notify.EmailText))
                {
                    loggingService.Info($"emailTxt empty for {notify.NotificationId}");
                    return new OperationDetails(false, "No Email Text", string.Empty);
                }
                //EmailTo (Возможно добавить через ;)
                if (string.IsNullOrWhiteSpace(notify.EmailTo))
                {
                    loggingService.Error("Empty emails recipient");
                    return new OperationDetails(false, "Invalid email recipients", string.Empty);
                }
                loggingService.Info($"Task:{Task.CurrentId} - EmailTo: {notify.EmailTo}");
                if (!HelperBll.ValidateMail(setting.AddressEmail))
                {
                    loggingService.Error("Email from invalid");
                    return new OperationDetails(false, "Email from invalid", string.Empty);
                }
                //Цикл
                foreach (var emailP in notify.EmailTo.ToLower().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) // EmailTo = oo получателей
                {
                    if (string.IsNullOrWhiteSpace(emailP))
                    {
                        loggingService.Warn($"Email {emailP} - empty");
                        continue;
                    }
                    var email = emailP.Trim();
                    if (!HelperBll.ValidateMail(email.Trim()))
                    {
                        loggingService.Error($"Email {email} invalid");
                        continue;
                    }
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("", setting.AddressEmail));
                    message.To.Add(new MailboxAddress("", email));
                    message.Subject = "📝 Survey System";
                    var headerHtmlBody = $@"
                        <html>
                         <head>
                        <meta http-equiv='Content-Type' content='text/html; charset=utf-8'/>
                        <title>Survey System Email</title>
                        <style type='text/css'>
                         body {{margin: 0; padding: 0; min-width: 100%!important;}}
                        .content {{width: 100%; max-width: 900px;}}
		                .header {{padding: 40px 30px 20px 30px;}}
		                .innerpadding {{padding: 30px 30px 30px 30px;}}
		                .borderbottom {{border-bottom: 1px solid #f2eeed;}}
		                .h2 {{padding: 0 0 15px 0; font-size: 24px; line-height: 28px; font-weight: bold; color:#01DF01;}}
		                .bodycopy {{font-size: 16px; line-height: 22px; padding: 7px;}}
		                .bodyfooter {{font-size: 9px; text-align:justify; padding: 7px;}}
		                @media only screen and (min-device-width: 601px) 
			                {{
				                .content {{width: 600px !important;}}
			                }}			
                        </style>
                        </head>
                        <body yahoo bgcolor='#f7f7f7'>
	                    <!--[if (gte mso 9)|(IE)]>
                        <table width='100%' bgcolor='#f7f7f7' border='0' cellpadding='1' cellspacing='1'>
                            <tr>
                                <td>
				                <![endif]-->
                                    <table class='content' align='center' cellpadding='1' cellspacing='1' border='0'>
                                        <tr>
							                <td class='innerpadding borderbottom'>
								                <table width='100%' border='0'cellspacing='1' cellpadding='1'>									               
									                <tr>
										                <td class='bodycopy' colspan='2'> {notify.EmailText} </td>                                            										               
									                </tr>
                                                    <tr><td colspan='2'>";
                    const string footerHtmlBody = @"</td></tr><tr>
										                             <td  width='50%' align='center' class='bodyfooter'><b>ЗАКЛЮЧЕНИЕ О КОНФИДЕНЦИАЛЬНОСТИ.</b></td>
		                                                             <td  width='50%' align='center' class='bodyfooter'><b>CONFIDENTIALITY REPORT</b></td>					                              
                                                                  </tr>
                                                                  <tr>
                                                                     <td  width='50%' align='justify' class='bodyfooter'>Данное электронное сообщение содержит конфиденциальную информацию. Оно предназначено исключительно адресату. Всем прочим лицам доступ к данному сообщению запрещен.</td>	
                                                                     <td  width='50%' align='justify' class='bodyfooter'>This e-mail contains confidential information. It is intended exclusively for the addressee. All other persons are not allowed to access this message.</td>
                                                                  </tr>
								                            </table>
							                            </td>
						                            </tr>
                                                </table>
					                            <!--[if (gte mso 9)|(IE)]>
                                            </td>
                                        </tr>
                                    </table>
		                            <![endif]-->
                                </body>
                    </html>";
                    var htmlBody = new StringBuilder();
                    //Если есть LinkUrl and Vaild Url
                    if (!string.IsNullOrWhiteSpace(notify.EmailUrl) && HelperBll.CheckUrlValid(notify.EmailUrl))
                    {
                        htmlBody.Append(
                            $@"<table width='100%' border='0' cellpadding='5' style ='border-collapse: collapse;'>
                                    <tr> 
									    <td colspan='2' style='background-color:#6a9e21; border-radius: 20px;border-color: #4c5764;border: 2px solid #45b7af;padding: 10px;text-align: center;'>
										    <a style='display: block;color:#ffffff;font-size: 20px;text-decoration: none;text-transform: uppercase;' target='_blank' href='{notify.EmailUrl}'> 
											Перейти / Go to ➡️
											</a>
										</td>
									</tr>
                             </table>");
                    }
                    var builder = new BodyBuilder
                    {
                        HtmlBody = headerHtmlBody + htmlBody + footerHtmlBody
                    };
                    message.Body = builder.ToMessageBody();
                    using (var client = new SmtpClient())
                    {
                        loggingService.Info($"Task:{Task.CurrentId} - Set SmtpClient");
                        client.ServerCertificateValidationCallback = (s, c, h, ee) => true;
                        switch (setting.SecSocketOptions)
                        {
                            case "StartTls":
                                await client.ConnectAsync(setting.SmtpAddress.Trim(), int.Parse(setting.SmtpPort), SecureSocketOptions.StartTls);
                                break;
                            case "NONE":
                                await client.ConnectAsync(setting.SmtpAddress.Trim(), int.Parse(setting.SmtpPort), SecureSocketOptions.None);
                                break;
                            case "AUTO":
                                await client.ConnectAsync(setting.SmtpAddress.Trim(), int.Parse(setting.SmtpPort));
                                break;
                            default:
                                throw new NotImplementedException("Unrecognized UserValidationResult value Email - SecureSocketOptions.");
                        }
                        if (!string.IsNullOrWhiteSpace(setting.SmtpEmailPass))
                        {
                            loggingService.Info("Set login and Pass");
                            client.AuthenticationMechanisms.Clear();
                            client.AuthenticationMechanisms.Add("LOGIN");
                            await client.AuthenticateAsync(Encoding.ASCII, setting.AddressEmail.Trim(), setting.SmtpEmailPass.Trim());
                        }
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);
                    }
                }
                //Notify update
                notify.IsSend = true;
                notify.DateSend = DateTime.Now;
                Database.Notifications.Update(notify);
                loggingService.Info($"Send email to {notify.EmailTo}");
                return new OperationDetails(true, "Уведомления отправлены / Send Notifications", string.Empty);
            }
            catch (Exception e)
            {
                loggingService.Error($"Error: {e.Message}");
                return new OperationDetails(false, $"Ошибка при отправке сообщения: {e.Message}", "SendMessageEmail");
            }
        }
    }
}

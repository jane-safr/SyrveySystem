using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;

namespace BLL.SurveySystem.Services
{
    public class UploadedFileService : IUploadedFileService
    {
        IUnitOfWork Database { get; set; }
        private readonly ILoggerService<UploadedFileService> loggingService;

        public UploadedFileService(IUnitOfWork uow, ILoggerService<UploadedFileService> logServ)
        {
            this.Database = uow;
            this.loggingService = logServ;
        }
        public async Task<OperationDetails> CreateAsync(UploadedFileDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("Empty Model");
                    return new OperationDetails(false, "Пустая модель / Empty Model", string.Empty);
                }
                if (model.UploadedFileId == Guid.Empty)
                {
                    loggingService.Error("Empty Id");
                    return new OperationDetails(false, "Отсутствует Id файла / Empty File Id", string.Empty);
                }
                if (string.IsNullOrEmpty(model.PathFile))
                {
                    loggingService.Error("Empty PathFile");
                    return new OperationDetails(false, "Отсутствует путь к файлу / Empty File path", string.Empty);
                }
                if (model.ApplicationUserId == Guid.Empty)
                {
                    loggingService.Error("Empty ApplicationUserId");
                    return new OperationDetails(false, "Отсутствует Id пользователя / Empty ApplicationUserId", string.Empty);
                }
                if (string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Empty CreatedBy");
                    return new OperationDetails(false, "Отсутствует поле Создан / Empty CreatedBy UserName", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / CreatedOn value is not valid", string.Empty);
                }

                var user = await Database.UserManager.FindByIdAsync(model.ApplicationUserId.ToString().ToLower());
                if (user == null)
                {
                    loggingService.Error("ApplicationUserId not found");
                    return new OperationDetails(false, "Пользователь не найден / User not found", string.Empty);
                }
                //Если указана temp
                var pathShare = (await Database.Settings.GetNameAsync("ShareBasePath"))?.Value;
                if (string.IsNullOrEmpty(pathShare))
                {
                    loggingService.Error("No ShareBasePath in database");
                    return new OperationDetails(false, "Отсутствует общий путь / Empty PathShare", string.Empty);
                }
                var pathFile = Path.Combine(pathShare, model.PathFile);
                if (!File.Exists(pathFile))
                {
                    loggingService.Error($"File does not exist {pathFile}");
                    return new OperationDetails(false, "Файл не существует / File does not exist", string.Empty);
                }
                var dublicateId = await Database.UploadedFiles.GetAsync(model.UploadedFileId);
                // валидация
                if (dublicateId != null)
                {
                    loggingService.Error($"UploadedFileId already exists: {model.UploadedFileId}");
                    return new OperationDetails(false, "Файл с таким Id уже существует / UploadedFileId already exists", string.Empty);
                }
                var modelSave = new UploadedFile
                {
                    UploadedFileId = model.UploadedFileId,
                    CreatedBy = model.CreatedBy,
                    CreatedOn = model.CreatedOn,
                    ApplicationUserId = model.ApplicationUserId,
                    PathFile = model.PathFile.Trim(),
                    FileName = model.FileName.Trim(),
                    FileSize = model.FileSize,
                    IsActive = model.IsActive,
                    FileType = model.FileType
                };
                Database.UploadedFiles.Create(modelSave);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Файл загружен / Completed", "CreateUploadedFile");
                }
                else
                {
                    loggingService.Error("Error Adding File");
                    return new OperationDetails(false, "Ошибка при добавлении / Not Completed", "CreateUploadedFile");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "");
            }
        }

        public async Task<OperationDetails> DeleteAsync(Guid uploadedFileId)
        {
            try
            {
                if (uploadedFileId == Guid.Empty)
                {
                    loggingService.Error("Empty uploadedFileId");
                    return new OperationDetails(false, "Отсутствует Id файла / Empty File Id", string.Empty);
                }
                var file = await Database.UploadedFiles.GetAsync(uploadedFileId);
                if (file != null)
                {
                    var shareBasePath = (await Database.Settings.GetNameAsync("ShareBasePath"))?.Value;
                    if (string.IsNullOrEmpty(shareBasePath))
                    {
                        loggingService.Error("Empty PathShare in settings");
                        return new OperationDetails(false, "Отсутствует общий путь / Empty PathShare", string.Empty);
                    }
                    var pathFile = Path.Combine(shareBasePath, file.PathFile); //Delete File
                    if (File.Exists(pathFile))
                    {
                        File.Delete(pathFile);
                    }
                    //Delete row in Database
                    await Database.UploadedFiles.DeleteAsync(file.UploadedFileId);
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete Completed", "DeleteUploadedFile");
                    }
                    else
                    {
                        loggingService.Warn("Delete UploadedFile none Item");
                        return new OperationDetails(false, "Не удалено / Delete Not Completed", "DeleteUploadedFile");
                    }
                }
                loggingService.Warn($"Delete UploadedFile None Item {uploadedFileId}");
                return new OperationDetails(false, "Не удалено / Delete Not Completed", "DeleteUploadedFile");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteUploadedFile");
            }
        }

        public async Task<UploadedFileDTO> GetByIdAsync(Guid uploadedFileId)
        {
            try
            {
                if (uploadedFileId == Guid.Empty)
                {
                    loggingService.Error("Empty uploadedFileId (no guid)");
                    return null;
                }
                var file = await Database.UploadedFiles.GetAsync(uploadedFileId);
                var config = new MapperConfiguration(c =>
                {
                    c.CreateMap<UploadedFile, UploadedFileDTO>();
                });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();
                var modelDto = mapper.Map<UploadedFile, UploadedFileDTO>(file);
                return modelDto;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return null;
            }
        }
    }
}
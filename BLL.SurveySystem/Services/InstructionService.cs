using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Helpers;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;

namespace BLL.SurveySystem.Services
{
    public class InstructionService : IInstructionService
    {
        IUnitOfWork Database { get; set; }
        readonly ILoggerService<InstructionService> loggingService;

        public InstructionService(IUnitOfWork uow, ILoggerService<InstructionService> loggingService)
        {
            this.Database = uow;
            this.loggingService = loggingService;
        }

        public async Task<OperationDetails> CreateAsync(InstructionDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("InstructionDTO is Empty");
                    return new OperationDetails(false, "Инструкция отсуствует / Instruction is Empty", string.Empty);
                }

                if (model.InstructionId == Guid.Empty || string.IsNullOrWhiteSpace(model.NameRus) ||
                    string.IsNullOrWhiteSpace(model.NameEng) || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false,
                        "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }

                if (!HelperBll.ValidateText(model.NameRus)) // check text
                {
                    loggingService.Error("Instruction Rus is not valid");
                    return new OperationDetails(false,
                        "Наименование Rus инструкции недействительно / Instruction Rus name is not valid",
                        string.Empty);
                }

                if (!HelperBll.ValidateText(model.NameEng)) // check text
                {
                    loggingService.Error("Instruction Eng is not valid");
                    return new OperationDetails(false,
                        "Наименование Eng инструкции недействительно / Instruction Eng name is not valid",
                        string.Empty);
                }

                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("CreatedOn value is not valid");
                    return new OperationDetails(false,
                        "Значение даты создания не действительно / CreatedOn value is not valid", string.Empty);
                }

                var duplicateId = await Database.Instructions.GetAsync(model.InstructionId);
                if (duplicateId != null)
                {
                    loggingService.Error($"Duplicate in InstructionId by ID = {model.InstructionId}");
                    return new OperationDetails(false, "Дубликат / Duplicate by Id ", "Duplicate");
                }

                if (model.IsActive && model.IsAdmin) // only 1 active manual admin
                {
                    var instrAct = (await Database.Instructions.FindAsync(x =>
                        x.IsActive && x.IsAdmin && x.InstructionId != model.InstructionId)).ToList();
                    if (instrAct.Any())
                    {
                        return new OperationDetails(false, "Актуальная инструкция администатора уже существует / Active admin manual exists", "Duplicate");
                    }
                }

                if (model.IsActive && !model.IsAdmin) // only 1 active manual user
                {
                    var instrAct = (await Database.Instructions.FindAsync(x =>
                        x.IsActive && !x.IsAdmin && x.InstructionId != model.InstructionId)).ToList();
                    if (instrAct.Any())
                    {
                        return new OperationDetails(false,
                            "Актуальная инструкция пользователя уже существует / Active user manual exists", "Duplicate");
                    }
                }

                var saveModel = new Instruction
                {
                    InstructionId = model.InstructionId,
                    CreatedOn = model.CreatedOn,
                    NameRus = HelperBll.DeleteRowTabToText(model.NameRus.Trim()),
                    NameEng = HelperBll.DeleteRowTabToText(model.NameEng.Trim()),
                    IsAdmin = model.IsAdmin,
                    Code = Math.Abs(model.InstructionId.GetHashCode()),
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive
                };
                Database.Instructions.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Completed", "CreateInstruction");
                }
                else
                {
                    loggingService.Error("Not save dataBase");
                    return new OperationDetails(false, "Не добавлено / Not Completed", "CreateInstruction");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new OperationDetails(false, "Не добавлено / Not Completed", "CreateInstruction");
            }
        }

        public async Task<OperationDetails> UpdateAsync(InstructionDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("InstructionDTO Model Empty");
                    return new OperationDetails(false, "Instruction Model is Empty", string.Empty);
                }

                if (model.InstructionId == Guid.Empty || string.IsNullOrWhiteSpace(model.NameRus) ||
                    string.IsNullOrWhiteSpace(model.NameEng))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false,
                        "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }

                if (!HelperBll.ValidateText(model.NameRus)) // check text
                {
                    loggingService.Error("Instruction Rus is not valid");
                    return new OperationDetails(false,
                        "Наименование Rus инструкции недействительно / Instruction Rus name is not valid",
                        string.Empty);
                }

                if (!HelperBll.ValidateText(model.NameEng)) // check text
                {
                    loggingService.Error("Instruction Eng is not valid");
                    return new OperationDetails(false,
                        "Наименование Eng инструкции недействительно / Instruction Eng name is not valid",
                        string.Empty);
                }

                if (model.Code < 0)
                {
                    loggingService.Error($"Code is invalid {model.Code}");
                    return new OperationDetails(false, "Код недействителен / Code is invalid", string.Empty);
                }

                var instr = await Database.Instructions.GetAsync(model.InstructionId);
                if (instr == null)
                {
                    loggingService.Error("Data not found");
                    return new OperationDetails(false, "Данные не найдены / Data not found", string.Empty);
                }

                if (model.IsActive && model.IsAdmin) // only 1 active manual admin
                {
                    var instrAct = (await Database.Instructions.FindAsync(x =>
                        x.IsActive && x.IsAdmin && x.InstructionId != model.InstructionId)).ToList();
                    if (instrAct.Any())
                    {
                        return new OperationDetails(false,
                            "Актуальная инструкция администатора уже существует / Active admin manual exists",
                            "Duplicate");
                    }
                }

                if (model.IsActive && !model.IsAdmin) // only 1 active manual user
                {
                    var instrAct = (await Database.Instructions.FindAsync(x =>
                        x.IsActive && !x.IsAdmin && x.InstructionId != model.InstructionId)).ToList();
                    if (instrAct.Any())
                    {
                        return new OperationDetails(false,
                            "Актуальная инструкция пользователя уже существует / Active user manual exists",
                            "Duplicate");
                    }
                }

                loggingService.Warn($"Update: NameOld:{instr.NameRus} -> NameNew:{model.NameRus}");
                loggingService.Warn($"Update: NameOld:{instr.NameEng} -> NameNew:{model.NameEng}");
                loggingService.Warn($"Update: IsActiveOld:{instr.IsActive} -> IsActiveNew:{model.IsActive}");
                //Update
                instr.NameRus = HelperBll.DeleteRowTabToText(model.NameRus.Trim());
                instr.NameEng = HelperBll.DeleteRowTabToText(model.NameEng.Trim());
                instr.IsActive = model.IsActive;
                instr.IsAdmin = model.IsAdmin;
                Database.Instructions.Update(instr);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateInstruction");
                }
                else
                {
                    loggingService.Error("Not Completed");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateInstruction");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateInstruction");
            }
        }

        // удаляем, если нет документов (рус англ), иначе - неактивный
        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Empty InstructionId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", "DeleteInstructionId");
                }

                var instr = await Database.Instructions.GetAsync(id);
                if (instr != null)
                {
                    if ((instr.UploadFileRusId == Guid.Empty || instr.UploadFileRusId == null) &&
                        (instr.UploadFileEngId == Guid.Empty || instr.UploadFileEngId == null))
                    {
                        await Database.Instructions.DeleteAsync(id);
                        var resD = await Database.Save();
                        if (resD > 0)
                        {
                            return new OperationDetails(true, "Успешно удалено / Delete Completed",
                                "DeleteInstructionId");
                        }

                        loggingService.Warn("Delete Instruction in database (save)");
                        return new OperationDetails(false, "Не удалено / Delete not Completed", "DeleteInstructionId");
                    }

                    instr.IsActive = false;
                    Database.Instructions.Update(instr);
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно / Completed (inactive)", "DeleteInstructionId");
                    }

                    loggingService.Warn("Inactive Instruction in database (save)");
                    return new OperationDetails(false, "Не изменено / Not Completed (inactive)", "DeleteInstructionId");
                }

                loggingService.Warn("Inactive InstructionId failed");
                return new OperationDetails(false, "Не изменено / Not Completed (inactive)", "DeleteInstructionId");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteInstructionId");
            }
        }

        public async Task<OperationDetails> UploadFileAsync(Guid instructionId, Guid uploadFileId, bool isRus)
        {
            try
            {
                if (instructionId == Guid.Empty)
                {
                    loggingService.Error("Invalid Id");
                    return new OperationDetails(false, "Неверный идентификатор / Invalid Id", string.Empty);
                }

                if (uploadFileId == Guid.Empty)
                {
                    loggingService.Error("Invalid FileId");
                    return new OperationDetails(false, "Неверный идентификатор файла / Invalid FileId", string.Empty);
                }

                var instr = await Database.Instructions.GetAsync(instructionId);
                if (instr == null)
                {
                    loggingService.Error("Instruction not found");
                    return new OperationDetails(false, "Инструкция не найдена / Instruction not found", string.Empty);
                }

                if (!instr.IsActive)
                {
                    loggingService.Error("Instruction is not active");
                    return new OperationDetails(false, "Инструкция не активна / Instruction is not active",
                        string.Empty);
                }

                var file = await Database.UploadedFiles.GetAsync(uploadFileId);
                if (file == null)
                {
                    loggingService.Error("Uploaded file not found");
                    return new OperationDetails(false, "Загруженный файл не найден / Uploaded file not found",
                        string.Empty);
                }

                if (string.IsNullOrEmpty(file.PathFile))
                {
                    loggingService.Error("Uploaded file not found path");
                    return new OperationDetails(false, "Загруженный файл не найден / Uploaded file not found",
                        string.Empty);
                }

                //Проверяем файл в шаре
                var shareBasePath = (await Database.Settings.GetNameAsync("ShareBasePath"))?.Value;
                if (string.IsNullOrEmpty(shareBasePath))
                {
                    loggingService.Warn("Database not Share File path");
                    return new OperationDetails(false, "Database not Share File path", string.Empty);
                }

                var pathFile = Path.Combine(shareBasePath, file.PathFile);
                if (!File.Exists(pathFile))
                {
                    loggingService.Warn($"Database not Share File path {pathFile}");
                    return new OperationDetails(false, "Загруженный файл не найден / Uploaded file not found",
                        string.Empty);
                }

                if (isRus)
                    instr.UploadFileRusId = file.UploadedFileId;
                else
                    instr.UploadFileEngId = file.UploadedFileId;

                Database.Instructions.Update(instr);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно загружено / Completed", "UploadInstruction");
                }
                else
                {
                    return new OperationDetails(false, "Ошибка при загрузке / Error upload", "UploadInstruction");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UploadInstruction");
            }
        }

        public async Task<OperationDetails> DeleteFileAsync(Guid instructionId, bool isRus)
        {
            try
            {
                if (instructionId == Guid.Empty)
                {
                    loggingService.Warn("Empty Id");
                    return new OperationDetails(false, "Неверный Id / Empty Id", string.Empty);
                }

                var instr = await Database.Instructions.GetAsync(instructionId);
                if (instr == null)
                {
                    loggingService.Warn("No Item in DataBase");
                    return new OperationDetails(false, "No Item in DataBase", string.Empty);
                }

                var uplFileId =
                    (isRus ? instr.UploadFileRusId : instr.UploadFileEngId) ?? Guid.Empty; // fileId : rus or eng
                if (uplFileId == Guid.Empty)
                {
                    loggingService.Error("UplFileId empty");
                    return new OperationDetails(false, "FileId empty", "DeleteFile");
                }

                //Delete File
                var file = await Database.UploadedFiles.GetAsync(uplFileId);
                if (file != null)
                {
                    //Если файл перемещен загружаем основуню директорию
                    var shareBasePath = (await Database.Settings.GetNameAsync("ShareBasePath"))?.Value;
                    if (string.IsNullOrWhiteSpace(shareBasePath))
                    {
                        loggingService.Warn("Database not Share File path");
                        return new OperationDetails(false, "Database not Share File path", string.Empty);
                    }

                    var filePath = Path.Combine(shareBasePath, file.PathFile);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    await Database.UploadedFiles.DeleteAsync(uplFileId);
                    if (isRus)
                        instr.UploadFileRusId = null;
                    else
                        instr.UploadFileEngId = null;
                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Файл успешно удален / Delete file completed", "DeleteFile");
                    }
                }

                loggingService.Warn("Error delete file");
                return new OperationDetails(false, "Файл не удален / Delete not completed", "DeleteFile");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteFile");
            }
        }

        public async Task<IEnumerable<InstructionDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.Instructions.GetIQueryable();
                if (filterModels != null && filterModels.Any())
                {
                    foreach (var filter in filterModels)
                    {
                        if (filter != null)
                        {
                            if (filter.Field.StartsWith("IsActive", StringComparison.OrdinalIgnoreCase) &&
                                !string.IsNullOrWhiteSpace(filter.Value))
                            {
                                if (bool.TryParse(filter.Value, out var val))
                                {
                                    inquiry = inquiry.Where(x => x.IsActive == val);
                                }
                            }
                            else if (filter.Field.StartsWith("IsNoAdmin", StringComparison.OrdinalIgnoreCase) &&
                                     !string.IsNullOrWhiteSpace(filter.Value))
                            {
                                if (bool.TryParse(filter.Value, out var val))
                                {
                                    inquiry = inquiry.Where(x => x.IsAdmin != val);
                                }
                            }
                            else if (filter.Field.StartsWith("Searchtxt", StringComparison.OrdinalIgnoreCase) &&
                                     !string.IsNullOrWhiteSpace(filter.Value))
                            {
                                inquiry = inquiry.Where(x => x.NameRus.ToLower().Contains(filter.Value.ToLower().Trim())
                                                             || x.NameEng.ToLower()
                                                                 .Contains(filter.Value.ToLower().Trim()));
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                                     !string.IsNullOrWhiteSpace(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                    inquiry = inquiry.Where(x => x.InstructionId == idGuid);
                                }
                            }
                        }
                    }
                }

                var fullData = await inquiry.Take(50).AsNoTracking().ToListAsync();
                var config = new MapperConfiguration(c => { c.CreateMap<Instruction, InstructionDTO>().MaxDepth(1); });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();
                var results = mapper.Map<IEnumerable<Instruction>, List<InstructionDTO>>(fullData);
                return results;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<InstructionDTO>();
            }
        }

        public async Task<IEnumerable<InstructionDTO>> GetAllAsync()
        {
            try
            {
                var fullData = await Database.Instructions.GetAllAsync();
                var config = new MapperConfiguration(c => { c.CreateMap<Instruction, InstructionDTO>().MaxDepth(1); });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();
                var result = mapper.Map<IEnumerable<Instruction>, List<InstructionDTO>>(fullData);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<InstructionDTO>();
            }
        }

        public async Task<InstructionDTO> GetByCodeAsync(int instrCode)
        {
            try
            {
                if (instrCode < 1)
                {
                    loggingService.Error("InstructionCode invalid");
                    return new InstructionDTO();
                }

                var instr = (await Database.Instructions.FindAsync(c => c.Code == instrCode)).FirstOrDefault();
                if (instr != null)
                {
                    var config = new MapperConfiguration(c => { c.CreateMap<Instruction, InstructionDTO>(); });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var result = mapper.Map<Instruction, InstructionDTO>(instr);
                    return result;
                }

                return new InstructionDTO();
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new InstructionDTO();
            }
        }

        public async Task<InstructionDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("InstructionId = Empty");
                    return new InstructionDTO();
                }

                var instr = await Database.Instructions.GetAsync(id);
                if (instr != null)
                {
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<Instruction, InstructionDTO>().MaxDepth(1);
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var result = mapper.Map<Instruction, InstructionDTO>(instr);
                    return result;
                }

                return new InstructionDTO();
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new InstructionDTO();
            }
        }
    }
}
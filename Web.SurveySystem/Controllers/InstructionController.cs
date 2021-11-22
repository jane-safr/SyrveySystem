using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Microsoft.AspNet.Identity;
using Web.SurveySystem.Helpers;
using Web.SurveySystem.Models.ViewModels;

namespace Web.SurveySystem.Controllers
{
    [Authorize]
    [RoutePrefix("instructions")]
    public class InstructionController : Controller
    {
        private readonly IInstructionService instructionService;
        private readonly ISettingService settingService;
        private readonly IUploadedFileService fileService;
        public ILoggerService<InstructionController> loggingService;
        public InstructionController(IInstructionService instrService, IUploadedFileService fService, ISettingService setService, ILoggerService<InstructionController> loggingService)
        {
            this.instructionService = instrService;
            this.settingService = setService;
            this.loggingService = loggingService;
            this.fileService = fService;
        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("show")]
        public async Task<ActionResult> Show()
        {
            //CurrentUser
            if (!Guid.TryParse(User.Identity.GetUserId(), out var userId))
            {
                loggingService.Error($"{User.Identity.Name} code empty");
                return RedirectToAction("Forbidden", "Error");
            }
            var instructions = (await instructionService.GetAllAsync()).Where(i => i.IsActive).ToList();
            if (!instructions.Any())
            {
                loggingService.Error("Instructions not found");
                return RedirectToAction("NotFound", "Error");
            }
            var config = new MapperConfiguration(c => { c.CreateMap<InstructionDTO, InstructionVM>(); });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var instructionVm = mapper.Map<IEnumerable<InstructionDTO>, List<InstructionVM>>(instructions);
            return View("Show", instructionVm);
        }

        [HttpPost]
        [Authorize]
        [Route("all")]
        public async Task<JsonNetResult> GetAll()
        {
            try
            {
                var filter = new List<FilterModels>();
                if (User.IsInRole("user"))
                {
                    filter.Add(new FilterModels
                    {
                        Field = "IsActive",
                        Value = "true"
                    });
                    filter.Add(new FilterModels
                    {
                        Field = "IsNoAdmin",
                        Value = "true"
                    });
                }
                var instructions = await instructionService.FindByFilterAsync(filter);
                var config = new MapperConfiguration(c => { c.CreateMap<InstructionDTO, InstructionVM>(); });
                config.AssertConfigurationIsValid();
                var mapper = config.CreateMapper();
                var instructionVm = mapper.Map<IEnumerable<InstructionDTO>, List<InstructionVM>>(instructions);
                return new JsonNetResult(new { success = true, data = instructionVm });
            }
            catch (Exception e)
            {
                var user = User.Identity.GetUserName();
                loggingService.Error($"GetAllInstructions: User: {user} - {e.Message}");
                return JsonNetResult.Warn(e.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("fileupload")]
        public async Task<JsonResult> SaveFile(IEnumerable<HttpPostedFileBase> files, Guid instructionId, bool isRus)
        {
            try
            {
                var user = User.Identity.GetUserName();
                if (!Guid.TryParse(User.Identity.GetUserId(), out var userId))
                {
                    loggingService.Error("UserId - invalid");
                    return Json(new { success = false, errorText = "Неверный id пользователя / Invalid User Id" }, "text/plain");
                }

                if (files == null)
                {
                    return Json(new { success = false, errorText = "Выберите файл / Select file" }, "text/plain");
                }
                if (instructionId == Guid.Empty)
                {
                    return Json(new { success = false, errorText = "Инструкция не найдена / Empty instructionId" }, "text/plain");
                }
                //Загружаем макс. размер файла.
                var setting = (await settingService.GetSettings()).ToList();
                if (!setting.Any())
                {
                    loggingService.Error("No settings");
                    return Json(new { success = false, errorText = "Не найдены настройки / Settings not found" }, "text/plain");
                }
                var fileSizeStr = setting.FirstOrDefault(s => s.Name == "MaxFileSize")?.Value;
                var shareBasePath = setting.FirstOrDefault(s => s.Name == "ShareBasePath")?.Value;
                var maxLengthFileName = setting.FirstOrDefault(s => s.Name == "MaxLengthFileName")?.Value;
                if (!string.IsNullOrEmpty(fileSizeStr) && !string.IsNullOrEmpty(shareBasePath) && !string.IsNullOrEmpty(maxLengthFileName))
                {
                    var fileSize = int.Parse(fileSizeStr);
                    foreach (var file in files.Take(1))
                    {
                        //Проверяем размер файла
                        if (file.ContentLength > 0 && file.ContentLength <= fileSize)
                        {
                            var supportedTypes = "pdf,doc,docx".Split(new[] { "," }, StringSplitOptions.None);
                            var fileExt = Path.GetExtension(file.FileName)?.Substring(1).ToLower();
                            if (!supportedTypes.Contains(fileExt))
                            {
                                loggingService.Error("Invalid file format");
                                return Json(new { success = false, errorText = "Неверный формат файла / Invalid file format" }, "text/plain");
                            }
                            //Проверяем имя файла
                            if (string.IsNullOrEmpty(file.FileName))
                            {
                                loggingService.Error("Invalid file name");
                                return Json(new { success = false, errorText = "Неверное имя файла / Invalid file name" }, "text/plain");
                            }

                            if (file.FileName.Length > int.Parse(maxLengthFileName))
                            {
                                loggingService.Error($"File name exceeds {maxLengthFileName} Characters");
                                return Json(new { success = false, errorText = $"Наименование файла превышает {maxLengthFileName} символов / File name exceeds {maxLengthFileName} characters" }, "text/plain");
                            }

                            var fileName = string.Concat(Path.GetFileNameWithoutExtension(file.FileName), "__%%", DateTime.Now.ToString("yyyyMMddHHmmssfff"), Path.GetExtension(file.FileName));
                            if (string.IsNullOrEmpty(fileName))
                            {
                                return Json(new { success = false, errorText = "Неверное имя файла / Invalid file name" }, "text/plain");
                            }
                            var pathDirectory = Path.Combine(shareBasePath, "Instructions", user);
                            if (!Directory.Exists(pathDirectory))
                            {
                                Directory.CreateDirectory(pathDirectory);
                            }
                            var physicalPath = Path.Combine(pathDirectory, fileName);
                            if (System.IO.File.Exists(physicalPath))
                            {
                                System.IO.File.Delete(physicalPath);
                            }
                            //saveFile
                            file.SaveAs(physicalPath);
                            if (System.IO.File.Exists(physicalPath))
                            {
                                var saveFileModel = new UploadedFileDTO
                                {
                                    UploadedFileId = Guid.NewGuid(),
                                    CreatedOn = DateTime.Now,
                                    CreatedBy = user,
                                    PathFile = Path.Combine("Instructions", user, fileName),
                                    FileName = file.FileName,
                                    FileSize = file.ContentLength,
                                    ApplicationUserId = userId,
                                    IsActive = true,
                                    //Тип файла 1-документы, 2 - инструкции Rus, 3 - инструкции Eng
                                    FileType = isRus ? 2 : 3
                                };
                                var up = await fileService.CreateAsync(saveFileModel);
                                if (up.Succedeed)
                                {
                                    var updateRequest = await instructionService.UploadFileAsync(instructionId, saveFileModel.UploadedFileId, isRus);
                                    return Json(new { success = updateRequest.Succedeed, errorText = updateRequest.Message }, "text/plain");
                                }
                            }
                        }
                    }
                }
                return Json(new { success = false, errorText = "Файл не был загружен / File was not uploaded" }, "text/plain");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorText = $"Error:{ex.Message}" }, "text/plain");
            }
        }

        [HttpPost]
        [Authorize]
        [Route("fileremove")]
        public async Task<JsonNetResult> DeleteFile(Guid instructionId, bool isRus)
        {
            try
            {
                if (instructionId == Guid.Empty)
                {
                    return JsonNetResult.Warn("empty Id");
                }
                var user = User.Identity.GetUserName();
                loggingService.Warn($"{user} Delete -  DeleteFile {instructionId}");
                var deleted = await instructionService.DeleteFileAsync(instructionId, isRus);
                return new JsonNetResult(new { success = deleted.Succedeed, message = deleted.Message });
            }
            catch (Exception ex)
            {
                loggingService.Error(ex);
                return JsonNetResult.Failure(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("download/{instructionId}/{rus?}", Name = "download")]
        public async Task<ActionResult> DownloadFiles(Guid instructionId, bool rus = false) //rus
        {
            var user = User.Identity.GetUserName();
            if (instructionId == Guid.Empty)
            {
                loggingService.Error($"{user} instructionId Empty");
                return new HttpNotFoundResult();
            }
            var instr = await instructionService.GetByIdAsync(instructionId);
            if (instr == null)
            {
                loggingService.Error($"{user} No document");
                return new HttpNotFoundResult();
            }
            Guid upFileId;
            //rus
            if (rus)
            {
                if (instr.UploadFileRusId == null || instr.UploadFileRusId == Guid.Empty)
                {
                    loggingService.Error($"{user} No document RUS");
                    return new HttpNotFoundResult();
                }
                upFileId = (Guid)instr.UploadFileRusId;
            }
            else
            {
                if (instr.UploadFileEngId == null || instr.UploadFileEngId == Guid.Empty)
                {
                    loggingService.Error($"{user} No document RUS");
                    return new HttpNotFoundResult();
                }
                upFileId = (Guid)instr.UploadFileEngId;
            }

            if (upFileId == Guid.Empty)
            {
                loggingService.Error($"{user} upFileId empty");
                return new HttpNotFoundResult();
            }
            var uploadFile = await fileService.GetByIdAsync(upFileId);
            if (uploadFile == null)
            {
                loggingService.Error($"{user} No document");
                return new HttpNotFoundResult();
            }
            var res = await DownloadFile(uploadFile);
            return res;
        }

        private async Task<ActionResult> DownloadFile(UploadedFileDTO uploadFile)
        {
            try
            {
                if (uploadFile == null)
                {
                    loggingService.Error("DownloadFile Instruction uploadFile null");
                    return new HttpNotFoundResult();
                }

                var shareBasePath = (await settingService.GetSettingName("ShareBasePath"))?.Value;
                if (!string.IsNullOrEmpty(shareBasePath) && !string.IsNullOrEmpty(uploadFile.PathFile))
                {
                    var file = uploadFile.PathFile;
                    var fullPath = Path.Combine(shareBasePath, file);
                    if (!System.IO.File.Exists(fullPath))
                    {
                        loggingService.Error($"File {fullPath} not exists");
                        return new HttpNotFoundResult();
                    }
                    string dtype;
                    var docType = Path.GetExtension(fullPath).Substring(1).ToLower();
                    switch (docType)
                    {
                        case "docx":
                            dtype = "application/vnd.ms-word.document";
                            break;
                        case "doc":
                            dtype = "application/ms-word";
                            break;
                        case "pdf":
                            dtype = "application/pdf";
                            break;
                        default:
                            dtype = "application/force-download";
                            break;
                    }
                    var bytes = System.IO.File.ReadAllBytes(fullPath);
                    if (bytes.Length > 0)
                    {
                        return File(bytes, dtype, uploadFile.FileName);
                    }
                    return new HttpNotFoundResult();
                }
                return new HttpNotFoundResult();
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message);
                return new HttpNotFoundResult();
            }
        }

        [HttpPost]
        [Authorize]
        [Route("add")]
        public async Task<JsonNetResult> Create(InstructionVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = User.Identity.GetUserName();
                    var userId = User.Identity.GetUserId();
                    var config = new MapperConfiguration(c =>
                    {
                        c.CreateMap<InstructionVM, InstructionDTO>()
                            .ForMember(x => x.InstructionId, m => m.MapFrom(s => Guid.NewGuid()))
                            .ForMember(x => x.CreatedOn, x => x.MapFrom(m => DateTime.Now))
                            .ForMember(x => x.CreatedBy, x => x.MapFrom(m => user))
                            .ForMember(x => x.UploadFileEngId, map => map.Ignore())
                            .ForMember(x => x.UploadFileRusId, map => map.Ignore());
                    });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<InstructionVM, InstructionDTO>(model);
                    var update = await instructionService.CreateAsync(modelDto);
                    return new JsonNetResult(new { success = update.Succedeed, message = update.Message, pId = model.InstructionId });
                }
                else
                {
                    var listErrors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    loggingService.Error(listErrors);
                    return JsonNetResult.Warn(listErrors);
                }
            }
            catch (Exception e)
            {
                loggingService.Error($"Create Failed: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("edit")]
        public async Task<JsonNetResult> Update(InstructionVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var config = new MapperConfiguration(c => { c.CreateMap<InstructionVM, InstructionDTO>(); });
                    config.AssertConfigurationIsValid();
                    var mapper = config.CreateMapper();
                    var modelDto = mapper.Map<InstructionVM, InstructionDTO>(model);
                    var update = await instructionService.UpdateAsync(modelDto);
                    return new JsonNetResult(new { success = update.Succedeed, message = update.Message });
                }
                else
                {
                    var listErrors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    loggingService.Error(listErrors);
                    return JsonNetResult.Warn(listErrors);
                }
            }
            catch (Exception e)
            {
                loggingService.Error($"Update failed: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("remove")]
        public async Task<JsonNetResult> Delete(Guid instructionId)
        {
            try
            {
                if (instructionId == Guid.Empty)
                {
                    return JsonNetResult.Warn("Empty instructionId");
                }
                if (this.User.IsInRole("admin") || this.User.IsInRole("manager"))
                {
                    var user = User.Identity.GetUserName();
                    loggingService.Warn($"{user} Delete - instr {instructionId}");
                    var deleted = await instructionService.DeleteAsync(instructionId);
                    return new JsonNetResult(new { success = deleted.Succedeed, message = deleted.Message });
                }
                return JsonNetResult.Failure("Доступ запрещен / Access denied");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex);
                return JsonNetResult.Failure(ex.Message);
            }
        }
    }
}
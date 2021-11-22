using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BLL.SurveySystem.DTO;
using BLL.SurveySystem.Helpers;
using BLL.SurveySystem.Infrastructure;
using BLL.SurveySystem.Interfaces;
using Domain.SurveySystem.Entity;
using Domain.SurveySystem.Interfaces;

namespace BLL.SurveySystem.Services
{
    public class SurveyService : ISurveyService
    {
        private IUnitOfWork Database { get; set; }
        private readonly ILoggerService<SurveyService> loggingService;
        public SurveyService(IUnitOfWork uow, ILoggerService<SurveyService> logServ)
        {
            this.Database = uow;
            this.loggingService = logServ;
        }
        public async Task<OperationDetails> CreateAsync(SurveyDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("SurveyDTO Empty");
                    return new OperationDetails(false, "Тест отсутствует / Survey is Empty", string.Empty);
                }
                if (model.SurveyId == Guid.Empty || string.IsNullOrEmpty(model.NameRus) || string.IsNullOrEmpty(model.NameEng) || string.IsNullOrEmpty(model.CreatedBy))
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                if (model.SurveyTypeId == Guid.Empty)
                {
                    loggingService.Error("SurveyType not filled in");
                    return new OperationDetails(false, "Тип не заполнен / Survey Type is not filled in", string.Empty);
                }
                // get type
                var type = await Database.SurveyTypes.GetAsync(model.SurveyTypeId);
                if (type == null)
                {
                    loggingService.Error("surveytype not found");
                    return new OperationDetails(false, "Тип не найден / Survey Type not found", string.Empty);
                }
                if (string.IsNullOrEmpty(model.PurposeEng) || string.IsNullOrEmpty(model.PurposeRus))
                {
                    loggingService.Error("Purposes are empty");
                    return new OperationDetails(false, "Цель не заполнена / Purpose not filled in", string.Empty);
                }
                if (model.SurveyCode < 1)
                {
                    loggingService.Error("SurveyCode invalid");
                    return new OperationDetails(false, "Код недействителен / Survey Code is invalid", string.Empty);
                }
                if (model.CreatedOn == DateTime.MaxValue || model.CreatedOn == DateTime.MinValue)
                {
                    loggingService.Error("Creation date value is not valid");
                    return new OperationDetails(false, "Значение даты создания недействительно / Creation date value is not valid", string.Empty);
                }
                if (model.TimeEstimateMin < 1)
                {
                    loggingService.Error("TimeEstimateMin invalid");
                    return new OperationDetails(false, "Время прохождения недействительно / Estimate Time is invalid", string.Empty);
                }

                var duplicateId = await Database.Surveys.GetAsync(model.SurveyId);
                if (duplicateId != null)
                {
                    loggingService.Error("Duplicate By SurveyId");
                    return new OperationDetails(false, "Дубликат / Duplicate By Id ", "Duplicate");
                }
                // dupl by name eng rus
                var duplSurvey = await Database.Surveys.FindAsync(x => x.NameRus == model.NameRus || x.NameEng == model.NameEng);
                if (duplSurvey.Any())
                {
                    loggingService.Error("Duplicate By Name");
                    return new OperationDetails(false, "Дубликат по имени / Duplicate by name", "Duplicate");
                }

                if (model.IsAnonymous) // если анон - создаем короткую ссылку
                {
                    //настройка путь к сайту
                    var setUrl = await Database.Settings.GetNameAsync("BaseUrl");
                    if (setUrl == null)
                    {
                        loggingService.Error("BaseUrl empty");
                        return new OperationDetails(false, "Отсутствует BaseUrl / BaseUrl is Empty", string.Empty);
                    }
                    if (string.IsNullOrEmpty(setUrl.Value))
                    {
                        loggingService.Error("BaseUrl Value empty");
                        return new OperationDetails(false, "Отсутствует BaseUrl / BaseUrl is Empty", string.Empty);
                    }
                   model.ShortLink = string.Concat(setUrl.Value, "survey/start/", model.SurveyCode.ToString());
                }
                //SaveModel
                var saveModel = new Survey
                {
                    SurveyId = model.SurveyId,
                    SurveyCode = model.SurveyCode,
                    CreatedOn = model.CreatedOn,
                    CreatedBy = model.CreatedBy,
                    IsActive = model.IsActive,
                    NameRus = model.NameRus.Trim(),
                    NameEng = model.NameEng.Trim(),
                    PurposeRus = model.PurposeRus.Trim(),
                    PurposeEng = model.PurposeEng.Trim(),
                    SurveyTypeId = model.SurveyTypeId,
                    TimeEstimateMin = model.TimeEstimateMin,
                    IsRandomQuestions = model.IsRandomQuestions,
                    ShortLink = string.IsNullOrEmpty(model.ShortLink) ?  null : model.ShortLink.Trim(),
                    IsAnonymous = model.IsAnonymous
                };
                Database.Surveys.Create(saveModel);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно добавлено / Completed", "CreateSurvey");
                }
                else
                {
                    return new OperationDetails(false, "Ошибка при добавлении / Not Completed", "CreateSurvey");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "CreateSurvey");
            }
        }

        public async Task<SurveyDTO> GetByCodeAsync(int codeId)
        {
            try
            {
                if (codeId < 1)
                {
                    loggingService.Error("codeId invalid");
                    return null;
                }
                var survey = (await Database.Surveys.FindAsync(b => b.SurveyCode == codeId)).FirstOrDefault();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<Survey, SurveyDTO>(survey);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new SurveyDTO();
            }
        }

        public async Task<OperationDetails> GenerateLinkAsync(int codeId)
        {
            if (codeId < 1)
            {
                loggingService.Error("codeId invalid");
                return new OperationDetails(false, "Тест отсутствует / Survey is Empty", string.Empty);
            }

            //проверяем тест
            var survey = (await Database.Surveys.FindAsync(x => x.SurveyCode == codeId)).FirstOrDefault();
            if (survey == null)
            {
                loggingService.Error("Survey is Empty");
                return new OperationDetails(false, "Тест отсутствует / Survey is Empty", string.Empty);
            }
            if (!survey.IsActive)
            {
                loggingService.Error("Survey is not active");
                return new OperationDetails(false, "Тест неактивен / Survey is not active", string.Empty);
            }

            if (!survey.IsAnonymous)
            {
                loggingService.Error("Short link is available for Anonymous tests only");
                return new OperationDetails(false, "Короткая ссылка доступна только для анонимных тестов / Short link is available for Anonymous survey only", string.Empty);
            }
            //TODO


            //настройка путь к сайту
            var setUrl = await Database.Settings.GetNameAsync("BaseUrl");
            if (setUrl == null)
            {
                loggingService.Error("BaseUrl empty");
                return new OperationDetails(false, "Отсутствует BaseUrl / BaseUrl is Empty", string.Empty);
            }
            if (string.IsNullOrEmpty(setUrl.Value))
            {
                loggingService.Error("BaseUrl Value empty");
                return new OperationDetails(false, "Отсутствует BaseUrl / BaseUrl is Empty", string.Empty);
            }

            return new OperationDetails(false, "Отсутствует BaseUrl / BaseUrl is Empty", string.Empty);

        }

        public async Task<IEnumerable<SurveyDTO>> GetAllAsync()
        {
            try
            {
                var result = await Database.Surveys.GetAllAsync();
                var mapper = MapperAll.MapperConfigDto();
                var surveys = mapper.Map<IEnumerable<Survey>, List<SurveyDTO>>(result);
                return surveys;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<SurveyDTO>();
            }
        }
        public async Task<IEnumerable<SurveyDTO>> FindByFilterAsync(List<FilterModels> filterModels)
        {
            try
            {
                var inquiry = Database.Surveys.GetIQueryable();
                if (filterModels != null && filterModels.Any())
                {
                    foreach (var filter in filterModels)
                    {
                        if (filter != null)
                        {
                            if (filter.Field.StartsWith("IsActive", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (bool.TryParse(filter.Value, out var val))
                                {
                                    inquiry = inquiry.Where(x => x.IsActive == val);
                                }
                            }
                            else if (filter.Field.StartsWith("searchtxt", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (int.TryParse(filter.Value, out var code))
                                {
                                    inquiry = inquiry.Where(x => x.SurveyCode == code);
                                }
                                else
                                {
                                    inquiry = inquiry.Where(x =>
                                        x.NameRus.Contains(filter.Value.Trim()) ||
                                        x.NameEng.Contains(filter.Value.Trim()) || x.PurposeRus.Contains(filter.Value.Trim()) || x.PurposeEng.Contains(filter.Value.Trim()));
                                }
                                
                            }
                            else if (filter.Field.StartsWith("Id", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(filter.Value))
                            {
                                if (Guid.TryParse(filter.Value, out var idGuid))
                                {
                                     inquiry = inquiry.Where(x => x.SurveyId == idGuid);
                                }
                            }
                        }
                    }
                }
                var fullData = await inquiry.Take(100).AsNoTracking().ToListAsync();
                var mapper = MapperAll.MapperConfigDto();
                var result = mapper.Map<IEnumerable<Survey>, List<SurveyDTO>>(fullData);
                return result;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new List<SurveyDTO>();
            }
        }

        public async Task<SurveyDTO> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Error("no search parameter");
                    return new SurveyDTO();
                }
                var dataFirst = await Database.Surveys.GetAsync(id);
                var mapper = MapperAll.MapperConfigDto();
                var survey = mapper.Map<Survey, SurveyDTO>(dataFirst);
                return survey;
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new SurveyDTO();
            }
        }

        public async Task<OperationDetails> UpdateAsync(SurveyDTO model)
        {
            try
            {
                if (model == null)
                {
                    loggingService.Error("SurveyDTO Empty");
                    return new OperationDetails(false, "Тест отсутствует / Survey is Empty", string.Empty);
                }
                if (model.SurveyId == Guid.Empty || string.IsNullOrEmpty(model.NameRus) || string.IsNullOrEmpty(model.NameEng) )
                {
                    loggingService.Error("Not all required fields are filled in");
                    return new OperationDetails(false, "Заполнены не все обязательные поля / Not all required fields are filled in", string.Empty);
                }
                if (model.SurveyTypeId == Guid.Empty)
                {
                    loggingService.Error("SurveyType not filled in");
                    return new OperationDetails(false, "Тип не заполнен / Survey Type is not filled in", string.Empty);
                }
                // get type
                var type = await Database.SurveyTypes.GetAsync(model.SurveyTypeId);
                if (type == null)
                {
                    loggingService.Error("surveytype not found");
                    return new OperationDetails(false, "Тип не найден / Survey Type not found", string.Empty);
                }

                if (string.IsNullOrEmpty(model.PurposeEng) || string.IsNullOrEmpty(model.PurposeRus))
                {
                    loggingService.Error("Purposes are empty");
                    return new OperationDetails(false, "Цель не заполнена / Purpose not filled in", string.Empty);
                }
                if (model.SurveyCode < 1)
                {
                    loggingService.Error("SurveyCode invalid");
                    return new OperationDetails(false, "Код недействителен / Survey Code is invalid", string.Empty);
                }
                if (model.TimeEstimateMin < 1)
                {
                    loggingService.Error("TimeEstimateMin invalid");
                    return new OperationDetails(false, "Время прохождения недействительно / Estimate Time is invalid", string.Empty);
                }
                var survey = await Database.Surveys.GetAsync(model.SurveyId);
                if (survey == null)
                {
                    loggingService.Error("survey not found");
                    return new OperationDetails(false, "Тест не найден / Survey not found", "Duplicate");
                }
                // dupl by Name
                var dupSurvey = await Database.Surveys.FindAsync(x => (x.NameRus == model.NameRus || x.NameEng == model.NameEng) && x.SurveyId !=model.SurveyId);
                if (dupSurvey.Any())
                {
                    loggingService.Error("Duplicate By Name");
                    return new OperationDetails(false, "Дубликат по имени / Duplicate By name", "Duplicate");
                }

                if (model.IsAnonymous) // если анон - создаем короткую ссылку
                {
                    //настройка путь к сайту
                    var setUrl = await Database.Settings.GetNameAsync("BaseUrl");
                    if (setUrl == null)
                    {
                        loggingService.Error("BaseUrl empty");
                        return new OperationDetails(false, "Отсутствует BaseUrl / BaseUrl is Empty", string.Empty);
                    }
                    if (string.IsNullOrEmpty(setUrl.Value))
                    {
                        loggingService.Error("BaseUrl Value empty");
                        return new OperationDetails(false, "Отсутствует BaseUrl / BaseUrl is Empty", string.Empty);
                    }
                    model.ShortLink = string.Concat(setUrl.Value, "survey/start/", model.SurveyCode.ToString());
                }

                loggingService.Warn($"Update: NameRus:{survey.NameRus} -> New:{model.NameRus}");
                loggingService.Warn($"Update: NameEng:{survey.NameEng} -> New:{model.NameEng}");
                loggingService.Warn($"Update: IsActive:{survey.IsActive} -> New:{model.IsActive}");
                //Update
                survey.NameRus = model.NameRus.Trim();
                survey.NameEng = model.NameEng.Trim();
                survey.PurposeRus = model.PurposeRus.Trim();
                survey.PurposeEng = model.PurposeEng.Trim();
                survey.TimeEstimateMin = model.TimeEstimateMin;
                survey.IsActive = model.IsActive;
                survey.IsRandomQuestions = model.IsRandomQuestions;
                survey.IsAnonymous = model.IsAnonymous;
                survey.SurveyTypeId = model.SurveyTypeId;
                survey.ShortLink = string.IsNullOrEmpty(model.ShortLink) ? null : model.ShortLink.Trim();

                Database.Surveys.Update(survey);
                var res = await Database.Save();
                if (res > 0)
                {
                    return new OperationDetails(true, "Успешно обновлено / Completed", "UpdateSurvey");
                }
                else
                {
                    loggingService.Warn("UpdateSurvey Error db");
                    return new OperationDetails(false, "Не обновлено / Not Completed", "UpdateSurvey");
                }
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "UpdateSurvey");
            }
        }
        public async Task<OperationDetails> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    loggingService.Warn("Delete - Empty SurveyId");
                    return new OperationDetails(false, "Неверный Id / Empty Id", "DeleteSurvey");
                }
                var result = await Database.Surveys.GetAsync(id);
                if (result != null)
                {
                    // если кто-то проходил тест - сделать неактивным
                      var invitations = (await Database.Invitations.FindAsync(d => d.SurveyId == id));
                      if (invitations.Any())
                      {
                          if (invitations.Any(x => x.IsAccepted || x.IsFinished))
                          {
                              loggingService.Warn($"cannot delete survey -> inactive {id}");
                              result.IsActive = false;
                              Database.Surveys.Update(result);
                          }
                          else // если приглашения не начаты - удалить: + приглашения + вопросы
                          await Database.Surveys.DeleteAsync(id);
                      }
                    else
                    await Database.Surveys.DeleteAsync(id); // удалить если нет приглашений

                    var res = await Database.Save();
                    if (res > 0)
                    {
                        return new OperationDetails(true, "Успешно удалено / Delete completed", "DeleteSurvey");
                    }
                    else
                    {
                        loggingService.Warn("DeleteSurvey in database none Item");
                        return new OperationDetails(false, "Не удалено / Not Completed", "DeleteSurvey");
                    }
                }
                loggingService.Warn("DeleteSurvey in database none Item");
                return new OperationDetails(false, "Не удалено / Not completed", "DeleteSurvey");
            }
            catch (Exception ex)
            {
                loggingService.Error(ex.Message + " " + ex.StackTrace);
                return new OperationDetails(false, $"{ex.Message}  {ex.StackTrace}", "DeleteSurvey");
            }
        }
    }
}

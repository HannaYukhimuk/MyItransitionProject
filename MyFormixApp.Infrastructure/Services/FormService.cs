using AutoMapper;
using MyFormixApp.Domain.Entities;
using MyFormixApp.Application.Models;
using MyFormixApp.Domain.DTOs.Forms;
using MyFormixApp.Application.Repositories;
using MyFormixApp.Domain.DTOs.Templates;
using MyFormixApp.Domain.DTOs.Answers;
using Microsoft.AspNetCore.Http;
using MyFormixApp.Application.Services;
using MyFormixApp.Application.Results.Forms;

namespace MyFormixApp.Infrastructure.Services
{
    public class FormService : IFormService
    {
        private readonly IFormRepository _formRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IMapper _mapper;

        public FormService(
            IFormRepository formRepository,
            ITemplateRepository templateRepository,
            IMapper mapper)
        {
            _formRepository = formRepository;
            _templateRepository = templateRepository;
            _mapper = mapper;
        }

        public async Task<FormDetailsDto?> GetByIdAsync(Guid id, Guid currentUserId) => 
            await GetFormDetails(id, currentUserId);

        public async Task<IEnumerable<FormDetailsDto>> GetByUserAsync(Guid userId) => 
            await MapForms(await _formRepository.GetByUserAsync(userId));

        public async Task<FormDetailsDto> CreateAsync(FormDto dto, Guid userId)
        {
            await ValidateFormCreation(dto, userId);
            var form = CreateFormEntity(dto, userId);
            return await SaveAndReturnForm(form, userId);
        }

        public async Task<bool> UpdateAsync(FormDto dto, Guid userId, bool isAdmin)
        {
            var form = await GetFormForUpdate(dto.Id!.Value, userId, isAdmin);
            if (form == null) return false;
            
            UpdateFormAnswers(form, dto.Answers);
            await _formRepository.UpdateAsync(form);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            if (!await CanDeleteForm(id, userId)) return false;
            await _formRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> CreateResponseAsync(Guid templateId, Guid userId, Dictionary<Guid, string> responses)
        {
            var template = await ValidateTemplateAccess(templateId, userId);
            ValidateRequiredQuestions(template, responses.Keys);
            await SaveFormResponse(templateId, userId, responses);
            return true;
        }

        public async Task<IEnumerable<FormDetailsDto>> GetAllFormsAsync() => 
            await MapForms(await _formRepository.GetAllWithDetailsAsync());

        public async Task<IEnumerable<FormDetailsDto>> GetByTemplateAsync(Guid templateId, Guid currentUserId)
        {
            await ValidateTemplateAccess(templateId, currentUserId);
            return await MapForms(await _formRepository.GetByTemplateAsync(templateId));
        }

        public async Task<TemplateStatisticsDto> GetTemplateStatisticsAsync(Guid templateId, Guid currentUserId)
        {
            var template = await ValidateTemplateAccess(templateId, currentUserId);
            return await CalculateStatistics(template);
        }

        public async Task<FormDetailsDto?> GetByUserAndTemplateAsync(Guid userId, Guid templateId) => 
            await GetUserTemplateForm(userId, templateId);

        public async Task<IEnumerable<FormDetailsDto>> GetUserFormsAsync(Guid userId) => 
            await MapForms(await _formRepository.GetByUserAsync(userId));

        public async Task<FormResult> ProcessFormResponseAsync(Guid templateId, Guid userId, IFormCollection formCollection)
        {
            if (await HasExistingResponse(userId, templateId))
                return FormAlreadySubmittedResult();
            
            try
            {
                var responses = ParseFormResponses(formCollection);
                await CreateResponseAsync(templateId, userId, responses);
                return SuccessFormResult();
            }
            catch (Exception ex)
            {
                return ErrorFormResult(ex);
            }
        }

        public async Task<ServiceResult<TemplateFormsView>> GetTemplateFormsAsync(Guid templateId, Guid userId)
        {
            try
            {
                var template = await ValidateTemplateAccess(templateId, userId);
                var forms = await MapForms(await _formRepository.GetByTemplateAsync(template.Id));
                return new ServiceResult<TemplateFormsView>(new TemplateFormsView
                {
                    Forms = forms,
                    TemplateTitle = template.Title,
                    TemplateId = template.Id
                });
            }
            catch (Exception ex)
            {
                return new ServiceResult<TemplateFormsView>(ex.Message);
            }
        }

        public async Task<ServiceResult<FormDetailsDto>> GetFormDetailsAsync(Guid id, Guid userId)
        {
            try
            {
                var form = await GetFormOrThrow(id, userId);
                return SuccessFormDetailsResult(form);
            }
            catch (Exception ex)
            {
                return ErrorFormDetailsResult(ex);
            }
        }

        public async Task<FormOperationResult> UpdateFormAsync(FormDto dto, Guid userId, bool isAdmin)
        {
            try
            {
                ValidateFormDto(dto);
                var success = await UpdateAsync(dto, userId, isAdmin);
                return CreateUpdateResult(success, isAdmin);
            }
            catch (Exception ex)
            {
                return ErrorUpdateResult(ex);
            }
        }

        public async Task<OperationResult> DeleteFormAsync(Guid id, Guid userId)
        {
            try
            {
                var success = await DeleteAsync(id, userId);
                return CreateDeleteResult(success);
            }
            catch (Exception ex)
            {
                return ErrorDeleteResult(ex);
            }
        }

        private async Task<FormDetailsDto?> GetFormDetails(Guid id, Guid currentUserId)
        {
            var form = await _formRepository.GetByIdWithDetailsAsync(id);
            return form == null ? null : MapToFormDetailsDto(form);
        }

        private async Task<IEnumerable<FormDetailsDto>> MapForms(IEnumerable<Form> forms) => 
            _mapper.Map<IEnumerable<FormDetailsDto>>(forms);

        private async Task ValidateFormCreation(FormDto dto, Guid userId)
        {
            if (await _formRepository.GetByUserAndTemplateAsync(userId, dto.TemplateId) != null)
                throw new InvalidOperationException("Duplicate form submission");
            
            var template = await GetTemplateOrThrow(dto.TemplateId);
            ValidateTemplateAccess(template, userId);
            ValidateRequiredAnswers(template, dto.Answers);
        }

        private async Task<Template> GetTemplateOrThrow(Guid templateId) => 
            await _templateRepository.GetByIdWithDetailsAsync(templateId) ?? throw new ArgumentException("Template not found");

        private static void ValidateTemplateAccess(Template template, Guid userId)
        {
            if (!HasAccessToTemplate(template, userId))
                throw new UnauthorizedAccessException("No template access");
        }

        private static bool HasAccessToTemplate(Template template, Guid userId) => 
            template.IsPublic || template.UserId == userId || template.Accesses.Any(a => a.UserId == userId);

        private static void ValidateRequiredAnswers(Template template, IEnumerable<AnswerDto> answers)
        {
            var missing = GetMissingRequiredQuestions(template, answers);
            if (missing.Any()) throw new ArgumentException($"Missing answers: {string.Join(", ", missing)}");
        }

        private static List<Guid> GetMissingRequiredQuestions(Template template, IEnumerable<AnswerDto> answers) => 
            template.Questions
                .Where(q => q.IsRequired)
                .Select(q => q.Id)
                .Except(answers.Select(a => a.QuestionId))
                .ToList();

        private static Form CreateFormEntity(FormDto dto, Guid userId) => 
            new()
            {
                TemplateId = dto.TemplateId,
                UserId = userId,
                Answers = dto.Answers.Select(MapToAnswer).ToList()
            };

        private static Answer MapToAnswer(AnswerDto answerDto) => new()
        {
            QuestionId = answerDto.QuestionId,
            ValueText = answerDto.MultiTextValue != null ? string.Join(";", answerDto.MultiTextValue) : answerDto.TextValue,
            ValueNumber = answerDto.NumberValue != null ? Convert.ToInt32(answerDto.NumberValue) : null,
            ValueBool = answerDto.BoolValue
        };

        private async Task<FormDetailsDto> SaveAndReturnForm(Form form, Guid userId)
        {
            var created = await _formRepository.CreateAsync(form);
            return await GetByIdAsync(created.Id, userId) ?? throw new Exception("Form creation failed");
        }

        private async Task<Form?> GetFormForUpdate(Guid id, Guid userId, bool isAdmin)
        {
            var form = await _formRepository.GetByIdWithDetailsAsync(id);
            return form == null || (form.UserId != userId && !isAdmin) ? null : form;
        }

        private static void UpdateFormAnswers(Form form, IEnumerable<AnswerDto> answerDtos)
        {
            foreach (var answerDto in answerDtos)
                UpdateFormAnswer(form, answerDto);
        }

        private static void UpdateFormAnswer(Form form, AnswerDto answerDto)
        {
            var question = form.Template.Questions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
            if (question == null) return;

            var storedValue = GetStoredValue(answerDto, question);
            var existingAnswer = form.Answers.FirstOrDefault(a => a.QuestionId == answerDto.QuestionId);

            if (existingAnswer != null) existingAnswer.ValueText = storedValue;
            else form.Answers.Add(new Answer { QuestionId = answerDto.QuestionId, ValueText = storedValue });
        }

        private static string GetStoredValue(AnswerDto answerDto, Question question)
        {
            if (answerDto.MultiTextValue?.Any() == true)
                return string.Join(";", question.Options?
                    .Where(o => answerDto.MultiTextValue.Contains(o.Text))
                    .Select(o => o.Id.ToString()) ?? Enumerable.Empty<string>());

            if (!string.IsNullOrEmpty(answerDto.TextValue))
                return GetOptionIdOrValue(question, answerDto.TextValue);

            return string.Empty;
        }

        private static string GetOptionIdOrValue(Question question, string textValue) => 
            (question.Type == "radio" || question.Type == "select")
                ? question.Options?.FirstOrDefault(o => o.Text == textValue)?.Id.ToString() ?? textValue
                : textValue;

        private async Task<bool> CanDeleteForm(Guid id, Guid userId)
        {
            var form = await _formRepository.GetByIdAsync(id);
            return form != null && form.UserId == userId;
        }

        private async Task<Template> ValidateTemplateAccess(Guid templateId, Guid userId)
        {
            var template = await GetTemplateOrThrow(templateId);
            ValidateTemplateAccess(template, userId);
            return template;
        }

        private static void ValidateRequiredQuestions(Template template, IEnumerable<Guid> answeredQuestionIds)
        {
            var missing = GetMissingRequiredQuestions(template, answeredQuestionIds);
            if (missing.Any()) throw new ArgumentException($"Missing answers: {string.Join(", ", missing)}");
        }

        private static List<Guid> GetMissingRequiredQuestions(Template template, IEnumerable<Guid> answeredQuestionIds) => 
            template.Questions
                .Where(q => q.IsRequired)
                .Select(q => q.Id)
                .Except(answeredQuestionIds)
                .ToList();

        private async Task SaveFormResponse(Guid templateId, Guid userId, Dictionary<Guid, string> responses)
        {
            var form = new Form
            {
                TemplateId = templateId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Answers = responses.Select(r => new Answer { QuestionId = r.Key, ValueText = r.Value }).ToList()
            };
            await _formRepository.CreateAsync(form);
        }

        private async Task<TemplateStatisticsDto> CalculateStatistics(Template template)
        {
            var forms = await _formRepository.GetByTemplateWithDetailsAsync(template.Id);
            return CreateStatisticsDto(template, forms);
        }

        private static TemplateStatisticsDto CreateStatisticsDto(Template template, IEnumerable<Form> forms) => 
            new()
            {
                TemplateId = template.Id,
                TemplateTitle = template.Title,
                SubmissionsByDay = forms.GroupBy(f => f.CreatedAt.Date)
                    .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count()),
                QuestionStatistics = template.Questions.ToDictionary(
                    q => q.Title,
                    q => GetQuestionStatistics(q, forms))
            };

        private static Dictionary<string, int> GetQuestionStatistics(Question question, IEnumerable<Form> forms)
        {
            var answers = forms.SelectMany(f => f.Answers.Where(a => a.QuestionId == question.Id));
            return question.Type switch
            {
                "boolean" => GetBooleanStatistics(answers),
                "radio" or "checkbox" => GetOptionStatistics(question, answers),
                _ => new Dictionary<string, int>()
            };
        }

        private static Dictionary<string, int> GetBooleanStatistics(IEnumerable<Answer> answers) => 
            new()
            {
                ["Yes"] = answers.Count(a => a.ValueBool == true),
                ["No"] = answers.Count(a => a.ValueBool == false)
            };

        private static Dictionary<string, int> GetOptionStatistics(Question question, IEnumerable<Answer> answers) => 
            question.Options?.ToDictionary(
                o => o.Text,
                o => answers.Count(a => a.ValueText?.Contains(o.Text) == true)) ?? new Dictionary<string, int>();

        private async Task<FormDetailsDto?> GetUserTemplateForm(Guid userId, Guid templateId)
        {
            var form = await _formRepository.GetByUserAndTemplateAsync(userId, templateId);
            return form == null ? null : _mapper.Map<FormDetailsDto>(form);
        }

        private static FormDetailsDto MapToFormDetailsDto(Form form) => 
            new()
            {
                Id = form.Id,
                TemplateId = form.TemplateId,
                UserId = form.UserId,
                CreatedAt = form.CreatedAt,
                Answers = form.Answers.Select(MapToAnswerDetailsDto).ToList()
            };

        private static AnswerDetailsDto MapToAnswerDetailsDto(Answer answer) => 
            new()
            {
                Id = answer.Id,
                QuestionId = answer.Question.Id,
                QuestionTitle = answer.Question.Title,
                QuestionDescription = answer.Question.Description ?? string.Empty,
                QuestionType = answer.Question.Type,
                Options = answer.Question.Options?.OrderBy(o => o.Position).Select(o => o.Text).ToList() ?? new List<string>(),
                TextValue = GetDisplayValue(answer, answer.Question),
                MultiTextValue = GetMultiTextValue(answer, answer.Question)
            };

        private static string GetDisplayValue(Answer answer, Question question) => 
            question.Type != "radio" && question.Type != "select"
                ? answer.ValueText
                : question.Options?.FirstOrDefault(o => o.Id.ToString() == answer.ValueText)?.Text ?? answer.ValueText;

        private static List<string>? GetMultiTextValue(Answer answer, Question question)
        {
            if (question.Type != "checkbox" || string.IsNullOrEmpty(answer.ValueText)) return null;
            return answer.ValueText.Split(';')
                .Select(id => question.Options?.FirstOrDefault(o => o.Id.ToString() == id)?.Text)
                .Where(text => text != null)
                .ToList()!;
        }

        private async Task<bool> HasExistingResponse(Guid userId, Guid templateId) => 
            await _formRepository.GetByUserAndTemplateAsync(userId, templateId) != null;

        private static FormResult FormAlreadySubmittedResult() => 
            new FormResult("You have already submitted a response for this template");

        private static Dictionary<Guid, string> ParseFormResponses(IFormCollection formCollection) => 
            formCollection
                .Where(k => k.Key.StartsWith("responses[") && k.Key.EndsWith("]"))
                .Select(k => new
                {
                    QuestionId = Guid.TryParse(k.Key[10..^1], out var qId) ? qId : Guid.Empty,
                    Value = string.Join(";", k.Value.ToArray())
                })
                .Where(r => r.QuestionId != Guid.Empty)
                .ToDictionary(r => r.QuestionId, r => r.Value);

        private static FormResult SuccessFormResult() => 
            new FormResult(true, "Your responses have been saved!");

        private static FormResult ErrorFormResult(Exception ex) => 
            new FormResult(ex.Message);

        private async Task<ServiceResult<TemplateFormsView>> CreateTemplateFormsResult(Template template)
        {
            var forms = await MapForms(await _formRepository.GetByTemplateAsync(template.Id));
            return new ServiceResult<TemplateFormsView>(new TemplateFormsView
            {
                Forms = forms,
                TemplateTitle = template.Title,
                TemplateId = template.Id
            });
        }

        private static ServiceResult<TemplateFormsView> ErrorTemplateFormsResult(Exception ex) => 
            new ServiceResult<TemplateFormsView>(ex.Message);

        private async Task<FormDetailsDto> GetFormOrThrow(Guid id, Guid userId)
        {
            var form = await GetByIdAsync(id, userId);
            return form ?? throw new Exception("Form not found");
        }

        private static ServiceResult<FormDetailsDto> SuccessFormDetailsResult(FormDetailsDto form) => 
            new ServiceResult<FormDetailsDto>(form);

        private static ServiceResult<FormDetailsDto> ErrorFormDetailsResult(Exception ex) => 
            new ServiceResult<FormDetailsDto>(ex.Message);

        private static void ValidateFormDto(FormDto dto)
        {
            if (dto?.Id == null) throw new ArgumentException("Invalid form submission");
        }

        private static FormOperationResult CreateUpdateResult(bool success, bool isAdmin) => 
            new FormOperationResult(success, success ? "Form updated successfully" : "Update failed")
            {
                RedirectAction = isAdmin ? "AdminEdit" : "Details"
            };

        private static FormOperationResult ErrorUpdateResult(Exception ex) => 
            new FormOperationResult(ex.Message);

        private static OperationResult CreateDeleteResult(bool success) => 
            new OperationResult(success, success ? "Form deleted" : "Failed to delete form");

        private static OperationResult ErrorDeleteResult(Exception ex) => 
            new OperationResult(ex.Message);
    }
}
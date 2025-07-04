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

        public async Task<FormDetailsDto?> GetByIdAsync(Guid id, Guid currentUserId)
        {
            var form = await _formRepository.GetByIdWithDetailsAsync(id);
            return form == null ? null : MapToFormDetailsDto(form);
        }

        public async Task<IEnumerable<FormDetailsDto>> GetByUserAsync(Guid userId) =>
            _mapper.Map<IEnumerable<FormDetailsDto>>(await _formRepository.GetByUserAsync(userId));

        public async Task<FormDetailsDto> CreateAsync(FormDto dto, Guid userId)
        {
            await ValidateFormCreation(dto, userId);

            var form = new Form
            {
                TemplateId = dto.TemplateId,
                UserId = userId,
                Answers = dto.Answers.Select(MapToAnswer).ToList()
            };

            var created = await _formRepository.CreateAsync(form);
            return await GetByIdAsync(created.Id, userId)
                   ?? throw new Exception("Failed to retrieve created form");
        }

        public async Task<bool> UpdateAsync(FormDto dto, Guid userId, bool isAdmin = false)
        {
            var form = await _formRepository.GetByIdWithDetailsAsync(dto.Id!.Value);
            if (form == null || (form.UserId != userId && !isAdmin)) return false;

            UpdateFormAnswers(form, dto.Answers);
            await _formRepository.UpdateAsync(form);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var form = await _formRepository.GetByIdAsync(id);
            if (form == null || (form.UserId != userId)) return false;

            await _formRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> CreateResponseAsync(Guid templateId, Guid userId, Dictionary<Guid, string> responses)
        {
            var template = await ValidateTemplateAccess(templateId, userId);
            ValidateRequiredQuestions(template, responses.Keys);

            var form = new Form
            {
                TemplateId = templateId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Answers = responses.Select(r => new Answer { QuestionId = r.Key, ValueText = r.Value }).ToList()
            };

            await _formRepository.CreateAsync(form);
            return true;
        }

        public async Task<IEnumerable<FormDetailsDto>> GetAllFormsAsync() =>
            _mapper.Map<IEnumerable<FormDetailsDto>>(await _formRepository.GetAllWithDetailsAsync());

        public async Task<IEnumerable<FormDetailsDto>> GetByTemplateAsync(Guid templateId, Guid currentUserId)
        {
            await ValidateTemplateAccess(templateId, currentUserId);
            return _mapper.Map<IEnumerable<FormDetailsDto>>(await _formRepository.GetByTemplateAsync(templateId));
        }

        public async Task<TemplateStatisticsDto> GetTemplateStatisticsAsync(Guid templateId, Guid currentUserId)
        {
            var template = await ValidateTemplateAccess(templateId, currentUserId);
            var forms = await _formRepository.GetByTemplateWithDetailsAsync(templateId);

            return new TemplateStatisticsDto
            {
                TemplateId = templateId,
                TemplateTitle = template.Title,
                SubmissionsByDay = forms.GroupBy(f => f.CreatedAt.Date)
                    .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count()),
                QuestionStatistics = template.Questions.ToDictionary(
                    q => q.Title,
                    q => GetQuestionStatistics(q, forms))
            };
        }

        public async Task<FormDetailsDto?> GetByUserAndTemplateAsync(Guid userId, Guid templateId)
        {
            var form = await _formRepository.GetByUserAndTemplateAsync(userId, templateId);
            return form == null ? null : _mapper.Map<FormDetailsDto>(form);
        }

        public async Task<IEnumerable<FormDetailsDto>> GetUserFormsAsync(Guid userId)
        {
            var forms = await _formRepository.GetByUserAsync(userId);
            return _mapper.Map<IEnumerable<FormDetailsDto>>(forms);
        }

        private async Task ValidateFormCreation(FormDto dto, Guid userId)
        {
            if (await _formRepository.GetByUserAndTemplateAsync(userId, dto.TemplateId) != null)
                throw new InvalidOperationException("You have already submitted a form for this template");

            var template = await _templateRepository.GetByIdWithDetailsAsync(dto.TemplateId)
                           ?? throw new ArgumentException("Template not found");

            if (!HasAccessToTemplate(template, userId))
                throw new UnauthorizedAccessException("You don't have access to this template");

            var missingQuestions = template.Questions
                .Where(q => q.IsRequired)
                .Select(q => q.Id)
                .Except(dto.Answers.Select(a => a.QuestionId))
                .ToList();

            if (missingQuestions.Any())
                throw new ArgumentException($"Missing answers for required questions: {string.Join(", ", missingQuestions)}");
        }

        private async Task<Template> ValidateTemplateAccess(Guid templateId, Guid userId)
        {
            var template = await _templateRepository.GetByIdWithDetailsAsync(templateId)
                           ?? throw new ArgumentException("Template not found");

            if (!HasAccessToTemplate(template, userId))
                throw new UnauthorizedAccessException("You don't have access to this template");

            return template;
        }

        private static bool HasAccessToTemplate(Template template, Guid userId)
        {
            return template.IsPublic || template.UserId == userId || template.Accesses.Any(a => a.UserId == userId);
        }

        private static void ValidateRequiredQuestions(Template template, IEnumerable<Guid> answeredQuestionIds)
        {
            var missingRequired = template.Questions
                .Where(q => q.IsRequired)
                .Select(q => q.Id)
                .Except(answeredQuestionIds)
                .ToList();

            if (missingRequired.Any())
                throw new ArgumentException($"Missing answers for required questions: {string.Join(", ", missingRequired)}");
        }

        private static Answer MapToAnswer(AnswerDto answerDto) => new()
        {
            QuestionId = answerDto.QuestionId,
            ValueText = answerDto.MultiTextValue != null ? string.Join(";", answerDto.MultiTextValue) : answerDto.TextValue,
            ValueNumber = answerDto.NumberValue != null ? Convert.ToInt32(answerDto.NumberValue) : null,
            ValueBool = answerDto.BoolValue
        };

        private static void UpdateFormAnswers(Form form, IEnumerable<AnswerDto> answerDtos)
        {
            foreach (var answerDto in answerDtos)
            {
                var question = form.Template.Questions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
                if (question == null) continue;

                var valueToStore = GetStoredValue(answerDto, question);
                var existingAnswer = form.Answers.FirstOrDefault(a => a.QuestionId == answerDto.QuestionId);

                if (existingAnswer != null)
                    existingAnswer.ValueText = valueToStore;
                else
                    form.Answers.Add(new Answer { QuestionId = answerDto.QuestionId, ValueText = valueToStore });
            }
        }

        private static string GetStoredValue(AnswerDto answerDto, Question question)
        {
            if (answerDto.MultiTextValue?.Any() == true)
                return string.Join(";", question.Options?
                    .Where(o => answerDto.MultiTextValue.Contains(o.Text))
                    .Select(o => o.Id.ToString()) ?? Enumerable.Empty<string>());

            if (!string.IsNullOrEmpty(answerDto.TextValue))
            {
                return (question.Type == "radio" || question.Type == "select")
                    ? question.Options?.FirstOrDefault(o => o.Text == answerDto.TextValue)?.Id.ToString() ?? answerDto.TextValue
                    : answerDto.TextValue;
            }

            return string.Empty;
        }

        private static Dictionary<string, int> GetQuestionStatistics(Question question, IEnumerable<Form> forms)
        {
            var answers = forms.SelectMany(f => f.Answers.Where(a => a.QuestionId == question.Id));

            return question.Type switch
            {
                "boolean" => new Dictionary<string, int>
                {
                    ["Yes"] = answers.Count(a => a.ValueBool == true),
                    ["No"] = answers.Count(a => a.ValueBool == false)
                },
                "radio" or "checkbox" => question.Options?.ToDictionary(
                    o => o.Text,
                    o => answers.Count(a => a.ValueText?.Contains(o.Text) == true)) ?? new Dictionary<string, int>(),
                _ => new Dictionary<string, int>()
            };
        }

        private static FormDetailsDto MapToFormDetailsDto(Form form) => new()
        {
            Id = form.Id,
            TemplateId = form.TemplateId,
            UserId = form.UserId,
            CreatedAt = form.CreatedAt,
            Answers = form.Answers.Select(MapToAnswerDetailsDto).ToList()
        };

        private static AnswerDetailsDto MapToAnswerDetailsDto(Answer answer)
        {
            var question = answer.Question;
            var options = question.Options?
                .OrderBy(o => o.Position)
                .Select(o => o.Text)
                .ToList() ?? new List<string>();

            return new AnswerDetailsDto
            {
                Id = answer.Id,
                QuestionId = question.Id,
                QuestionTitle = question.Title,
                QuestionDescription = question.Description ?? string.Empty,
                QuestionType = question.Type,
                Options = options,
                TextValue = GetDisplayValue(answer, question),
                MultiTextValue = GetMultiTextValue(answer, question)
            };
        }

        private static string GetDisplayValue(Answer answer, Question question)
        {
            if (question.Type != "radio" && question.Type != "select")
                return answer.ValueText;

            return question.Options?.FirstOrDefault(o => o.Id.ToString() == answer.ValueText)?.Text ?? answer.ValueText;
        }

        private static List<string>? GetMultiTextValue(Answer answer, Question question)
        {
            if (question.Type != "checkbox" || string.IsNullOrEmpty(answer.ValueText))
                return null;

            return answer.ValueText
                .Split(';')
                .Select(id => question.Options?.FirstOrDefault(o => o.Id.ToString() == id)?.Text)
                .Where(text => text != null)
                .ToList()!;
        }

        public async Task<FormResult> ProcessFormResponseAsync(Guid templateId, Guid userId, IFormCollection formCollection)
        {
            try
            {
                if (await GetByUserAndTemplateAsync(userId, templateId) != null)
                    return new FormResult("You have already submitted a response for this template");

                var responses = formCollection
                    .Where(k => k.Key.StartsWith("responses[") && k.Key.EndsWith("]"))
                    .Select(k => new
                    {
                        QuestionId = Guid.TryParse(k.Key[10..^1], out var qId) ? qId : Guid.Empty,
                        Value = string.Join(";", k.Value.ToArray())
                    })
                    .Where(r => r.QuestionId != Guid.Empty)
                    .ToDictionary(r => r.QuestionId, r => r.Value);

                var success = await CreateResponseAsync(templateId, userId, responses);
                return new FormResult(success, success ? "Your responses have been saved!" : "Failed to save your responses");
            }
            catch (Exception ex)
            {
                return new FormResult(ex.Message);
            }
        }

        public async Task<ServiceResult<TemplateFormsView>> GetTemplateFormsAsync(Guid templateId, Guid userId)
        {
            try
            {
                var template = await ValidateTemplateAccess(templateId, userId);
                var forms = _mapper.Map<IEnumerable<FormDetailsDto>>(await _formRepository.GetByTemplateAsync(templateId));
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
                var form = await GetByIdAsync(id, userId) ?? throw new Exception("Form not found");
                return new ServiceResult<FormDetailsDto>(form);
            }
            catch (Exception ex)
            {
                return new ServiceResult<FormDetailsDto>(ex.Message);
            }
        }

        public async Task<FormOperationResult> UpdateFormAsync(FormDto dto, Guid userId, bool isAdmin)
        {
            try
            {
                if (dto?.Id == null) throw new ArgumentException("Invalid form submission");
                var success = await UpdateAsync(dto, userId, isAdmin);
                return new FormOperationResult(success, success ? "Form updated successfully" : "Update failed")
                {
                    RedirectAction = isAdmin ? "AdminEdit" : "Details"
                };
            }
            catch (Exception ex)
            {
                return new FormOperationResult(ex.Message);
            }
        }

        public async Task<OperationResult> DeleteFormAsync(Guid id, Guid userId)
        {
            try
            {
                var success = await DeleteAsync(id, userId);
                return new OperationResult(success, success ? "Form deleted" : "Failed to delete form");
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message);
            }
        }
    }
}

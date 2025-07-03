namespace MyFormixApp.Domain.DTOs.Templates
{
    public class TemplateStatisticsDto
    {
        public Guid TemplateId { get; set; }
        public required string TemplateTitle { get; set; }
        public int TotalSubmissions { get; set; }
        public required Dictionary<string, int> SubmissionsByDay { get; set; }
        public required Dictionary<string, Dictionary<string, int>> QuestionStatistics { get; set; }
    }
}
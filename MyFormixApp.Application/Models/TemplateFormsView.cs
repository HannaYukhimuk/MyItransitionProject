using MyFormixApp.Domain.DTOs.Forms;

namespace MyFormixApp.Application.Models
{
    public class TemplateFormsView
    {
        public Guid TemplateId { get; set; }
        public string TemplateTitle { get; set; }
        public IEnumerable<FormDetailsDto> Forms { get; set; }
    }
}
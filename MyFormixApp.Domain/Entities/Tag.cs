namespace MyFormixApp.Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TemplateTag> Templates { get; set; } = new List<TemplateTag>();
    }
}
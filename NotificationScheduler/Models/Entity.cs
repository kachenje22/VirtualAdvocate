using System;

namespace NotificationScheduler.Models
{
    public class Entity
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
        public DateTime DateOfExpiry { get; set; }
        public int UserId { get; set; }
        public TemplateType TemplateType { get; set; }
        public int TemplateId { get; set; }
        public string CustomerName { get; set; }
        public int DocumentId { get; set; }
    }
}

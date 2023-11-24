using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Models
{
    public class Category
    {
        [Key]
        public string CategoryId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

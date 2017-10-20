using System;
using System.ComponentModel.DataAnnotations;


namespace TOIFeedServer.Models
{
    public class ContextModel
    {
        public ContextModel()
        {
            
        }
        public ContextModel(int id, string title, string description = null)
        {
            Id = id;
            Title = title;
            Description = description;
        }

        [Key]
        public int Id { get; set; }

        public string Description { get; set; }

        [StringLength(70)]
        [Required]
        public string Title { get; set; }
    }
}

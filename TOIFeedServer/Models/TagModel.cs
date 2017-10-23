using System;
using System.ComponentModel.DataAnnotations;

namespace TOIFeedServer.Models
{
    public class TagModel
    {
        public TagModel()
        {
            
        }
        public TagModel(Guid id, TagType type)
        {
            TagId = id;
            TagType = type;
        }

        [Key]
        public Guid TagId { get; set; }

        public TagType TagType { get; set; }
    }
}

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

        public override bool Equals(object obj)
        {
            return obj is TagModel t && t.TagId == TagId;
        }

        protected bool Equals(TagModel other)
        {
            return TagId.Equals(other.TagId);
        }

        public override int GetHashCode()
        {
            return TagId.GetHashCode();
        }
    }
}

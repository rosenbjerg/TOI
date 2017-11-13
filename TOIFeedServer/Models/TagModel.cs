    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

namespace TOIFeedServer.Models
{
    public class TagModel
    {
        public TagModel()
        {
            
        }
        public TagModel(string id, TagType type)
        {
            TagId = id;
            TagType = type;
        }

        [Key]
        public string TagId { get; set; }

        public string Name { get; set; }

        public TagType TagType { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }   

        public int Radius { get; set; }

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

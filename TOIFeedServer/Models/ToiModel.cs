using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TOIClasses;

namespace TOIFeedServer.Models
{
    public class ToiModel : TagInfo
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public List<TagModel> TagModels { get; set; } = new List<TagModel>();
        public List<ContextModel> ContextModels { get; set; } = new List<ContextModel>();

        public object GetToiInfo()
        {
            return new { Title, Description, Url, Image };
        }

        public override bool Equals(object obj)
        {
            return obj is ToiModel t && t.Id == Id;
        }

        protected bool Equals(ToiModel other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

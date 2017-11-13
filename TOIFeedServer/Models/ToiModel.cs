using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TOIClasses;

namespace TOIFeedServer.Models
{
    public class ToiModel : TagInfo
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public List<ToiTagModel> TagModels { get; set; } = new List<ToiTagModel>();
        public List<ToiContextModel> ContextModels { get; set; } = new List<ToiContextModel>();

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

        public ToiModel(){}

        public ToiModel(List<ContextModel> ctx, List<TagModel> tags)
        {
            TagModels = tags.Select(t => new ToiTagModel(this, t)).ToList();
            ContextModels = ctx.Select(c => new ToiContextModel(this, c)).ToList();
        }

        public void AddTag(TagModel tag) => TagModels.Add(new ToiTagModel(this, tag));
        public void AddContext(ContextModel ctx) => ContextModels.Add(new ToiContextModel(this, ctx));
    }
}

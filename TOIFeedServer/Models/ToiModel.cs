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
        public Guid Id { get; set; }

        public List<ToiTagModel> TagModels { get; set; }
        public List<ToiContextModel> ContextModels { get; set; }

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

        public ToiModel(Guid id, List<ContextModel> ctx, List<TagModel> tags)
        {
            Id = id
            TagModels = tags.Select(t => new ToiTagModel(this, t)).ToList();
            ContextModels = ctx.Select(c => new ToiContextModel(this, c)).ToList();
        }

        public void AddTag(TagModel tag) => TagModels.Add(new ToiTagModel(this, tag));
        public void AddContext(ContextModel ctx) => ContextModels.Add(new ToiContextModel(this, ctx));
    }
}

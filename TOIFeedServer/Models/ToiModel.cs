using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TOIClasses;

namespace TOIFeedServer.Models
{
    public class ToiModel
    {
        public ToiModel()
        {
            
        }
        public ToiModel(Guid id, TagInfoModel tagInfoModel)
        {
            Id = id;
            TagInfoModel = tagInfoModel;
        }

        [Key]
        public Guid Id { get; set; }
        public TagInfoModel TagInfoModel { get; set; }

        public List<TagModel> TagModels { get; set; }
        public ContextModel ContextModel { get; set; }

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

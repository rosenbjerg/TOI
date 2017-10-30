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

        public List<TagModel> TagModel { get; set; }
        public ContextModel ContextModel { get; set; }
    }
}

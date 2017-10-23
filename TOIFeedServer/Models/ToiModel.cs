using System;
using System.ComponentModel.DataAnnotations;
using TOIClasses;

namespace TOIFeedServer.Models
{
    public class ToiModel
    {
        public ToiModel()
        {
            
        }

        public ToiModel(Guid id, TagInfoModel info)
        {
            Id = id;
            Info = info;
        }

        [Key]

        public Guid Id { get; set; }
        public TagInfoModel Info { get; set; }

        public TagModel TagModel { get; set; }
        public ContextModel ContextModel { get; set; }

    }
}

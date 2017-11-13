using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOIFeedServer.Models
{
    public class ToiTagModel
    {
        public Guid ToiId { get; set; }
        [ForeignKey(nameof(ToiId))]
        public ToiModel Toi { get; set; }
        
        public Guid TagId { get; set; }
        [ForeignKey(nameof(TagId))]
        public TagModel Tag { get; set; }
        
        public ToiTagModel(ToiModel toi, TagModel tag)
        {
            Toi = toi;
            ToiId = toi.Id;
            TagId = tag.TagId;
            Tag = tag;
        }

        public ToiTagModel(){}
    }
}
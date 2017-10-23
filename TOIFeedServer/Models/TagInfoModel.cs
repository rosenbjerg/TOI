using System;
using System.Collections.Generic;
using System.Text;
using TOIClasses;

namespace TOIFeedServer.Models
{
    public class TagInfoModel : TagInfo
    {
        public TagInfoModel()
        {
            Id = new Guid();
        }

        public TagInfo GetTagInfo()
        {
            return new TagInfo
            {
                Description = this.Description,
                Image = this.Image,
                Title = this.Title,
                Url = this.Url
            };
        }

        public Guid Id { get; set; }
    }
}

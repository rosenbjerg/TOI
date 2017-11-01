using System;
using Newtonsoft.Json;
using TOIClasses;

namespace TOIFeedServer.Models
{
    public class TagInfoModel : TagInfo
    {
        public TagInfoModel()
        {
            Id = new Guid();
        }

        [JsonIgnore]
        public Guid Id { get; set; }

        public override bool Equals(object obj)
        {
            return obj is TagInfoModel t && t.Id == Id;
        }

        protected bool Equals(TagInfoModel other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

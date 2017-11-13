using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TOIClasses;

namespace TOIFeedServer.Models
{
    public class ToiModel : TagInfo
    {
        public class StringId
        {
            [Key]
            public Guid Key { get; set; }

            public string Value { get; set; }

            public StringId()
            {
                
            }

            public StringId(string value)
            {
                Key = Guid.NewGuid();
                Value = value;
            }

            public static implicit operator string(StringId si)
            {
                return si.Value;
            }

            public static implicit operator StringId(string s)
            {
                return new StringId(s);
            }

            public override bool Equals(object obj)
            {
                return obj is StringId si && si.Value == this.Value;
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        [Key]
        public string Id { get; set; }

        public List<StringId> Tags { get; set; }
        public List<StringId> Contexts { get; set; }

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

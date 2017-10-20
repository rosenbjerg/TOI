using System.ComponentModel.DataAnnotations;

namespace TOIFeedServer.Models
{
    public class TagModel
    {
        public TagModel()
        {
            
        }
        public TagModel(int id, TagType type)
        {
            TagId = id;
            TagType = type;
        }

        [Key]
        public int TagId { get; set; }

        public TagType TagType { get; set; }
    }
}

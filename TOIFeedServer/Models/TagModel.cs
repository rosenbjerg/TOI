using System.ComponentModel.DataAnnotations;

namespace TOIFeedServer.Models
{
    public class TagModel
    {
        public TagModel(int id)
        {
            TagId = id;
        }

        [Key]
        public int TagId { get; set; }
    }
}

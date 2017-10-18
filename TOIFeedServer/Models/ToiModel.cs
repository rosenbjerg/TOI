using System.ComponentModel.DataAnnotations;

namespace TOIFeedServer.Models
{
    public class ToiModel
    {
        public ToiModel()
        {
            
        }
        public ToiModel(string type)
        {
            Type = type;
        }

        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
    }
}

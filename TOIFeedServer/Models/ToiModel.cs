using System.ComponentModel.DataAnnotations;

namespace TOIFeedServer.Models
{
    public class ToiModel
    {
        public ToiModel()
        {
            
        }
        public ToiModel(int id ,string info)
        {
            Id = id;
            Info = info;
        }

        [Key]
        public int Id { get; set; }
        public string Info { get; set; }

        public TagModel TagModel { get; set; }
        public ContextModel ContextModel { get; set; }
    }
}

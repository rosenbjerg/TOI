using System.ComponentModel.DataAnnotations;


namespace TOIFeedServer.Models
{
    public class ContextModel
    {
        public ContextModel(int id)
        {
            Id = id;
        }

        [Key]
        public int Id { get; set; }

        public string Description { get; set; }
        public string Title { get; set; }
    }
}

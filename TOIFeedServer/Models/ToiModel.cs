using System;
using System.ComponentModel.DataAnnotations;

namespace TOIFeedServer.Models
{
    public class ToiModel
    {
        public ToiModel()
        {
            
        }
<<<<<<< Updated upstream
        public ToiModel(string type)
=======
        public ToiModel(Guid id ,string info)
>>>>>>> Stashed changes
        {
            Type = type;
        }

        [Key]
<<<<<<< Updated upstream
        public int Id { get; set; }
        public string Type { get; set; }
=======
        public Guid Id { get; set; }
        public string Info { get; set; }

        public TagModel TagModel { get; set; }
        public ContextModel ContextModel { get; set; }
>>>>>>> Stashed changes
    }
}

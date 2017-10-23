using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOIFeedServer.Models
{
    public class PositionModel
    {
        public PositionModel()
        {
            
        }
        public PositionModel(TagModel tm, double x, double y)
        {
            this.TagModelId = tm.TagId;
            this.TagModel = tm;
            X = x;
            Y = y;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(TagModelId))]
        public TagModel TagModel { get; set; }
        public Guid TagModelId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

}

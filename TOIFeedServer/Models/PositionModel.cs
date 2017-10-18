using System.ComponentModel.DataAnnotations;

namespace TOIFeedServer.Models
{
    public class PositionModel
    {
        public PositionModel(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        [Key]
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

}

namespace ApiProject.Dtos.reviews
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int StarRating { get; set; }
        public string? Comment { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
namespace SentimentApi.Dtos
{
    public class CommentCreateDto
    {
        public required string ProductId { get; set; }
        public required string UserId { get; set; }
        public required string CommentText { get; set; }
    }

}

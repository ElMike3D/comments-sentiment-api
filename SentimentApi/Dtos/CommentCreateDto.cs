namespace SentimentApi.Dtos
{
    public class CommentCreateDto
    {
        public string ProductId { get; set; }
        public string UserId { get; set; }
        public string CommentText { get; set; }
    }

}

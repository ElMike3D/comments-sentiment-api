using Microsoft.AspNetCore.Mvc;
using SentimentApi.Data;
using SentimentApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SentimentApi.Dtos;
using SentimentApi.Services;

namespace SentimentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/comments/ping
        [HttpGet("ping")]
        public async Task<ActionResult<string>> Ping()
        {
            try
            {
                await _context.Comments.CountAsync();
                return Ok("Conexión a la base de datos exitosa");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error de conexión a la base de datos: {ex.Message}");
            }
        }

        // GET /api/comments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetCommentById(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound();
            return Ok(comment);
        }

        // POST /api/comments
        [HttpPost]
        public async Task<ActionResult<Comment>> CreateComment([FromBody] CommentCreateDto dto)
        {
            var comment = new Comment
            {
                ProductId = dto.ProductId,
                UserId = dto.UserId,
                CommentText = dto.CommentText,
                CreatedAt = DateTime.UtcNow
            };

            string text = comment.CommentText.ToLower();

            bool contienePositivo = text.Contains("excelente") || text.Contains("genial") ||
                                    text.Contains("fantástico") || text.Contains("bueno") || text.Contains("increíble");
            bool contieneNegativo = text.Contains("malo") || text.Contains("terrible") ||
                                    text.Contains("problema") || text.Contains("defecto") || text.Contains("horrible");

            if (contienePositivo && contieneNegativo)
            {
                comment.Sentiment = "neutral";
            }
            else if (contienePositivo)
            {
                comment.Sentiment = "positivo";
            }
            else if (contieneNegativo)
            {
                comment.Sentiment = "negativo";
            }
            else
            {
                comment.Sentiment = "neutral";
            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment);
        }

        // GET /api/comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments(
            [FromQuery] string product_id = null,
            [FromQuery] string sentiment = null)
        {
            var query = _context.Comments.AsQueryable();

            if (!string.IsNullOrEmpty(product_id))
            {
                query = query.Where(c => c.ProductId == product_id);
            }

            if (!string.IsNullOrEmpty(sentiment))
            {
                query = query.Where(c => c.Sentiment.ToLower() == sentiment.ToLower());
            }

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }

        // GET /api/sentiment-summary
        [HttpGet("/api/sentiment-summary")]
        public async Task<ActionResult<object>> GetSentimentSummary()
        {
            int totalComments = await _context.Comments.CountAsync();

            var sentimentCounts = await _context.Comments
                .GroupBy(c => c.Sentiment)
                .Select(g => new { Sentiment = g.Key, Count = g.Count() })
                .ToListAsync();

            var sentimentDict = sentimentCounts.ToDictionary(x => x.Sentiment, x => x.Count);

            foreach (var sentiment in new[] { "positivo", "negativo", "neutral" })
            {
                if (!sentimentDict.ContainsKey(sentiment))
                    sentimentDict[sentiment] = 0;
            }

            return Ok(new
            {
                total_comments = totalComments,
                sentiment_counts = sentimentDict
            });
        }

        // POST /api/comments/ai
        [HttpPost("ai")]
        public async Task<ActionResult<Comment>> CreateCommentWithAi(
            [FromBody] CommentCreateDto dto,
            [FromServices] AiService aiService)
        {
            var comment = new Comment
            {
                ProductId = dto.ProductId,
                UserId = dto.UserId,
                CommentText = dto.CommentText,
                CreatedAt = DateTime.UtcNow
            };

            comment.Sentiment = await aiService.AnalyzeSentimentAsync(dto.CommentText);

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment);
        }

        // GET /api/comments/{productId}/ai-summary
        [HttpGet("{productId}/ai-summary")]
        public async Task<ActionResult<string>> GetAiProductSummary(string productId, [FromServices] AiService aiService)
        {
            try
            {
                var summary = await aiService.SummarizeProductCommentsAsync(productId);
                return Ok(new { productId, summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al generar el resumen AI: {ex.Message}");
            }
        }


    }
}

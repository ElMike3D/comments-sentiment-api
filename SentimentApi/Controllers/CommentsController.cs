using Microsoft.AspNetCore.Mvc;
using SentimentApi.Data;
using SentimentApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        // GET temporal para probar la conexión
        [HttpGet("test")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsTest()
        {
            var comments = await _context.Comments.ToListAsync();
            return Ok(comments);
        }
    }
}

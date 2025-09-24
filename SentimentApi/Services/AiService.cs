using GenerativeAI;
using Microsoft.EntityFrameworkCore;
using SentimentApi.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SentimentApi.Services
{
    public class AiService
    {
        private readonly GenerativeModel _model;
        private readonly AppDbContext _context;

        public AiService(string apiKey, AppDbContext context)
        {
            var googleAI = new GoogleAi(apiKey);

            _model = googleAI.CreateGenerativeModel("models/gemini-1.5-flash");
            _context = context;
        }

        public async Task<string> AnalyzeSentimentAsync(string text)
        {
            string prompt = $@"
            Clasifica el siguiente comentario en 'positivo', 'negativo' o 'neutral'.
            Comentario: ""{text}""
            Responde SOLO con una de estas tres palabras: positivo, negativo o neutral.";

            var response = await _model.GenerateContentAsync(prompt);

            var sentiment = response.Text().Trim().ToLower();

            if (sentiment.Contains("positivo")) return "positivo";
            if (sentiment.Contains("negativo")) return "negativo";
            if (sentiment.Contains("neutral")) return "neutral";

            return "neutral";
        }

        public async Task<string> SummarizeProductCommentsAsync(string productId)
        {
            var comments = await _context.Comments
                .Where(c => c.ProductId == productId)
                .OrderBy(c => Guid.NewGuid())
                .Take(20)
                .Select(c => c.CommentText)
                .ToListAsync();


            if (comments == null || !comments.Any())
                return "No hay comentarios disponibles para este producto.";

            string prompt = $@"
            Resume de forma clara y breve las siguientes reseñas de un producto.
            Destaca los puntos positivos y negativos más frecuentes y escribe
            el resultado en un solo breve párrafo de máximo dos líneas.
            No menciones información cuantificable, nada de cantidad de reseñas ni de usuarios.
            
            Reseñas:
            {string.Join("\n- ", comments)}";

            var response = await _model.GenerateContentAsync(prompt);

            return response.Text().Trim();
        }
    }
}

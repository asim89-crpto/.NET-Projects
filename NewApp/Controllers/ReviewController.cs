using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewApp.Data;
using NewApp.Models;
using NewApp.Services;

namespace NewApp.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly SentimentModel _sentimentModel;

        public ReviewController(AppDbContext context, SentimentModel sentimentModel)
        {
            _context = context;
            _sentimentModel = sentimentModel;
        }

        [HttpPost]
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
            var prediction = _sentimentModel.PredictSentiment(review.Text);
            review.Sentiment = prediction?.Prediction == true ? "Positive" : "Negative";
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            return await _context.Reviews.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            return review;
        }

        [HttpGet("sentiment-counts")]
        public async Task<ActionResult<object>> GetSentimentCounts()
        {
            var counts = await _context.Reviews
                .GroupBy(r => r.Sentiment)
                .Select(g => new { Sentiment = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(counts.ToDictionary(c => c.Sentiment, c => c.Count));
        }

        [HttpGet("sentiment-trends")]
        public async Task<ActionResult<IEnumerable<object>>> GetSentimentTrends()
        {
            var trends = await _context.Reviews
                .GroupBy(r => r.Date.Date)
                .Select(g => new
                {
                    Date = g.Key, // Keep DateTime for now
                    Positive = g.Count(r => r.Sentiment == "Positive"),
                    Negative = g.Count(r => r.Sentiment == "Negative")
                })
                .OrderBy(t => t.Date)
                .ToListAsync();  // ✅ Executes in database first

            // ✅ Convert DateTime -> String AFTER executing the query
            var formattedTrends = trends.Select(t => new
            {
                Date = t.Date.ToString("yyyy-MM-dd"),  // Now it's safe
                Positive = t.Positive,
                Negative = t.Negative
            }).ToList();

            return Ok(formattedTrends);
        }



        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<Review>>> GetRecentReviews()
        {
            var recentReviews = await _context.Reviews
                .OrderByDescending(r => r.Date)
                .Take(10)
                .ToListAsync();

            return Ok(recentReviews);
        }
    }
}
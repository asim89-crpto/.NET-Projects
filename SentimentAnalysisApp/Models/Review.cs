using System.ComponentModel.DataAnnotations;

namespace NewApp.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public string Sentiment { get; set; } // Positive, Negative, Neutral

        public DateTime Date { get; set; } = DateTime.UtcNow; // Add Date property
    }
}
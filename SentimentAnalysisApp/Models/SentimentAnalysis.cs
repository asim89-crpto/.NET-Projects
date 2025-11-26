using Microsoft.ML.Data;

namespace NewApp.Models
{
    public class SentimentData
    {
        [LoadColumn(0)]
        public string SentimentText { get; set; } // Renamed to SentimentText

        [LoadColumn(1), ColumnName("Label")]
        public bool Label { get; set; } // Renamed to Label, and type matches the data.
    }

    public class SentimentPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Probability { get; set; }
        public float Score { get; set; }
    }
}
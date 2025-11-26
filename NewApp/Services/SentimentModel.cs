using Microsoft.ML;
using NewApp.Models; // Assuming your models are in NewApp.Models
using static Microsoft.ML.DataOperationsCatalog;

namespace NewApp.Services
{
    public class SentimentModel
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private readonly string _yelpDataPath;

        public SentimentModel()
        {
            _mlContext = new MLContext();
            _yelpDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "yelp_labelled.txt");
            _model = LoadModel(); // Load or train the model
        }

        private ITransformer LoadModel()
        {
            if (File.Exists("SentimentModel.zip"))
            {
                return _mlContext.Model.Load("SentimentModel.zip", out var schema);
            }

            return TrainModel(); // Train if model is not found
        }

        private ITransformer TrainModel()
        {
            TrainTestData splitDataView = LoadData(_mlContext);
            var model = BuildAndTrainModel(_mlContext, splitDataView.TrainSet);
            _mlContext.Model.Save(model, splitDataView.TrainSet.Schema, "SentimentModel.zip"); //save model
            return model;
        }

        private TrainTestData LoadData(MLContext mLContext)
        {
            IDataView dataView = mLContext.Data.LoadFromTextFile<SentimentData>(_yelpDataPath);
            TrainTestData splitDataView = mLContext.Data.TrainTestSplit(dataView, 0.2);
            return splitDataView;
        }

        private ITransformer BuildAndTrainModel(MLContext mlContext, IDataView splitTrainSet)
        {
            var estimator = mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.SentimentText))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression("Label", "Features"));
            return estimator.Fit(splitTrainSet);
        }

        public SentimentPrediction PredictSentiment(string reviewContent)
        {
            if (_model == null)
            {
                return null; // Model not loaded
            }

            PredictionEngine<SentimentData, SentimentPrediction> predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);

            var sampleStatement = new SentimentData
            {
                SentimentText = reviewContent
            };

            var predictionResult = predictionEngine.Predict(sampleStatement);

            return new SentimentPrediction
            {
                Prediction = predictionResult.Prediction,
                Probability = predictionResult.Probability,
                Score = predictionResult.Score
            };
        }
    }
}
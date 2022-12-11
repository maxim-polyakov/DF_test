using Npgsql;
using System;
using System.Collections.Generic;
using Microsoft.Data.Analysis;
using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;
using System.IO;
using Microsoft.ML.Data;
using SentimentAnalysis.Core.Domain;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;

//using static Microsoft.ML..DataOperationsCatalog;

namespace DB_Bridge
{
    public class AzurePostgresCreate
    {
        static ITransformer BuildAndTrainModel(MLContext mlContext, IDataView splitTrainSet)
        {
            var estimator = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "hi", inputColumnName: nameof(SentimentData.text))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "hi"));
            Console.WriteLine("=============== Create and Train the Model ===============");
            var model = estimator.Fit(splitTrainSet);
            Console.WriteLine("=============== End of training ===============");
            Console.WriteLine();
            return model;
        }
        static void Evaluate(MLContext mlContext, ITransformer model, IDataView splitTestSet)
        {
            Console.WriteLine("=============== Evaluating Model accuracy with Test data===============");
            IDataView predictions = model.Transform(splitTestSet);
            CalibratedBinaryClassificationMetrics metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");

            Console.WriteLine();
            Console.WriteLine("Model quality metrics evaluation");
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"Auc: {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
            Console.WriteLine("=============== End of model evaluation ===============");

        }


        static DataTable GetProviderFactoryClasses()
        {
            // Retrieve the installed providers and factories.
            DataTable table = DbProviderFactories.GetFactoryClasses();

            // Display each row and column value.
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    Console.WriteLine(row[column]);
                }
            }
            return table;
        }



        static TrainTestData LoadData(MLContext mlContext, DataFrame df, string select)
        {

            IDataView dataView = mlContext.Data.TakeRows(df,df.Rows.Count);

            TrainTestData splitDataView = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            return splitDataView;
        }

        static void UseModelWithSingleItem(MLContext mlContext, ITransformer model)
        {
            PredictionEngine<SentimentData, SentimentPrediction> predictionFunction = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
            
            string txt = "This was a very bad steak";
            StringDataFrameColumn textcol = new StringDataFrameColumn("text", 0);
            textcol.Append(txt);

            SentimentData sampleStatement = new SentimentData
            {
                text = textcol
            };

            var resultPrediction = predictionFunction.Predict(sampleStatement);

            Console.WriteLine();
            Console.WriteLine("=============== Prediction Test of model with a single sample and test dataset ===============");

            Console.WriteLine();
            Console.WriteLine($"Sentiment: {resultPrediction.text} | Prediction: {(Convert.ToBoolean(resultPrediction.Prediction) ? "Positive" : "Negative")} | Probability: {resultPrediction.Probability} ");

            Console.WriteLine("=============== End of Predictions ===============");
            Console.WriteLine();
        }

        static void UseModelWithBatchItems(MLContext mlContext, ITransformer model)
        {
            string txt = "This was a horrible meal";
            string stxt = "I love this spaghetti.";
            StringDataFrameColumn textcol = new StringDataFrameColumn("text", 0);
            StringDataFrameColumn stextcol = new StringDataFrameColumn("text", 0);
            textcol.Append(txt);
            stextcol.Append(stxt);
            IEnumerable<SentimentData> sentiments = new[]
            {


                new SentimentData
                {
                    text = textcol
                },
                new SentimentData
                {
                    text = stextcol
                }
            };

            IDataView batchComments = mlContext.Data.LoadFromEnumerable(sentiments);

            IDataView predictions = model.Transform(batchComments);

            IEnumerable<SentimentPrediction> predictedResults = mlContext.Data.CreateEnumerable<SentimentPrediction>(predictions, reuseRowObject: false);

            Console.WriteLine();

            Console.WriteLine("=============== Prediction Test of loaded model with multiple samples ===============");

            foreach (SentimentPrediction prediction in predictedResults)
            {
                Console.WriteLine($"Sentiment: {prediction.text} | Prediction: {(Convert.ToBoolean(prediction.Prediction) ? "Positive" : "Negative")} | Probability: {prediction.Probability} ");
            }
            Console.WriteLine("=============== End of predictions ===============");
        }

        static void Main(string[] args)
        {
            MLContext mlContext = new MLContext();



            DB_Bridge.DB_Communication dbc = new DB_Bridge.DB_Communication();

            string select = "select * FROM train_sets.all_set_thanks" +
                       " union all " +
                       "select * FROM train_sets.all_set_none" +
                       " union all " +
                       "select * FROM train_sets.all_set_hi" +
                       " union all " +
                       "select * FROM train_sets.all_set_business" +
                       " union all " +
                       "select * FROM train_sets.all_set_weather" +
                       " order by agendaid asc";

            DataFrame df = dbc.get_data(select);

            TrainTestData splitDataView = LoadData(mlContext, df, select);

            ITransformer model = BuildAndTrainModel(mlContext, splitDataView.TrainSet);

            Evaluate(mlContext, model, splitDataView.TestSet);

            UseModelWithSingleItem(mlContext, model);

            UseModelWithBatchItems(mlContext, model);

            



            Console.WriteLine(dbc.checkcommands("фас"));
        }
    }

}

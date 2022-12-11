using Microsoft.ML.Data;
using Microsoft.Data.Analysis;
using OpenQA.Selenium.DevTools.V106.DOM;
using static System.Net.Mime.MediaTypeNames;

public class SentimentPrediction : SentimentData
{

    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    public float Probability { get; set; }

    public float Score { get; set; }
}

public class SentimentData
{
    public PrimitiveDataFrameColumn<int> id { get; set; }

    public StringDataFrameColumn text { get; set; }

    public StringDataFrameColumn agenda { get; set; }

    public Int32DataFrameColumn agendaid { get; set; }

    public Int32DataFrameColumn hi { get; set; }

    public Int32DataFrameColumn business { get; set; }

    public Int32DataFrameColumn weather { get; set; }
        
    public Int32DataFrameColumn thanks { get; set; }
        
    public Int32DataFrameColumn emotionid { get; set; }

    public Int32DataFrameColumn trash { get; set; }
}
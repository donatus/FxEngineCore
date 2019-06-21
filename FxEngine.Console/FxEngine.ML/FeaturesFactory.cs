using FxEngine.Library;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.StaticPipe;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace FxEngine.ML
{
    public class FeaturesFactory
    {
        private readonly List<IFeaturable> _featurables;

        private PipExpectation _pipExpectation;



        public FeaturesFactory()
        {
            _featurables = new List<IFeaturable>();
        }

        public void AddFeaturable(IFeaturable featurable)
        {
            _featurables.Add(featurable);
        }

        public string[] GetFeaturesName()
        {
            List<string> result = new List<string>();

            foreach(IFeaturable featurable in _featurables)
            {
                result.AddRange(featurable.GetHeaders());
            }

            return result.ToArray();
        }

        public IDictionary<string,float> GetFeatures(DateTime dateTime)
        {
            var result = new Dictionary<string,float>();

            foreach (IFeaturable featurable in _featurables)
            {
                if (!featurable.HasFeatures(dateTime))
                {
                    return null;
                }

                foreach(var value in featurable.GetFeatures(dateTime))
                {
                    result.Add(value.Key, value.Value);
                }
            }

            return result;
        }

        public void SetPredition(PipExpectation pipExpectation)
        {
            _pipExpectation = pipExpectation;
        }

        public ITransformer Fit(IEstimator<ITransformer> estimator)
        {
            Console.WriteLine($"Running {estimator.GetType().Name}");
            MLContext mlContext = new MLContext();

            DateTime from = _featurables.Min(c => c.Begin);
            DateTime to = _featurables.Max(c => c.End);
            Period period = Period.S;
            int periodCount = 5;

            List<Dictionary<string,float>> features = new List<Dictionary<string, float>>();

            for (DateTime date = from; date < to; date = date.AddPeriod(period, periodCount))
            {
                if(_featurables.All(c => c.HasFeatures(date)) && _pipExpectation._futurPip.ContainsKey(date))
                {
                    Dictionary<string, float> feature = new Dictionary<string, float>();
                    var featuresDictionnary = GetFeatures(date);

                    if(featuresDictionnary == null)
                    {
                        continue;
                    }

                    foreach (var item in featuresDictionnary)
                    {
                        feature.Add(item.Key, item.Value);
                    }
                    feature["Label"] = decimal.ToSingle(_pipExpectation._futurPip[date].Long);
                    features.Add(feature);
                }
            }

            IDataView data = new FloatsDataView(features);

            string[] featureNames = features.First().Keys.Where(c => c != "Label").ToArray();

            var pipeline = mlContext.Transforms.Concatenate("Features", featureNames)
                .Append(mlContext.Transforms.NormalizeMeanVariance("Features"))
                .AppendCacheCheckpoint(mlContext);
                //.Append(mlContext.Regression.Trainers.OnlineGradientDescent(numberOfIterations:10));

            // Create data prep transformer
            ITransformer transformData = pipeline.Fit(data);
            IDataView transformedData = transformData.Transform(data);

            //IEstimator<ITransformer> estimator = mlContext.Regression.Trainers.OnlineGradientDescent(numberOfIterations:10);

            var cvResults = mlContext.Regression.CrossValidate(transformedData, estimator, numberOfFolds: 5);


            // Apply transforms to training data
            var models =  cvResults.OrderByDescending(fold => fold.Metrics.RSquared).ToArray();

            // Get Top Model
            var topModel = models[0];

            Console.WriteLine($"\tR2:                { topModel.Metrics.RSquared}");
            Console.WriteLine($"\tLossfunction:      { topModel.Metrics.LossFunction}");
            Console.WriteLine($"\tMeanAbsoluteError: { topModel.Metrics.MeanAbsoluteError}");

            //RegressionMetrics trainedModelMetrics = mlContext.Regression.Evaluate(testDataPredictions);
            //double rSquared = trainedModelMetrics.RSquared;

            return topModel.Model;
        }




    }

    internal sealed class FloatsDataView : IDataView
    {
        private readonly IEnumerable<IDictionary<string, float>> _data;

        public FloatsDataView(IEnumerable<IDictionary<string, float>> data)
        {
            _data = data;
            var builder = new DataViewSchema.Builder();
            foreach(var name in data.First().Keys)
            {
                builder.AddColumn(name, NumberDataViewType.Single);
            }

            Schema = builder.ToSchema();
        }

        public DataViewSchema Schema { get; }
        public bool CanShuffle => false;

        public long? GetRowCount() => null;

        public DataViewRowCursor GetRowCursor(IEnumerable<DataViewSchema.Column> columnsNeeded, Random rand = null)
                => new Cursor(_data, columnsNeeded,Schema);

        public DataViewRowCursor[] GetRowCursorSet(IEnumerable<DataViewSchema.Column> columnsNeeded, int n, Random rand = null)
                => new[] { GetRowCursor(columnsNeeded, rand) };

        
    }

    internal sealed class Cursor : DataViewRowCursor
    {
        private readonly IEnumerator<IDictionary<string, float>> _data;

        private readonly IEnumerable<DataViewSchema.Column> _columnsNeeded;

        private readonly IDictionary<string, Delegate> _getters;

        private readonly IList<FloatGetter> _floatGetters;

        private bool _disposed;

        private long _position;

        public override long Position => _position;

        public override long Batch => 0;

        public override DataViewSchema Schema { get; }

        public Cursor(IEnumerable<IDictionary<string, float>> data, IEnumerable<DataViewSchema.Column> columnsNeeded, DataViewSchema schema)
        {
            _data = data.GetEnumerator();
            _getters = new Dictionary<string, Delegate>();
            _floatGetters = new List<FloatGetter>();
            foreach (var name in data.First().Keys)
            {
                var floatgetter = new FloatGetter(name, _data);
                _floatGetters.Add(floatgetter);
                _getters.Add(name, (ValueGetter<float>)floatgetter.Getter);
            }
            Schema = schema;
            _columnsNeeded = columnsNeeded;
            _position = -1;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _data.Dispose();
                _position = -1;
            }
            _disposed = true;
            base.Dispose(disposing);
        }

        public override bool MoveNext()
        {
            if (_disposed)
                return false;
            if (_data.MoveNext())
            {
                _position++;
                return true;
            }
            Dispose();
            return false;
        }

        private void IdGetterImplementation(ref DataViewRowId id)
                    => id = new DataViewRowId((ulong)_position, 0);

        public override ValueGetter<DataViewRowId> GetIdGetter()
                    => IdGetterImplementation;

        public override bool IsColumnActive(DataViewSchema.Column column)
        {
            return _getters.ContainsKey(column.Name);
        }

        public override ValueGetter<TValue> GetGetter<TValue>(DataViewSchema.Column column)
        {
            return (ValueGetter<TValue>)_getters[column.Name];
        }
    }

    public class FloatGetter
    {
        private readonly string _columnName;

        private readonly IEnumerator<IDictionary<string, float>> _enumerator;

        public FloatGetter(string columnName, IEnumerator<IDictionary<string, float>> enumerator)
        {
            _columnName = columnName;
            _enumerator = enumerator;
        }

        public void Getter(ref float value) => value = _enumerator.Current[_columnName];

    }



}

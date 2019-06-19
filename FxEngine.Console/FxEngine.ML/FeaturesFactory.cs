using FxEngine.Library;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FxEngine.ML
{
    public class FeaturesFactory
    {
        private readonly List<IFeaturable> _featurables;

        private PipExpectation _pipExpectation;



        public FeaturesFactory()
        {

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

        public float[] GetFeatures(DateTime dateTime)
        {
            List<float> result = new List<float>();

            foreach (IFeaturable featurable in _featurables)
            {
                if (featurable.HasFeatures(dateTime))
                {
                    return null;
                }
                result.AddRange(featurable.GetFeatures(dateTime));
            }

            return result.ToArray();
        }

        public void AddPredition(PipExpectation pipExpectation)
        {
            _pipExpectation = pipExpectation;
        }

        public ITransformer Fit(MLContext mlContext)
        {
            //mlContext.Transforms.CopyColumns
            //throw new NotImplementedException();
            return null;
        }

        public DataViewSchema Schema { get; }
        public bool CanShuffle => false;

        public long? GetRowCount() => null;

        public DataViewRowCursor GetRowCursor(IEnumerable<DataViewSchema.Column> columnsNeeded, Random rand = null)
            => new Cursor(this, columnsNeeded.Any(c => c.Index == 0), columnsNeeded.Any(c => c.Index == 1));

        public DataViewRowCursor[] GetRowCursorSet(IEnumerable<DataViewSchema.Column> columnsNeeded, int n, Random rand = null)
            => new[] { GetRowCursor(columnsNeeded, rand) };

    }

    internal class Cursor : DataViewRowCursor
    {
        private bool _disposed;
        private long _position;

        private FeaturesFactory _featuresFactory;

        public override long Position => _position;
        public override long Batch => 0;
        public override DataViewSchema Schema { get; }

        public Cursor(FeaturesFactory featuresFactory, bool wantsLabel, bool wantsText)
        {
            _featuresFactory = featuresFactory;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _position = -1;
            }
            _disposed = true;
            base.Dispose(disposing);
        }

        public override ValueGetter<TValue> GetGetter<TValue>(DataViewSchema.Column column)
        {
            throw new NotImplementedException();
        }

        public override ValueGetter<DataViewRowId> GetIdGetter()
        {
            throw new NotImplementedException();
        }

        public override bool IsColumnActive(DataViewSchema.Column column)
        {
            throw new NotImplementedException();
        }

        public override bool MoveNext()
        {
            throw new NotImplementedException();
        }
    }
}

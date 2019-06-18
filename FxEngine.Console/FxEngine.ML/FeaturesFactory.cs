using FxEngine.Library;
using Microsoft.ML;
using System;
using System.Collections.Generic;

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
            mlContext.Transforms.CopyColumns
            throw new NotImplementedException();
        }

    }
}

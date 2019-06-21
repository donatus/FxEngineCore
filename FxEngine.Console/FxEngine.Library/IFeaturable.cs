using System;
using System.Collections.Generic;
using System.Text;

namespace FxEngine.Library
{
    public interface IFeaturable
    {
        DateTime Begin { get; }

        DateTime End { get; }

        Period Period { get; }
        int PeriodCount { get; }

        IEnumerable<string> GetHeaders();

        IDictionary<string,float> GetFeatures(DateTime dateTime);

        bool HasFeatures(DateTime dateTime);
    }
}

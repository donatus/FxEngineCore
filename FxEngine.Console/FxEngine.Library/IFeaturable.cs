using System;
using System.Collections.Generic;
using System.Text;

namespace FxEngine.Library
{
    public interface IFeaturable
    {
        IEnumerable<string> GetHeaders();
        IEnumerable<float> GetFeatures(DateTime dateTime);
        bool HasFeatures(DateTime dateTime);
    }
}

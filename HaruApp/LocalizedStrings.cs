using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HaruApp
{
    public class LocalizedStrings
    {
        private static readonly Resources.AppResources _localizedResources = new Resources.AppResources();

        public Resources.AppResources LocalizedResources
        {
            get { return _localizedResources; }
        }
    }
}

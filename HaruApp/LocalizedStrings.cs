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

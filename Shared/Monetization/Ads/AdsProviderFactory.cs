namespace BonGames.Monetization
{
    public static class AdsProviderFactory
    {
        private static IAdsProvider s_adsProvider;

        public static IAdsProvider GetAdsProvider() 
        {
            if (s_adsProvider == null)
            {
#if ADS_MONETIZATION_LEVELPLAY
                s_adsProvider = new LevelPlayAdsProvider();
                s_adsProvider.Initialize();
#endif
            }
            return s_adsProvider;
        }
    }
}
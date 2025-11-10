namespace BonGames.Monetization
{
    [System.Flags]
    public enum AdsUnit
    {
        Unavaiable = 0,
        Banner = 1 << 1,
        Rewarded = 1 << 2,
    }

    public interface IAdsProvider : IInitializable
    {
        public bool Show(AdsUnit unit, string placementId);
        public void Hide(AdsUnit unit);
        public bool TestAdIntegration(AdsUnit unit, string placementId);
    }
}

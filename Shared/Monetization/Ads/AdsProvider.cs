namespace BonGames.Monetization
{
    public class AdsProvider : IAdsProvider
    {
        public bool IsInitialized { get; protected set; }
        protected AdsProvider() { }

        public virtual void Initialize()
        {
            
        }

        public virtual bool Show(AdsUnit unit, string placementId) { return false; }
        public virtual void Hide(AdsUnit unit) { }

        public virtual bool TestAdIntegration(AdsUnit unit, string testId)
        { 
            return Show(unit, testId); 
        }
    }
}
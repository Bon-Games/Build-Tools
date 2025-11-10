using MobileConsole;

namespace BonGames.Monetization
{
    [ExecutableCommand(name = "Ads/Test")]
    [UnityEngine.Scripting.Preserve]
    public class AdsTest : Command
    {
        public string TestId;

        public override void Execute()
        {
            AdsProviderFactory.GetAdsProvider().TestAdIntegration(AdsUnit.Rewarded, TestId);
        }
    }
}
// #define ADS_MONETIZATION_LEVELPLAY
#if ADS_MONETIZATION_LEVELPLAY
using System;
using System.Collections.Generic;
using Unity.Services.LevelPlay;

namespace BonGames.Monetization
{
    public class LevelPlayAdsProvider : AdsProvider
    {
        private ILevelPlayRewardedAd _rewardedAd;
        private ILevelPlayBannerAd _bannerAd;
        private AdsUnit _loadedUnits;

        public LevelPlayAdsProvider()
        {
            _loadedUnits = AdsUnit.Unavaiable;
            _rewardedAd = new LevelPlayRewardedAd(adUnitId: "3hl7o91ncl21s20k");
            _bannerAd = new LevelPlayBannerAd(adUnitId: "q93r7fy9tpmtxc8i");
        }

        public override void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;

            LevelPlay.OnInitSuccess += OnSdkInitializationCompleted;
            LevelPlay.OnInitFailed += OnSdkInitializationFailed;

            _rewardedAd.OnAdLoaded += (info) => _loadedUnits |= AdsUnit.Rewarded;
            _bannerAd.OnAdLoaded += (info) => _loadedUnits |= AdsUnit.Banner;

            // This is Android App key
            LevelPlay.Init(appKey: "241bf67b5", null);
            LevelPlay.SetMetaData("is_test_suite", "enable");

        }

        private void OnSdkInitializationCompleted(LevelPlayConfiguration configuration)
        {
            _rewardedAd.LoadAd();
            _bannerAd.LoadAd();
            Logger.LogE(nameof(LevelPlayAdsProvider), "Initialization completed");
        }

        private void OnSdkInitializationFailed(LevelPlayInitError error)
        {
            IsInitialized = false;
            Logger.LogE(nameof(LevelPlayAdsProvider), error.ToString());
        }

        public override bool Show(AdsUnit unit, string placementId)
        {
            if (!_loadedUnits.HasFlag(unit)) return false;

            switch (unit)
            {
                case AdsUnit.Rewarded:
                    _rewardedAd.ShowAd(string.IsNullOrEmpty(placementId) ? "item_vc_energy" : placementId);
                    return true;
                case AdsUnit.Banner:
                    _bannerAd.ShowAd();
                    return true;
            }
            return false;
        }
        public override void Hide(AdsUnit unit)
        {
            switch (unit)
            {
                case AdsUnit.Banner:
                    _bannerAd.HideAd();
                    break;
            }
        }
        public override bool TestAdIntegration(AdsUnit unit, string testId)
        {
            LevelPlay.LaunchTestSuite();
            return true;
        }
    }
}
#endif
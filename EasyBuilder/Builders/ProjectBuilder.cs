using BonGames.Tools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BonGames.EasyBuilder.Enum;
using UnityEditor;
using BonGames.EasyBuilder.Argument;

namespace BonGames.EasyBuilder
{
    public abstract partial class ProjectBuilder : IProjectBuilder
    {
        public EAppTarget AppTarget { get; private set; }

        public BuildTarget BuildTarget { get; private set; }

        public EEnvironment Environment { get; private set; }

        protected string BuildInformationDirectory => BuilderUtils.BuildInformationDirectory();

        protected virtual List<string> PlatformSpecifiedSymbols { get; private set; } = new List<string>()
        {
            BuildDefines.EnableIL2CppScriptBackend,
        };

        protected BuildVersion Version { get; } = new BuildVersion();

        public BuildPlayerOptions BuildPlayerOptions { get; set; }

        public ProjectBuilder(EAppTarget appTarget, BuildTarget buildTarget, EEnvironment environment)
        {
            AppTarget = appTarget;
            BuildTarget = buildTarget;
            Environment = environment;
        }


        public UnityEditor.Build.Reporting.BuildReport Build()
        {
            // Switch to build target
            if (!ProjectSwitcher.SwitchTo(AppTarget, Environment, BuildTarget))
            {
                BonGames.Tools.Domain.ThrowIf(true, $"Failed to switch AppTarget:{AppTarget} BuildTarget:{BuildTarget} Environment:{Environment}");
            }

            Prepare();
            string buildProfileFilePath = null;
            if (!UnityEngine.Application.isBatchMode)
            {
                // In Editor mode, Uses .args.[Environment] as default
                buildProfileFilePath = BuilderUtils.GetActiveBuildProfileFilePath(Environment);
            }
            BonGames.CommandLine.ArgumentsResolver.Load(buildProfileFilePath);

            // Clean up current symbols, set editor symbols to match the build target
            ProjectSwitcher.SetScriptSymbolsTo(AppTarget, Environment);

            // Create build options
            Version.LoadVersion();
            BuildPlayerOptions = CreateBuildPlayerOptions();
            Domain.ThrowIf(!BuildPlayerOptions.scenes.Any(), "There's no scene inlcuded in the build, ensure you have at least 1 scene in either ProjectSettings or passed argument");

            // Pre Build
            PreBuildProcessors preProcessors = BuilderUtils.GetPreProcessors(BuildArguments.GetPreProcessorsBuild(), Environment);
            if (preProcessors != null)
            {
                preProcessors.Execute(this);
            }

            // Setup general product info
            SetProductInformation();

            // Behide the scene informations
            SetupInternally();

            // Build DLC
            if (BuildArguments.IsDlcBuildEnable())
            {
                string dlcDestination = BuildArguments.GetDlcDestination();
                string profileName = BuildArguments.GetDlcProfileName("Remote") ?? string.Empty;
                bool dlcBuildResult = false;
                string report;
                IDlcBuilder dlcBuilder = DlcBuilder.CreateBuilder(this.BuildTarget, this.Environment, profileName, dlcDestination);
                try
                {
                    dlcBuildResult = dlcBuilder.Build(out report);
                }
                catch (System.Exception e)
                {
                    dlcBuildResult = false;
                    report = e.ToString();
                }
                BonGames.Tools.Domain.ThrowIf(!dlcBuildResult, $"Dlc build failed:\n{report}");
            }

            // Player Build
            if (BuildArguments.IsPlayerBuildEnable())
            {
                UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(BuildPlayerOptions);

                if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    // Post Build
                    PostBuildProcessors postProcessors = BuilderUtils.GetPostProcessors(BuildArguments.GetPostProcessorsBuild(), Environment);
                    if (postProcessors != null)
                    {
                        string builtLocation = System.IO.Directory.Exists(report.summary.outputPath) ? report.summary.outputPath : System.IO.Path.GetDirectoryName(report.summary.outputPath);
                        postProcessors.Execute(report, this, builtLocation);
                    }
                }
                else
                {
                    BonGames.Tools.Domain.LogW("Skip Post Build Process due to build failed");
                }
                return report;
            }
            else
            {
                return default;
            }
        }

        protected virtual void Prepare()
        {

        }
        protected void SetProductInformation()
        {
            SetProductName();
            UpdateAppVersion();
            SignApp();
        }

        protected virtual void SetupInternally()
        {

        }

        protected virtual void UpdateAppVersion()
        {
            // Set all in once here, reduce complexity, if you want to set by your own logic, lets override this method
            PlayerSettings.bundleVersion = Version.BundleVersion;
            PlayerSettings.Android.bundleVersionCode = Version.Build;
            PlayerSettings.iOS.buildNumber = $"{Version.Build}";
            PlayerSettings.WSA.packageVersion = new System.Version(Version.Major, Version.Minor, Version.Build, Version.Revision);
        }

        protected virtual void SetProductName()
        {
            string product = BuildArguments.GetProductName();
            if (!string.IsNullOrEmpty(product))
            {
                PlayerSettings.productName = product;
            }

            // Setting for bundle id
            string bundleId = BuildArguments.GetBundleId();
            // If the bundle id is passed as an argument
            if (!string.IsNullOrEmpty(bundleId))
            {
                BuildTargetGroup buildGroup = BuilderUtils.GetBuildTargetGroup(BuildTarget);
                PlayerSettings.SetApplicationIdentifier(buildGroup, bundleId);
                EasyBuilder.LogI($"Set BundleId: {bundleId} for BuildTarget: {BuildTarget} BuildGroup: {buildGroup}");
            }
        }

        protected virtual void SignApp() { }

        protected virtual BuildPlayerOptions CreateBuildPlayerOptions()
        {
            BuildPlayerOptions buildPlayerOptions = GetDefaultBuildPlayerOptions();
            buildPlayerOptions.locationPathName = BuilderUtils.GetBuildLocationWithExecFile(AppTarget, BuildTarget, Environment, Version);
            buildPlayerOptions.targetGroup = BuilderUtils.GetBuildTargetGroup(BuildTarget);
            buildPlayerOptions.subtarget = BuilderUtils.GetSubBuildTarget(AppTarget, BuildTarget);
            buildPlayerOptions.scenes = GetActiveScenes();
            buildPlayerOptions.target = BuildTarget;
            OnBuildPlayerOptionCreate(ref buildPlayerOptions);

            return buildPlayerOptions;
        }

        protected virtual void OnBuildPlayerOptionCreate(ref BuildPlayerOptions ops)
        {

        }

        public BuildPlayerOptions GetDefaultBuildPlayerOptions()
        {
            BuildPlayerOptions buildPlayerOptions = BuilderUtils.GetDefaultBuildPlayerOptions(AppTarget, Environment);

            List<string> symbols = new List<string>(buildPlayerOptions.extraScriptingDefines);
            if (PlatformSpecifiedSymbols != null && PlatformSpecifiedSymbols.Count > 0)
            {
                symbols.AddRange(PlatformSpecifiedSymbols);
            }
            symbols.AddRange(BuildArguments.GetAdditionalSymbols());

            buildPlayerOptions.extraScriptingDefines = symbols.ToArray();
            return buildPlayerOptions;
        }

        protected virtual string[] GetActiveScenes()
        {
            string[] allScenes = AssetDatabase.FindAssets("t:scene");
            List<string> activeScenes = new List<string>();
            string[] customScenePaths = BuildArguments.GetScenePaths();
            if (customScenePaths.Length > 0)
            {
                activeScenes.AddRange(customScenePaths.Where(path => allScenes.Contains(path)));
            }
            else
            {
                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                for (int i = 0; i < scenes.Length; i++)
                {
                    EditorBuildSettingsScene sceneSettings = scenes[i];
                    if (sceneSettings.enabled)
                    {
                        activeScenes.Add(sceneSettings.path);
                    }
                }
            }
            return activeScenes.ToArray();
        }
    }
}

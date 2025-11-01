using Conditional = System.Diagnostics.ConditionalAttribute;

namespace BonGames
{
    public class Logger
    {
        private const string UnityEditorSymbol = "UNITY_EDITOR";
        private const string DevelopmentBuildSymbol = "DEVELOPMENT_BUILD";
        private const string EnableLogSymbol = "ENABLE_LOG";

        static Logger()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || ENABLE_LOG
            UnityEngine.Debug.unityLogger.logEnabled = true;
#else
            UnityEngine.Debug.unityLogger.logEnabled = false;
#endif
        }

        [Conditional(UnityEditorSymbol), Conditional(DevelopmentBuildSymbol), System.Diagnostics.Conditional(EnableLogSymbol)]
        public static void LogI(object obj)
        {
            UnityEngine.Debug.Log(obj);
        }

        [Conditional(UnityEditorSymbol), Conditional(DevelopmentBuildSymbol), System.Diagnostics.Conditional(EnableLogSymbol)]
        public static void LogW(object obj)
        {
            UnityEngine.Debug.LogWarning(obj);
        }

        [Conditional(UnityEditorSymbol), Conditional(DevelopmentBuildSymbol), System.Diagnostics.Conditional(EnableLogSymbol)]
        public static void LogE(object obj)
        {
            UnityEngine.Debug.LogError(obj);
        }

        [Conditional(UnityEditorSymbol), Conditional(DevelopmentBuildSymbol), System.Diagnostics.Conditional(EnableLogSymbol)]
        public static void Assert(bool condition, object obj)
        {
            UnityEngine.Debug.Assert(condition, obj);
        }

        [Conditional(UnityEditorSymbol), Conditional(DevelopmentBuildSymbol), System.Diagnostics.Conditional(EnableLogSymbol)]
        public static void Exception(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        [Conditional(UnityEditorSymbol), Conditional(DevelopmentBuildSymbol), System.Diagnostics.Conditional(EnableLogSymbol)]
        public static void LogI(string tag, object obj)
        {
            LogI($"[{tag}] {obj}");
        }

        [Conditional(UnityEditorSymbol), Conditional(DevelopmentBuildSymbol), System.Diagnostics.Conditional(EnableLogSymbol)]
        public static void LogW(string tag, object obj)
        {
            LogW($"[{tag}] {obj}");
        }

        [Conditional(UnityEditorSymbol), Conditional(DevelopmentBuildSymbol), System.Diagnostics.Conditional(EnableLogSymbol)]
        public static void LogE(string tag, object obj)
        {
            LogE($"[{tag}] {obj}");
        }

        [Conditional(UnityEditorSymbol), Conditional(DevelopmentBuildSymbol), System.Diagnostics.Conditional(EnableLogSymbol)]
        public static void Assert(string tag, bool condition, object obj)
        {
            Assert(condition, $"[{tag}] {obj}");
        }

        [Conditional(UnityEditorSymbol), Conditional(DevelopmentBuildSymbol), System.Diagnostics.Conditional(EnableLogSymbol)]
        public static void Exception(string tag, System.Exception exception)
        {
            Exception(new System.Exception($"[{tag}]\n{exception.Message}"));
        }
    }
}

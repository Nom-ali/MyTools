//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEditor.Build;

//[InitializeOnLoad]
//public static class DefineSymbolManager
//{
//    private const string SYMBOL = "MY_TOOLS";

//    static DefineSymbolManager()
//    {
//        AddDefineSymbol(SYMBOL);
//    }

//    private static void AddDefineSymbol(string symbol)
//    {
//        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
//        {
//            if (group == BuildTargetGroup.Unknown) continue;

//            NamedBuildTarget named = NamedBuildTarget.FromBuildTargetGroup(group);
//            string defines = PlayerSettings.GetScriptingDefineSymbols(named);
//            if (!defines.Contains(symbol))
//            {
//                defines = string.IsNullOrEmpty(defines) ? symbol : defines + ";" + symbol;
//                PlayerSettings.SetScriptingDefineSymbols(named, defines);
//                UnityEngine.Debug.Log($"[MyPackage] Added scripting define symbol: {symbol} for {group}");
//            }
//        }
//    }
//}
//#endif

#if UNITY_EDITOR

using MyBox;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EnumData", menuName = "RNA/Enums/Enum Data")]
public class EnumGenerator : ScriptableObject
{
    public string ScriptName = "ENUMS";
    public string EnumName = "Default";
    public string[] enumValues;
    public bool modifyExisting = false;  // New flag to choose between creating or modifying
    public bool MarkAsFlag = false;

    [MyBox.ButtonMethod(MyBox.ButtonMethodDrawOrder.AfterInspector, nameof(ScriptName), nameof(EnumName))]
    private void GenerateEnum()
    {
        if (ScriptName.IsNullOrEmpty() || EnumName.IsNullOrEmpty() || enumValues.Length <= 0)
        {
            Debug.LogError("Something is missing here.", this);
            return;
        }

        string path = "Assets/_Game_Assets/Scripts/RNA/Enums";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string enumFilePath = Path.Combine(path, $"{ScriptName}.cs");

        if (!modifyExisting || !File.Exists(enumFilePath))  // Create new if modifying is off or file doesn't exist
        {
            // Create a new enum script
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"public static class {ScriptName}");
            sb.AppendLine("{");
            if (MarkAsFlag)
                sb.AppendLine("    [System.Flags, System.Serializable]");
            else
                sb.AppendLine("    [System.Serializable]");
            sb.AppendLine($"    public enum {EnumName}");
            sb.AppendLine("    {");

            if (MarkAsFlag)
            {
                for (int i = 0; i < enumValues.Length; i++)
                {
                    string value = enumValues[i];
                    if (i == 0)
                        sb.AppendLine($"        {SanitizeEnumName(value)}    =    0,");
                    else
                        sb.AppendLine($"        {SanitizeEnumName(value)}    =    1    << {i - 1},");
                }
            }
            else
            {
                foreach (string value in enumValues)
                {
                    sb.AppendLine($"        {SanitizeEnumName(value)},");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(enumFilePath, sb.ToString());
        }
        else
        {
            // Modify existing enum script
            var existingContent = File.ReadAllText(enumFilePath);
            var enumStart = $"public enum {EnumName}";
            if (existingContent.Contains(enumStart))
            {
                Debug.LogError("Enum already exists in the file.");
                return;
            }

            // Locate the place to add new enum
            StringBuilder sb = new StringBuilder(existingContent);

            StringBuilder sb2 = new StringBuilder();
            
            sb2.AppendLine("\n");
            if (MarkAsFlag)
                sb2.AppendLine("    [System.Flags, System.Serializable]");
            else
                sb2.AppendLine("    [System.Serializable]");
            sb2.AppendLine($"    public enum {EnumName}");
            sb2.AppendLine("    {");
           
            if (MarkAsFlag)
            {
                for (int i = 0; i < enumValues.Length; i++)
                {
                    string value = enumValues[i];
                    if (i == 0)
                        sb2.AppendLine($"        {SanitizeEnumName(value)}    =    0,");
                    else
                        sb2.AppendLine($"        {SanitizeEnumName(value)}    =    1    << {i - 1},");
                }
            }
            else
            {
                foreach (string value in enumValues)
                {
                    sb2.AppendLine($"        {SanitizeEnumName(value)},");
                }
            }

            sb2.AppendLine("    }");

            sb.Insert(existingContent.LastIndexOf('}') - 1, $"\n    {sb2}");
            File.WriteAllText(enumFilePath, sb.ToString());
        }

        AssetDatabase.Refresh();
    }

    private string SanitizeEnumName(string name)
    {
        // Ensure the enum names are valid identifiers
        return System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
    }
}
#endif
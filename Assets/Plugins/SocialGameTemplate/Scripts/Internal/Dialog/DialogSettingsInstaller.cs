using System;
using System.Linq;
using SocialGame.Dialog;
using UnityEngine;
using UnityExtensions;
using Zenject;
#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
#endif

namespace SocialGame.Internal.Dialog
{
    [CreateAssetMenu(fileName = "DialogSettings", menuName = "Installers/DialogSettings")]
    public sealed class DialogSettingsInstaller : ScriptableObjectInstaller<DialogSettingsInstaller>
    {
        [SerializeField] private DialogSettings _settings = null;

        public override void InstallBindings()
        {
            Container.BindInstance(_settings).AsSingle();
        }

        #if UNITY_EDITOR
        public void OnValidate()
        {
            if (EditorApplication.isPlaying)
                return;
            
            string fileName = Path.Combine(ProjectModel.RootPath, "Scripts/Dialog/DialogType.cs");
            string filePath = Path.Combine(Application.dataPath, fileName);
            
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("// this file was auto-generated.");
            builder.AppendLine("namespace SocialGame.Dialog");
            builder.AppendLine("{");
            builder.AppendLine("    public enum DialogType");
            builder.AppendLine("    {");
            builder.AppendLine("        Sample = -1,");
            
            foreach (var prefab in _settings.Prefabs.Where(x => x != null))
            {
                builder.AppendLineFormat("        {0},", prefab.name.Replace("Dialog", ""));
            }
            
            builder.AppendLine("    }");
            builder.AppendLine("}");

            string text = builder.ToString();
            if (File.Exists(filePath))
            {
                if (File.ReadAllText(filePath) == text)
                    return;
                
                if (Provider.isActive)
                    Provider.Checkout(Path.Combine("Assets", fileName), CheckoutMode.Asset).Wait();
            }

            File.WriteAllText(filePath, text, Encoding.UTF8);
            
            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);

            Debug.unityLogger.Log(GetType().Name, "auto-generated DialogType");
        }
        #endif
    }
}

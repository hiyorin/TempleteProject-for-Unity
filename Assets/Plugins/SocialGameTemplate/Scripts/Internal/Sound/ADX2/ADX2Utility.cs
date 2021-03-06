#if SGT_ADX2
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace SocialGame.Internal.Sound.ADX2
{
    internal static class ADX2Utility
    {
        public static IObservable<byte[]> LoadAcfFile(string fileName)
        {
            string filePath = string.Format("{0}/{1}", Application.streamingAssetsPath, fileName);
            IObservable<byte[]> result = null;
            
#if UNITY_ANDROID && !UNITY_EDITOR
            var www = new WWW(filePath);
            return www.ToObservable().Select(_ => www.bytes);
#else
            result = Observable
                .Start(() => File.ReadAllBytes(filePath))
                .ObserveOnMainThread();
#endif
            return result;
        }
        
        public static IObservable<CriAtomCueSheet> AddCueSheet(CriAtomCueSheet cueSheet)
        {
            if (cueSheet == null)
                throw new ArgumentException();

            if (string.IsNullOrEmpty(cueSheet.name))
                return Observable.Return((CriAtomCueSheet)null);
            
            return Observable
                .Return(CriAtom.AddCueSheet(cueSheet.name, cueSheet.acbFile, cueSheet.awbFile))
                .SelectMany(x => Observable
                    .EveryUpdate()
                    .Where(_ => !x.IsLoading)
                    .Select(_ => x))
                .First();
        }
        
        public static bool RemoveCueSheet(CriAtomCueSheet cueSheet)
        {
            if (cueSheet == null || string.IsNullOrEmpty(cueSheet.name))
                return false;

            CriAtom.RemoveCueSheet(cueSheet.name);
            return true;
        }

        public static IEnumerable<string> GetCueNameList(string cueSheetName)
        {
            var acb = CriAtom.GetAcb(cueSheetName);
            var result = acb.GetCueInfoList().Select(x => x.name);            
            return result;
        }
        
#if UNITY_EDITOR
        public static void Initialize()
        {
            CriAtomConfig atomConfig = new CriAtomConfig();
            CriAtomPlugin.SetConfigParameters(
                Math.Max(atomConfig.maxVirtualVoices, CriAtomPlugin.GetRequiredMaxVirtualVoices(atomConfig)),
                atomConfig.maxVoiceLimitGroups,
                atomConfig.maxCategories,
                atomConfig.standardVoicePoolConfig.memoryVoices,
                atomConfig.standardVoicePoolConfig.streamingVoices,
                atomConfig.hcaMxVoicePoolConfig.memoryVoices,
                atomConfig.hcaMxVoicePoolConfig.streamingVoices,
                atomConfig.outputSamplingRate,
                atomConfig.asrOutputChannels,
                atomConfig.usesInGamePreview,
                atomConfig.serverFrequency,
                atomConfig.maxParameterBlocks,
                atomConfig.categoriesPerPlayback,
                atomConfig.maxBuses,
                false);
            CriAtomPlugin.SetConfigAdditionalParameters_PC(
                atomConfig.pcBufferingTime
            );
            CriAtomPlugin.InitializeLibrary();
        }

        public static void Dispose()
        {
            CriAtomPlugin.FinalizeLibrary();
        }
        
        public static CriAtomEx.CueInfo[] GetCueInfoList(CriAtomCueSheet cueSheet)
        {
            if (string.IsNullOrEmpty(cueSheet.name))
                return null;

            CriAtomEx.CueInfo[] result = null;
            
            while (!CriFsPlugin.isInitialized || !CriAtomPlugin.isInitialized)
            {
                Debug.Log("Sleep");
                Thread.Sleep(1000);
            }

            try
            {
                var acb = CriAtomExAcb.LoadAcbFile(null,
                    Path.Combine(Application.streamingAssetsPath, cueSheet.acbFile),
                    Path.Combine(Application.streamingAssetsPath, cueSheet.awbFile));
                if (acb != null)
                {
                    result = acb.GetCueInfoList();
                    acb.Dispose();
                }
            }
            catch (Exception e)
            {
                Debug.unityLogger.LogException(e);
            }

            return result;
        }
#endif
    }
}
#endif

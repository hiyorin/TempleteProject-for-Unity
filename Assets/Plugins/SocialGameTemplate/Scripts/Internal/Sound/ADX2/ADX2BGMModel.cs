#if SGT_ADX2
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using UnityObject = UnityEngine.Object;

namespace SocialGame.Internal.Sound.ADX2
{
    internal sealed class ADX2BGMModel : IInitializable, IDisposable, ISoundModel
    {
        [Inject] private IBGMIntent _intent = null;

        [Inject] private ISoundVolumeIntent _volumeIntent = null;
        
        [Inject] private ADX2BGMSettings _settings = null;
        
        private readonly BoolReactiveProperty _initialized = new BoolReactiveProperty();

        private readonly ReactiveProperty<CriAtomSource> _source = new ReactiveProperty<CriAtomSource>();
        
        private readonly Dictionary<string, string> _cueSheetDictionary = new Dictionary<string, string>();
        
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        
        void IInitializable.Initialize()
        {
            // initialize
            Observable.EveryUpdate()
                .Where(_ => CriWareInitializer.IsInitialized())
                .First()
                .SelectMany(_ => ADX2Utility.AddCueSheet(_settings.BuiltInCueSheet)
                    .First()
                    .Where(x => x != null))
                .Subscribe(x =>
                {
                    // add cue list
                    var cueNameList = ADX2Utility.GetCueNameList(x.name);
                    foreach (var cueName in cueNameList)
                        _cueSheetDictionary.Add(cueName, x.name);

                    // create sound source
                    var source = new GameObject("BGM").AddComponent<CriAtomSource>();
                    source.use3dPositioning = false;
                    source.loop = true;
                    source.player.AttachFader();

                    _source.Value = source;
                })
                .AddTo(_disposable);

            Observable.Merge(
                    _intent.OnPlayAsObservable().Select(x => x.ToString()),
                    _intent.OnPlayForNameAsObservable())
                .SelectMany(x => _initialized
                    .Where(y => y)
                    .First()
                    .Select(_ => x))
                .Subscribe(x =>
                {
                    string cueSheet;
                    if (!_cueSheetDictionary.TryGetValue(x, out cueSheet))
                    {
                        Debug.unityLogger.LogError(GetType().Name, $"{x} is not found.");
                        return;
                    }

                    _source.Value.cueSheet = cueSheet;
                    _source.Value.Play(x.ToString());
                })
                .AddTo(_disposable);

            _intent.OnStopAsObservable()
                .Subscribe(_ => _source.Value.Stop())
                .AddTo(_disposable);

            // volume
            _volumeIntent.OnMasterVolumeAsObservable()
                .SelectMany(x => _initialized
                    .Where(y => y)
                    .First()
                    .Select(_ => x))
                .Subscribe(x => _source.Value.volume = x)
                .AddTo(_disposable);
            
            _volumeIntent.OnBGMVolumeAsObservable()
                .SelectMany(x => _initialized
                    .Where(y => y)
                    .First()
                    .Select(_ => x))
                .Do(_ => Debug.Log(CriAtom.GetCategoryVolume(_settings.CategoryVolumeName)))
                .Subscribe(x => CriAtom.SetCategoryVolume(_settings.CategoryVolumeName, x))
                .AddTo(_disposable);
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
        
        #region ISoundModel implementation
        IObservable<Unit> ISoundModel.OnInitializeAsObservable()
        {
            return _initialized
                .Where(x => x)
                .AsUnitObservable();
        }
        
        IObservable<Transform> ISoundModel.OnAddObjectAsObservable()
        {
            return _source
                .Where(x => x != null)
                .Select(x => x.transform);
        }
        #endregion
    }
}
#endif
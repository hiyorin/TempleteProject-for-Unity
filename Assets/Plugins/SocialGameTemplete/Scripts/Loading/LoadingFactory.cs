﻿using UnityEngine;
using Zenject;

namespace SocialGame.Loading
{
    public sealed class LoadingFactory
    {
        [Inject] private DiContainer _container = null;

        [Inject] private LoadingSettings _settings = null;

        [Inject] private SampleLoading _samplePrefab = null;

        public GameObject Create(LoadingType type)
        {
            if (type == LoadingType.Sample)
                return _container.InstantiatePrefab(_samplePrefab);
            else
                return _container.InstantiatePrefab(_settings.Prefabs[(int)type]);
        }
    }
}

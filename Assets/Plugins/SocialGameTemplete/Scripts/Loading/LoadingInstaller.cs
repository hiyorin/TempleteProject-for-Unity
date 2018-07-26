﻿using System;
using UnityEngine;
using Zenject;

namespace SocialGame.Loading
{
    public sealed class LoadingInstaller : MonoInstaller<LoadingInstaller>
    {
        [SerializeField] private SampleLoading _sample = null;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LoadingModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<LoadingFactory>().AsSingle();
            Container.BindInstance(_sample).AsSingle();
        }
    }
}
﻿using System;
using UnityEngine;
using Zenject;

namespace SocialGame.Toast
{
    public sealed class ToastInstaller : MonoInstaller<ToastInstaller>
    {
        [SerializeField] private SampleToast _sample = null;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ToastModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<ToastFactory>().AsSingle();
            Container.BindInstance(_sample).AsSingle();
        }
    }
}

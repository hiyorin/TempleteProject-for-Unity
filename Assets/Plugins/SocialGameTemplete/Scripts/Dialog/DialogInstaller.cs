﻿using UnityEngine;
using Zenject;

namespace SocialGame.Dialog
{
    public class DialogInstaller : MonoInstaller<DialogInstaller>
    {
        [SerializeField] private SampleDialog _sample = null;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DialogModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<DialogFactory>().AsSingle();
            Container.BindInstance(_sample).AsSingle();
        }
    }
}

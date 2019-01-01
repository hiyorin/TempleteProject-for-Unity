﻿using System;
using SocialGame.Dialog;
using UnityEngine;
using UnityEngine.UI;
using UnityExtensions;
using UniRx;
using DG.Tweening;

namespace SocialGame.Internal.Dialog.Builtin
{
    internal sealed class SampleDialog : MonoBehaviour, IDialog
    {
        [SerializeField] private Text _message = null;

        [SerializeField] private Button _okButton = null;

        private void Start()
        {
            transform.localScale = Vector3.zero;
        }

        #region IDialog implementation
        IObservable<Unit> IDialog.OnOpenAsObservable(float defaultDuration)
        {
            return transform
                .DOScale(Vector3.one, defaultDuration)
                .OnCompleteAsObservable();
        }

        IObservable<Unit> IDialog.OnCloseAsObservable(float defaultDuration)
        {
            return transform
                .DOScale(Vector3.zero, defaultDuration)
                .OnCompleteAsObservable();
        }

        IObservable<Unit> IDialog.OnStartAsObservable(object param)
        {
            _message.text = param as string;
            return Observable.ReturnUnit();
        }

        IObservable<Unit> IDialog.OnResumeAsObservable(object param)
        {
            return Observable.ReturnUnit();
        }

        IObservable<RequestDialog> IDialog.OnNextAsObservable()
        {
            return Observable.Empty<RequestDialog>();
        }

        IObservable<object> IDialog.OnPreviousAsObservable()
        {
            return _okButton
                .OnClickAsObservable()
                .Select(_ => "OK");
        }
        #endregion
    }
}
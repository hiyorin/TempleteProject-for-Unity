﻿using System;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter.Xml;
using SocialGame.Toast;
using UnityEngine;
using Zenject;
using UniRx;
using UniRx.Async;

namespace SocialGame.Internal.Toast
{
    internal sealed class ToastModel : IInitializable, IDisposable, IToastModel
    {
        private class Context
        {
            public IToast Toast;
            public GameObject Object;
        }

        [Serializable]
        private class Request
        {
            public IToast Toast;
            public object Param;
        }

        [Inject] private IToastIntent _intent = null;

        [Inject] private IToastFactory _factory = null;

        [Inject] private ToastSettings _settings = null;

        private readonly ReactiveProperty<Request> _current = new ReactiveProperty<Request>();

        private readonly Queue<Request> _requests = new Queue<Request>();

        private readonly ReactiveDictionary<string, Context> _contexts = new ReactiveDictionary<string, Context>();

        private readonly BoolReactiveProperty _isOpen = new BoolReactiveProperty();

        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        void IInitializable.Initialize()
        {
            _intent
                .OnOpenAsObservable()
                .Subscribe(x => Open(x).GetAwaiter())
                .AddTo(_disposable);

            Observable
                .EveryUpdate()
                .Where(_ => _current.Value == null)
                .Select(_ => _requests.Count > 0)
                .Do(x => _isOpen.Value = x)
                .Where(x => x)
                .Select(_ => _requests.Dequeue())
                .Subscribe(x => _current.Value = x)
                .AddTo(_disposable);

            _current
                .Where(x => x != null)
                .SelectMany(x => Process(x).ToObservable())
                .Subscribe(_ => _current.Value = null)
                .AddTo(_disposable);
        }

        private async UniTask Open(RequestToast request)
        {
            Context context = null;
            if (!_contexts.TryGetValue(request.Name, out context))
            {
                var toastObject = await _factory.Create(request.Name);
                if (toastObject == null)
                    return;
                
                context = new Context() {
                    Toast = toastObject.GetComponent<IToast>(),
                    Object = toastObject,
                };
                _contexts.Add(request.Name, context);
            }
            
            var req = new Request() {
                Toast = context.Toast,
                Param = request.Param,
            };
            
            if (_current.Value == null)
            {
                _isOpen.Value = true;
                _current.Value = req;
            }
            else
                _requests.Enqueue(req);
        }
        
        private async UniTask Process(Request request)
        {
            await request.Toast.OnOpen(request.Param, _settings.DefaultDuration);
            await UniTask.Delay(TimeSpan.FromSeconds(_settings.ShowDuration));
            await request.Toast.OnClose(_settings.DefaultDuration);
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }

        #region IToastModel implementation
        IObservable<GameObject> IToastModel.OnAddAsObservable()
        {
            return _contexts
                .ObserveAdd()
                .Select(x => x.Value.Object);
        }

        IObservable<Unit> IToastModel.OnOpenAsObservable()
        {
            return _isOpen
                .Where(x => x)
                .AsUnitObservable();
        }

        IObservable<Unit> IToastModel.OnCloseAsObservable()
        {
            return _isOpen
                .Where(x => !x)
                .AsUnitObservable();
        }
        #endregion
    }
}

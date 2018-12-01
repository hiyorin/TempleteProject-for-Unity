﻿using System;
using UnityEngine;

namespace SocialGame.Internal.Network
{
    [Serializable]
    internal sealed class HttpSettings
    {
        public enum Format
        {
            JSON,
            MessagePack,
            LZ4MessagePack,
            XML,
        }
        
        [SerializeField] private string _domain = "https://www.google.co.jp/";

        [SerializeField] private string _productionEnvironment = "";

        [SerializeField] private string _stagingEnvironment = "stg";

        [SerializeField] private string _developmentEnvironment = "dev";
        
        [SerializeField] private Format _dataFormat = Format.JSON;
        
        [SerializeField] private bool _useChunkedTransfer = true;

        [SerializeField] private float _timeOutSeconds = 10.0f;

        [SerializeField] private int _retryCount = 0;

        public string Domain => _domain;

        public string ProductionEnvironment => _productionEnvironment;

        public string StagingEnvironment => _stagingEnvironment;

        public string DevelopmentEnvironment => _developmentEnvironment;

        public Format DataFormat => _dataFormat;

        public bool UseChunkedTransfer => _useChunkedTransfer;

        public float TimeOutSeconds => _timeOutSeconds;

        public int RetryCount => _retryCount;
    }
}

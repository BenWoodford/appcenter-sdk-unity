﻿// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT license.

#if UNITY_WSA_10_0 && !UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Mobile.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Microsoft.Azure.Mobile.Unity.Internal.Utils
{
    public class UnityApplicationSettings : IApplicationSettings
    {
        private const string MobileCenterSettingsKey = "MobileCenterSettings";

        private readonly object _lockObject = new object();
        private readonly IDictionary<string, object> _current;
        private bool _dirty = false;

        public UnityApplicationSettings()
        {
            _current = ReadAll();
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            lock (_lockObject)
            {
                object result;
                var found = _current.TryGetValue(key, out result);
                if (found)
                {
                    return (T)result;
                }
            }
            SetValue(key, defaultValue);
            return defaultValue;
        }
        public void SetValue(string key, object value)
        {
            lock (_lockObject)
            {
                _current[key] = value;
                _dirty = true;
            }
        }

        public bool ContainsKey(string key)
        {
            lock (_lockObject)
            {
                return _current.ContainsKey(key);
            }
        }

        public void Remove(string key)
        {
            lock (_lockObject)
            {
                _current.Remove(key);
                _dirty = true;
            }
        }

        private static IDictionary<string, object> ReadAll()
        {
            var json = PlayerPrefs.GetString(MobileCenterSettingsKey, null);
            var settings = JsonConvert.DeserializeObject(json) as IDictionary<string, object>;
            return settings != null ? settings : new Dictionary<string, object>();
        }

        public static void Initialize()
        {
            var gameObject = new GameObject("Mobile Center Settings Helper")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<UnityApplicationSettingsHelper>().StartCoroutine(MainThreadCoroutine());
        }

        private IEnumerator MainThreadCoroutine()
        {
            while (true)
            {
                yield return null;
                if (_dirty)
                {
                    string json;
                    lock (_lockObject)
                    {
                        json = JsonConvert.SerializeObject(_current);
                        _dirty = false;
                    }
                    PlayerPrefs.SetString(MobileCenterSettingsKey, json);
                    PlayerPrefs.Save();
                }
            }
        }
    }
}
#endif

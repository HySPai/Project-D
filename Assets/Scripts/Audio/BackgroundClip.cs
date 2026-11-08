using System;
using System.Collections.Generic;
using Ifreet.Core.Runtime.Audio.Data;
using UnityEngine;

namespace Ifreet.Core.Runtime.Audio
{
    [Serializable]
    public class BackgroundClip : TClip
    {
        [SerializeField] private List<BackgroundClipData> m_Data = new();

        public List<BackgroundClipData> Data => this.m_Data;
    }
}
using System;
using System.Collections.Generic;
using Ifreet.Core.Runtime.Audio.Data;
using UnityEngine;

namespace Ifreet.Core.Runtime.Audio
{
    [Serializable]
    public class SfxClip : TClip
    {
        // PRIVATE MEMBERS: --------------------------------------------------------------------------------------------

        [SerializeField] private List<SfxClipData> m_Data = new();

        // PROPERTIES: -------------------------------------------------------------------------------------------------
        
        public List<SfxClipData> Data => this.m_Data;
    }
}
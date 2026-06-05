using System;
using System.Collections.Generic;
using Ifreet.Core.Runtime.Audio.Enums;
using Ifreet.Core.Runtime.Audio.Generated;
using UnityEngine;

namespace Ifreet.Core.Runtime.Audio.Data
{
    [Serializable]
    public class SfxClipData
    {
        // PRIVATE MEMBERS: --------------------------------------------------------------------------------------------

        [SerializeField] private ClipViewMode m_ViewMode = ClipViewMode.Grid;
        [SerializeField] private AudioSfxID m_ID;
        [SerializeField] private List<AudioClip> m_Clips;

        // PROPERTIES: -------------------------------------------------------------------------------------------------

        public AudioSfxID ID => this.m_ID;

        public List<AudioClip> Clips => this.m_Clips;
    }
}
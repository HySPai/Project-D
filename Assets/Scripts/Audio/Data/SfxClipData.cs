using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using Ifreet.Core.Runtime.Audio.Enums;
#endif
using Ifreet.Core.Runtime.Audio.Generated;
using UnityEngine;

namespace Ifreet.Core.Runtime.Audio.Data
{
    /// <summary>The clip variants registered for one SFX ID.</summary>
    [Serializable]
    public class SfxClipData
    {
#if UNITY_EDITOR
        [SerializeField] private ClipViewMode m_ViewMode = ClipViewMode.Grid;

        /// <summary>Editor-only inspector layout for the clip list.</summary>
        public ClipViewMode ViewMode => this.m_ViewMode;
#endif
        [SerializeField] private AudioSfxID m_ID;
        [SerializeField] private List<AudioClip> m_Clips;

        /// <summary>The audio ID these clips belong to.</summary>
        public AudioSfxID ID => this.m_ID;

        /// <summary>The clip variants for this ID.</summary>
        public List<AudioClip> Clips => this.m_Clips;
    }
}

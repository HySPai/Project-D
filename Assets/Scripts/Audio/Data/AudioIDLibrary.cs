using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ifreet.Core.Runtime.Audio.Data
{
    [Serializable]
    public class AudioIDLibrary
    {
        // PRIVATE MEMBERS: --------------------------------------------------------------------------------------------
        
        [SerializeField] private List<IDLibrary> m_SfxID = new();
        [Space(10), SerializeField] private List<IDLibrary> m_BackgroundID = new();

        // PROPERTIES: -------------------------------------------------------------------------------------------------
        
        public List<IDLibrary> SfxID => this.m_SfxID;
        public List<IDLibrary> BackgroundID => this.m_BackgroundID;
    }
}
using System;
using UnityEngine;

namespace Ifreet.Core.Runtime.Audio.Data
{
    [Serializable]
    public class IDLibrary
    {
        // PRIVATE MEMBERS: --------------------------------------------------------------------------------------------
        
        [SerializeField] private string m_ID;

        // PROPERTIES: -------------------------------------------------------------------------------------------------
        
        public string ID => this.m_ID;
    }
}
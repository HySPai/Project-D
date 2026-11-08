using System;
using UnityEngine;

namespace Ifreet.Core.Runtime.Audio.Data
{
    [Serializable]
    public class IDLibrary
    {
        [SerializeField] private string m_ID;

        public string ID => this.m_ID;
    }
}
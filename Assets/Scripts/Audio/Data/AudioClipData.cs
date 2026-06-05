using Ifreet.Core.Runtime.Constants;
using UnityEngine;

namespace Ifreet.Core.Runtime.Audio.Data
{
    public static class Files
    {
        public const string FILE_NAME = "so_audio_clip_data";
        public const string MENU_NAME = "Ifreet/Data/Audio";
    }
    
    [CreateAssetMenu(menuName = Files.MENU_NAME, fileName = Files.FILE_NAME, order = 0)]
    public class AudioClipData : ScriptableObject
    {
        // PRIVATE MEMBERS: --------------------------------------------------------------------------------------------

        [SerializeField] private AudioIDLibrary m_IDLibrary;
        [Space(20), SerializeField] private SfxClip m_Sfx;
        [SerializeField] private BackgroundClip m_Background;

        // PROPERTIES: -------------------------------------------------------------------------------------------------
        
        public AudioIDLibrary Library => this.m_IDLibrary;
        public SfxClip Sfx => this.m_Sfx;
        public BackgroundClip Background => this.m_Background;
    }
}
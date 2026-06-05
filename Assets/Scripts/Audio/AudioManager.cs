using System.Collections.Generic;
using Ifreet.Core.Runtime.Audio.Data;
using Ifreet.Core.Runtime.Audio.Generated;
using UnityEngine;
using UnityEngine.Audio;

namespace Ifreet.Core.Runtime.Audio
{
    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        // PRIVATE FIELDS: ---------------------------------------------------------------------------------------------
        
        [Header("Data")]
        [SerializeField] private AudioClipData m_AudioData;

        [Header("Mixer")]
        [SerializeField] private AudioMixer m_MainMixer;
        [SerializeField] private AudioMixerGroup m_SfxGroup;
        [SerializeField] private AudioMixerGroup m_BackgroundGroup;

        [Header("SFX Settings")]
        [SerializeField] private int m_SfxSourcePoolSize = 10; // số lượng audio source cho SFX pool

        private Dictionary<AudioSfxID, List<AudioClip>> m_SfxDict;
        private Dictionary<AudioBackgroundID, List<AudioClip>> m_BgDict;
        private Dictionary<AudioSfxID, AudioSource> m_SfxLoopDict;

        private List<AudioSource> m_SfxSources;
        private int m_SfxIndex;

        private AudioSource m_BackgroundSource;
        
        private SfxRepeatTracker m_SfxRepeatTracker;


        // INITIALIZERS: -----------------------------------------------------------------------------------------------

        protected void Awake()
        {
            this.m_SfxRepeatTracker = new SfxRepeatTracker();
            this.m_SfxLoopDict = new();
            
            // Setup SFX pool
            this.m_SfxSources = new List<AudioSource>();
            for (int i = 0; i < this.m_SfxSourcePoolSize; i++)
            {
                var src = this.gameObject.AddComponent<AudioSource>();
                src.outputAudioMixerGroup = this.m_SfxGroup;
                this.m_SfxSources.Add(src);
            }
            this.m_SfxIndex = 0;

            // Setup music source
            this.m_BackgroundSource = this.gameObject.AddComponent<AudioSource>();
            this.m_BackgroundSource.outputAudioMixerGroup = this.m_BackgroundGroup;

            // Build SFX Dictionary
            this.m_SfxDict = new Dictionary<AudioSfxID, List<AudioClip>>();
            foreach (var clipData in this.m_AudioData.Sfx.Data)
            {
                if (!this.m_SfxDict.ContainsKey(clipData.ID))
                {
                    this.m_SfxDict.Add(clipData.ID, clipData.Clips);
                }
            }

            // Build Background Dictionary
            this.m_BgDict = new Dictionary<AudioBackgroundID, List<AudioClip>>();
            foreach (var clipData in this.m_AudioData.Background.Data)
            {
                if (!this.m_BgDict.ContainsKey(clipData.ID))
                {
                    this.m_BgDict.Add(clipData.ID, clipData.Clips);
                }
            }
        }

        // PUBLIC METHODS: ---------------------------------------------------------------------------------------------

        public void PlaySfx(AudioSfxID id, float volume = 1, int index = -1, bool useHaptic = true)
        {
            if (this.m_SfxDict.TryGetValue(id, out var clips))
            {
                if (clips.Count == 0) return;

                var chosenIndex = (index == -1) 
                    ? Random.Range(0, clips.Count) 
                    : Mathf.Clamp(index, 0, clips.Count - 1);
                
                var clip = clips[chosenIndex];
                if (clip == null) return;

                // lấy AudioSource tiếp theo trong pool
                var src = this.m_SfxSources[this.m_SfxIndex];

                src.PlayOneShot(clip, volume);

                // tăng index (vòng tròn)
                this.m_SfxIndex = (this.m_SfxIndex + 1) % this.m_SfxSources.Count;
            }

            if (useHaptic)
            {
                HapticController.Instance.PlayHaptic();
            }
        }
        
        public void PlaySfxRepeat(AudioSfxID id, float volume = 1, bool useHaptic = true)
        {
            if (this.m_SfxDict.TryGetValue(id, out var clips))
            {
                if (clips.Count == 0) return;

                int chosenIndex = this.m_SfxRepeatTracker.GetNextIndex(id, clips.Count);
                if (chosenIndex == -1) return;

                var clip = clips[chosenIndex];
                if (clip == null) return;

                var src = this.m_SfxSources[this.m_SfxIndex];
                src.PlayOneShot(clip, volume);

                this.m_SfxIndex = (this.m_SfxIndex + 1) % this.m_SfxSources.Count;
            }

            if (useHaptic)
            {
                HapticController.Instance.PlayHaptic();
            }
        }
        
        public void ResetRepeatSfx(AudioSfxID id)
        {
            this.m_SfxRepeatTracker.Reset(id);
        }
        
        public void ResetAllRepeatSfx()
        {
            this.m_SfxRepeatTracker.ResetAll();
        }

        public void PlaySfxLoop(AudioSfxID id, float volume = 1, int index = -1, bool useHaptic = true)
        {
            if (this.m_SfxDict.TryGetValue(id, out var clips))
            {
                if (clips.Count == 0) return;

                var chosenIndex = (index == -1) 
                    ? Random.Range(0, clips.Count) 
                    : Mathf.Clamp(index, 0, clips.Count - 1);

                var clip = clips[chosenIndex];
                if (clip == null) return;

                // nếu clip này đã loop thì không cần play lại
                if (this.m_SfxLoopDict.ContainsKey(id))
                    return;

                // tạo AudioSource riêng cho loop (ko dùng pool OneShot)
                var src = this.gameObject.AddComponent<AudioSource>();
                src.outputAudioMixerGroup = this.m_SfxGroup;
                src.loop = true;
                src.volume = volume;
                src.clip = clip;
                src.Play();

                this.m_SfxLoopDict.Add(id, src);
            }

            if (useHaptic)
            {
                HapticController.Instance.PlayHaptic();
            }
        }

        public void StopLoopSfx(AudioSfxID id)
        {
            if (this.m_SfxLoopDict.TryGetValue(id, out var src))
            {
                if (src != null)
                {
                    src.Stop();
                    Destroy(src); // xoá AudioSource để gọn gàng
                }
                this.m_SfxLoopDict.Remove(id);
            }
        }

        public void PlayBackground(AudioBackgroundID id, int index = -1, bool loop = true)
        {
            if (this.m_BgDict.TryGetValue(id, out var clips))
            {
                if (clips.Count == 0) return;

                var chosenIndex = (index == -1) 
                    ? Random.Range(0, clips.Count) 
                    : Mathf.Clamp(index, 0, clips.Count - 1);

                var clip = clips[chosenIndex];
                if (clip == null) return;

                this.m_BackgroundSource.clip = clip;
                this.m_BackgroundSource.loop = loop;
                this.m_BackgroundSource.Play();
            }
            else
            {
                Debug.LogError($"Background {id} not found!");
            }
        }

        public void StopBackground()
        {
            this.m_BackgroundSource.Stop();
            this.m_BackgroundSource.clip = null;
        }

        public void SetSfxVolume(float volume) // 0~1
        {
            PlayerprefSave.Sound = volume;
            this.m_MainMixer.SetFloat("Sfx", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        }

        public void SetBackgroundVolume(float volume) // 0~1
        {
            PlayerprefSave.Music = volume;
            this.m_MainMixer.SetFloat("Background", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
        }
    }
}

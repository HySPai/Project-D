using System.Collections.Generic;
using Ifreet.Core.Runtime.Audio.Data;
using Ifreet.Core.Runtime.Audio.Generated;
using UnityEngine;
using UnityEngine.Audio;

namespace Ifreet.Core.Runtime.Audio
{
    /// <summary>
    /// Plays SFX through a round-robin pool of one-shot sources, looping SFX on dedicated sources and one background
    /// track, all routed to the assigned mixer groups.
    /// </summary>
    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        private const string SFX_VOLUME_PARAM = "Sfx";
        private const string BACKGROUND_VOLUME_PARAM = "Background";
        private const float MIN_LINEAR_VOLUME = 0.0001f;

        [Header("Data")]
        [SerializeField] private AudioClipData m_AudioData;

        [Header("Mixer")]
        [SerializeField] private AudioMixer m_MainMixer;
        [SerializeField] private AudioMixerGroup m_SfxGroup;
        [SerializeField] private AudioMixerGroup m_BackgroundGroup;

        [Header("SFX Settings")]
        [SerializeField] private int m_SfxSourcePoolSize = 10;

        private Dictionary<AudioSfxID, List<AudioClip>> m_SfxDict;
        private Dictionary<AudioBackgroundID, List<AudioClip>> m_BgDict;
        private Dictionary<AudioSfxID, AudioSource> m_SfxLoopDict;

        private List<AudioSource> m_SfxSources;
        private int m_SfxIndex;

        private AudioSource m_BackgroundSource;

        private SfxRepeatTracker m_SfxRepeatTracker;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            this.m_SfxRepeatTracker = new SfxRepeatTracker();
            this.m_SfxLoopDict = new Dictionary<AudioSfxID, AudioSource>();

            this.m_SfxSources = new List<AudioSource>(this.m_SfxSourcePoolSize);
            for (int i = 0; i < this.m_SfxSourcePoolSize; i++)
            {
                var src = this.gameObject.AddComponent<AudioSource>();
                src.outputAudioMixerGroup = this.m_SfxGroup;
                this.m_SfxSources.Add(src);
            }
            this.m_SfxIndex = 0;

            this.m_BackgroundSource = this.gameObject.AddComponent<AudioSource>();
            this.m_BackgroundSource.outputAudioMixerGroup = this.m_BackgroundGroup;

            this.m_SfxDict = new Dictionary<AudioSfxID, List<AudioClip>>();
            foreach (var clipData in this.m_AudioData.Sfx.Data)
            {
                this.m_SfxDict.TryAdd(clipData.ID, clipData.Clips);
            }

            this.m_BgDict = new Dictionary<AudioBackgroundID, List<AudioClip>>();
            foreach (var clipData in this.m_AudioData.Background.Data)
            {
                this.m_BgDict.TryAdd(clipData.ID, clipData.Clips);
            }
        }

        /// <summary>Plays a one-shot SFX; <paramref name="index"/> -1 picks a random variant.</summary>
        public void PlaySfx(AudioSfxID id, float volume = 1, int index = -1, bool useHaptic = true)
        {
            if (this.m_SfxDict.TryGetValue(id, out var clips))
            {
                if (!TryPickClip(clips, index, out var clip)) return;

                this.PlayOneShot(clip, volume);
            }

            if (useHaptic)
            {
                HapticController.Instance.PlayHaptic();
            }
        }

        /// <summary>Plays the next variant of an SFX in order, wrapping around after the last one.</summary>
        public void PlaySfxRepeat(AudioSfxID id, float volume = 1, bool useHaptic = true)
        {
            if (this.m_SfxDict.TryGetValue(id, out var clips))
            {
                if (clips.Count == 0) return;

                int chosenIndex = this.m_SfxRepeatTracker.GetNextIndex(id, clips.Count);
                if (chosenIndex == -1) return;

                var clip = clips[chosenIndex];
                if (clip == null) return;

                this.PlayOneShot(clip, volume);
            }

            if (useHaptic)
            {
                HapticController.Instance.PlayHaptic();
            }
        }

        /// <summary>Restarts the ordered variant sequence of one SFX.</summary>
        public void ResetRepeatSfx(AudioSfxID id)
        {
            this.m_SfxRepeatTracker.Reset(id);
        }

        /// <summary>Restarts the ordered variant sequence of every SFX.</summary>
        public void ResetAllRepeatSfx()
        {
            this.m_SfxRepeatTracker.ResetAll();
        }

        /// <summary>Starts a looping SFX on its own source; does nothing if that SFX is already looping.</summary>
        public void PlaySfxLoop(AudioSfxID id, float volume = 1, int index = -1, bool useHaptic = true)
        {
            if (this.m_SfxDict.TryGetValue(id, out var clips))
            {
                if (!TryPickClip(clips, index, out var clip)) return;

                if (this.m_SfxLoopDict.ContainsKey(id))
                    return;

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

        /// <summary>Stops a looping SFX and removes its source.</summary>
        public void StopLoopSfx(AudioSfxID id)
        {
            if (this.m_SfxLoopDict.TryGetValue(id, out var src))
            {
                if (src != null)
                {
                    src.Stop();
                    Destroy(src);
                }
                this.m_SfxLoopDict.Remove(id);
            }
        }

        /// <summary>Plays a background track; <paramref name="index"/> -1 picks a random variant.</summary>
        public void PlayBackground(AudioBackgroundID id, int index = -1, bool loop = true)
        {
            if (this.m_BgDict.TryGetValue(id, out var clips))
            {
                if (!TryPickClip(clips, index, out var clip)) return;

                this.m_BackgroundSource.clip = clip;
                this.m_BackgroundSource.loop = loop;
                this.m_BackgroundSource.Play();
            }
            else
            {
                GameLog.Warn($"Background {id} not found!", this);
            }
        }

        /// <summary>Stops the background track.</summary>
        public void StopBackground()
        {
            this.m_BackgroundSource.Stop();
            this.m_BackgroundSource.clip = null;
        }

        /// <summary>Saves and applies the SFX volume, given as a 0-1 linear value.</summary>
        public void SetSfxVolume(float volume)
        {
            PlayerprefSave.Sound = volume;
            this.m_MainMixer.SetFloat(SFX_VOLUME_PARAM, LinearToDecibel(volume));
        }

        /// <summary>Saves and applies the background volume, given as a 0-1 linear value.</summary>
        public void SetBackgroundVolume(float volume)
        {
            PlayerprefSave.Music = volume;
            this.m_MainMixer.SetFloat(BACKGROUND_VOLUME_PARAM, LinearToDecibel(volume));
        }

        private void PlayOneShot(AudioClip clip, float volume)
        {
            var src = this.m_SfxSources[this.m_SfxIndex];
            src.PlayOneShot(clip, volume);
            this.m_SfxIndex = (this.m_SfxIndex + 1) % this.m_SfxSources.Count;
        }

        private static bool TryPickClip(List<AudioClip> clips, int index, out AudioClip clip)
        {
            clip = null;
            if (clips.Count == 0) return false;

            int chosenIndex = index == -1
                ? Random.Range(0, clips.Count)
                : Mathf.Clamp(index, 0, clips.Count - 1);

            clip = clips[chosenIndex];
            return clip != null;
        }

        private static float LinearToDecibel(float volume)
        {
            return Mathf.Log10(Mathf.Clamp(volume, MIN_LINEAR_VOLUME, 1f)) * 20f;
        }
    }
}

using System.Collections.Generic;
using Ifreet.Core.Runtime.Audio.Generated;

namespace Ifreet.Core.Runtime.Audio
{
    /// <summary>Tracks, per SFX, which variant plays next so variants cycle in order.</summary>
    public class SfxRepeatTracker
    {
        private readonly Dictionary<AudioSfxID, int> m_IndexDict = new();

        /// <summary>Returns the next variant index for <paramref name="id"/>, or -1 when it has no clips.</summary>
        public int GetNextIndex(AudioSfxID id, int clipCount)
        {
            if (clipCount == 0) return -1;

            if (!this.m_IndexDict.TryGetValue(id, out int index))
            {
                this.m_IndexDict[id] = 0;
                return 0;
            }

            index = (index + 1) % clipCount;
            this.m_IndexDict[id] = index;

            return index;
        }

        /// <summary>Restarts the variant sequence of one SFX.</summary>
        public void Reset(AudioSfxID id)
        {
            if (this.m_IndexDict.ContainsKey(id))
                this.m_IndexDict[id] = 0;
        }

        /// <summary>Restarts the variant sequence of every SFX.</summary>
        public void ResetAll()
        {
            this.m_IndexDict.Clear();
        }
    }
}

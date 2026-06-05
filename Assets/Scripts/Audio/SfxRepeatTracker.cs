using System.Collections.Generic;
using Ifreet.Core.Runtime.Audio.Generated;

namespace Ifreet.Core.Runtime.Audio
{
    public class SfxRepeatTracker
    {
        // PRIVATE MEMBERS: --------------------------------------------------------------------------------------------
        
        private readonly Dictionary<AudioSfxID, int> m_IndexDict = new();

        // PUBLIC METHODS: ---------------------------------------------------------------------------------------------
        
        public int GetNextIndex(AudioSfxID id, int clipCount)
        {
            if (clipCount == 0) return -1;

            if (!this.m_IndexDict.ContainsKey(id))
            {
                this.m_IndexDict[id] = 0;
                return 0;
            }

            // lấy index hiện tại rồi tăng
            int index = this.m_IndexDict[id];
            index = (index + 1) % clipCount;
            this.m_IndexDict[id] = index;

            return index;
        }

        public void Reset(AudioSfxID id)
        {
            if (this.m_IndexDict.ContainsKey(id))
                this.m_IndexDict[id] = 0;
        }
        
        public void ResetAll()
        {
            this.m_IndexDict.Clear();
        }
    }
}
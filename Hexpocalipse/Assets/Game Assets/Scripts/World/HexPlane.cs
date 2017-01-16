using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{

    [Serializable]
    public sealed class HexPlane: HexDataStorage
    {
        [SerializeField]
        public int chunkDepth { get; private set; }
        [SerializeField]
        private Dictionary<HexCoords, float[]> chunks;
        [SerializeField]
        private HexChunkProvider chunkProvider;
        [SerializeField]
        private List<ChunksSection> debugChunks;

        public HexPlane(int chunkDepth, HexChunkProvider chunkProvider)
        {
            this.chunkDepth = chunkDepth;
            this.chunkProvider = chunkProvider;
            chunks = new Dictionary<HexCoords, float[]>();
        }

        public float this[HexCoords coords]
        {
            get
            {
                return ValueAt(coords);
            }
        }

        public bool Contains(HexCoords coords)
        {
            return chunks.ContainsKey(coords.ChunkCoords(chunkDepth));
        }

        public float ValueAt(HexCoords coords)
        {
            HexCoords chunkStart = coords.ChunkCoords(chunkDepth);
            float[] retChunk = null;
            if(!chunks.ContainsKey(chunkStart))
            {
                float[] retVal = chunkProvider.ChunkForStorage(this, chunkStart);
                if(retVal == null)
                {
                    return 0.0f;
                }
                chunks[chunkStart] = retVal;
                //RefreshDebugChunks();
            }
            retChunk = chunks[chunkStart];
            HexCoords delta = coords - (chunkStart << chunkDepth);
            //if (retChunk.Length != (1L << (2 * chunkDepth)))
            //{
            //    UnityEngine.Debug.Log("length mismatch: " + retChunk.Length.ToString() + " vs " + (1L << (2 * chunkDepth)).ToString());
            //    return 0.0f;
            //}
            //if (((delta.u << chunkDepth) | delta.v) >= retChunk.Length)
            //{
            //    UnityEngine.Debug.Log("Wrong index: " + ((delta.u << chunkDepth) | delta.v).ToString() + " [ " + delta.u.ToString() + " ; " + delta.v.ToString() + " ] / " + retChunk.Length.ToString());
            //    return 0.0f;
            //}
            return retChunk[(delta.u << chunkDepth) | delta.v];
        }

        void RefreshDebugChunks()
        {
            debugChunks = new List<ChunksSection>();
            foreach (KeyValuePair<HexCoords, float[]> entry in chunks)
            {
                debugChunks.Add(new ChunksSection(entry.Key, entry.Value));
            }
        }

        [Serializable]
        public struct ChunksSection
        {
            public HexCoords coords;
            public float[] chunks;

            public ChunksSection(HexCoords coords, float[] chunks)
            {
                this.coords = coords;
                this.chunks = chunks;
            }
        }
    }

}


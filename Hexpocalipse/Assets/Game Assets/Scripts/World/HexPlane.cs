using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace World
{

    [Serializable]
    public sealed class HexPlane: HexDataStorage
    {
        [SerializeField]
        public int chunkDepth { get; private set; }
        [SerializeField]
        private string savePath;
        [SerializeField]
        private Dictionary<HexCoords, float[]> chunks;
        [SerializeField]
        private HexChunkProvider chunkProvider;

        public HexPlane(int chunkDepth, HexChunkProvider chunkProvider, string savePath = null)
        {
            this.chunkDepth = chunkDepth;
            this.chunkProvider = chunkProvider;
            this.savePath = savePath;
            chunks = new Dictionary<HexCoords, float[]>();

            if (savePath != null && !Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
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
            if (!chunks.ContainsKey(chunkStart))
            {
                retChunk = LoadChunk(chunkStart);
                if (retChunk == null)
                {
                    retChunk = chunkProvider.ChunkForStorage(this, chunkStart);
                    if (retChunk == null)
                    {
                        return 0.0f;
                    }
                    SaveChunk(chunkStart, retChunk);
                }
                chunks[chunkStart] = retChunk;
            }
            else
            {
                retChunk = chunks[chunkStart];
            }
            HexCoords delta = coords - (chunkStart << chunkDepth);
            return retChunk[(delta.u << chunkDepth) | delta.v];
        }

        void SaveChunk(HexCoords coords, float[] values)
        {
            string path = PathForSavingChunk(coords);
            if(path == null)
            {
                return;
            }
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Create(path);
            bf.Serialize(fs, values);
            fs.Close();

            //Logger.Log("File '" + path + "' saved.");
        }

        float[] LoadChunk(HexCoords coords)
        {
            string path = PathForSavingChunk(coords);
            if (path == null || !File.Exists(path))
            {
                return null;
            }
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(path, FileMode.Open);
            float[] result = (float[])bf.Deserialize(fs);
            fs.Close();

            //Logger.Log("File '" + path + "' loaded.");

            return result;
        }

        string PathForSavingChunk(HexCoords coords)
        {
            if(savePath == null)
            {
                return null;
            }
            return savePath + "/" + coords.ToString() + ".dat";
        }
    }

}


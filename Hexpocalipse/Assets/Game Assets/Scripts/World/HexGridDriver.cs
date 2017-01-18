using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    [Serializable]
    public class HexGridDriver: HexChunkProvider, HexDataStorage
    {
        [SerializeField]
        public int chunkDepth { get; private set; }
        [SerializeField]
        HexPlane[] dataPlanes;
        [SerializeField]
        HexPlane alphaPlane;
        [SerializeField]
        HexGridGen gridGen;

        public HexGridDriver(int chunkDepth, int fractalDepth, float delta0, float lambda, string savePath = null)
        {
            string alphaPath = (savePath == null) ? null : (savePath + "/h/a");
            string heightPath = (savePath == null) ? null : (savePath + "/h/");

            this.chunkDepth = chunkDepth;
            gridGen    = new HexGridGen(fractalDepth, delta0, lambda);
            alphaPlane = new HexPlane(chunkDepth, new HexAlphaGen(), alphaPath);
            dataPlanes = new HexPlane[chunkDepth + 1];
            for(int i = 0; i <= chunkDepth; i++)
            {
                string planePath = (heightPath == null) ? null : (heightPath + i.ToString());
                dataPlanes[i] = new HexPlane(i, this, planePath);
            }
        }

        public float[] ChunkForStorage(HexDataStorage storage, HexCoords chunkStart)
        {
            int planeIndex = -1;
            for(int i = 0; i <= chunkDepth; i++)
            {
                if(dataPlanes[i] != storage)
                {
                    continue;
                }
                planeIndex = i;
                break;
            }
            if(planeIndex < 0 || planeIndex != storage.chunkDepth)
            {
                return null;
            }
            float[] result = new float[1L << (2 * planeIndex)];
            int chunkSize = 1 << planeIndex;
            HexCoords realChunkStart = (chunkStart << chunkDepth);
            for (int u = 0; u < chunkSize; u++)
            {
                for(int v = 0; v < chunkSize; v++)
                {
                    HexCoords dCoords = new HexCoords(u, v);
                    HexCoords tCoords = realChunkStart + (dCoords << (chunkDepth - planeIndex)); 
                    float value = Locate(tCoords, false);
                    int indx = (u << planeIndex) | v;
                    result[indx] = value;
                }
            }
            return result;
        }

        public bool Contains(HexCoords coords)
        {
            return true;
        }

        public float ValueAt(HexCoords coords)
        {
            return Locate(coords, true);
        }

        public float this[HexCoords coords]
        {
            get
            {
                return ValueAt(coords);
            }
        }

        private float Locate(HexCoords coords, bool makeNewChunks)
        {
            int d = coords.Depth;
            if (d >= chunkDepth)
            {
                d = 0;
            } else
            {
                d = chunkDepth - d;
            }
            HexCoords newCoords = coords.ChunkCoords(chunkDepth - d);
            float result;
            if (makeNewChunks || dataPlanes[d].Contains(newCoords))
            {
                result = dataPlanes[d].ValueAt(newCoords);
            }
            else
            {
                result = gridGen.Evaluate(coords, this, alphaPlane);
            }
            return result;
        }
    }
}

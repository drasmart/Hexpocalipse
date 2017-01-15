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

        public HexGridDriver(int chunkDepth, float delta0, float lambda)
        {
            this.chunkDepth = chunkDepth;
            gridGen    = new HexGridGen(chunkDepth, delta0, lambda);
            alphaPlane = new HexPlane(chunkDepth, new HexAlphaGen());
            dataPlanes = new HexPlane[chunkDepth + 1];
            for(int i = 0; i <= chunkDepth; i++)
            {
                dataPlanes[i] = new HexPlane(i, this);
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
            //Debug.Log("Size = " + result.Length.ToString());
            int chunkSize = 1 << planeIndex;
            for(int u = 0; u < chunkSize; u++)
            {
                for(int v = 0; v < chunkSize; v++)
                {
                    HexCoords tCoords = new HexCoords(u, v) << (chunkDepth - planeIndex);
                    //Debug.Log("Index = " + ((u << planeIndex) | v).ToString());
                    result[(u << planeIndex) | v] = Locate(tCoords, false);
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
            if(d >= chunkDepth)
            {
                d = 0;
            } else
            {
                d = chunkDepth - d;
            }
            HexCoords newCoords = coords.ChunkCoords(coords.Depth);
            if (makeNewChunks || dataPlanes[d].Contains(newCoords))
            {
                return dataPlanes[d].ValueAt(newCoords);
            }
            return gridGen.Evaluate(coords, this, alphaPlane);
        }
    }
}

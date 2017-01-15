using System;
using System.Collections;

namespace World
{

    public interface HexChunkProvider
    {
        float[] ChunkForStorage(HexDataStorage storage, HexCoords chunkStart);
    }

    public interface HexDataStorage
    {
        int chunkDepth { get; }
        bool Contains(HexCoords coords);
        float ValueAt(HexCoords coords);
    }

    public interface HexValGen
    {
        HexCoords[] PointsRequiredForEvaluating(HexCoords coords);
        float Evaluate(HexCoords coords, HexDataStorage valueStorage, HexDataStorage alphaStorage);
    }

}

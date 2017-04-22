
namespace World
{

    public class LockedStorage : HexDataStorage
    {

        private HexDataStorage lockedStorage;

        public LockedStorage(HexDataStorage storage)
        {
            lockedStorage = storage;
        }
        
        public int chunkDepth {
            get {
                return lockedStorage.chunkDepth;
            }
        }

        public bool Contains(HexCoords coords)
        {
            return lockedStorage.Contains(coords);
        }

        public float ValueAt(HexCoords coords)
        {
            float result;
            lock (lockedStorage)
            {
                result = lockedStorage.ValueAt(coords);
            }
            return result;
        }

    }

}

using System;
using UnityEngine;

namespace World
{

    [Serializable]
    public struct HexCoords
    {
        // Stored properties

        public long u;
        public long v;

        // Constructor

        public HexCoords(long u, long v)
        {
            this.u = u;
            this.v = v;
        }

        // Computed properties

        private static double Sin60  = Math.Sin(Math.PI / 3);
        private static float  Sin60f = (float)Sin60;

        public double Alpha
        {
            get
            {
                return Math.Atan2(Sin60 * v, u - 0.5f * v);
            }
        }

        public int Depth
        {
            get
            {
                int result = 0x7FFF;
                long t = u | v;
                if (t != 0)
                {
                    int i = 0;
                    while ((t & 1 << i) == 0)
                    {
                        i++;
                    }
                    result = i;
                }
                return result;
            }
        }

        public HexCoords[] Parents
        {
            get
            {
                int d2 = Depth + 1;
                long u0 = (u >> d2) << d2;
                long v0 = (v >> d2) << d2;
                long dt = 1 << d2;
                long du = (u - u0) > 0 ? dt : 0;
                long dv = (v - v0) > 0 ? dt : 0;
                HexCoords[] result = { new HexCoords(u0, v0), new HexCoords(u0 + du, v0 + dv) };
                return result;
            }
        }

        public Vector2 toVector2
        {
            get
            {
                return new Vector2(u - 0.5f * v, Sin60f * v);
            }
        }

        public override string ToString()
        {
            return "[ u = " + u.ToString() + " ; v = " + v.ToString() + " ]";
        }

        // Parametrized properties

        public Vector3 toVector3(float y)
        {
            return new Vector3(u - 0.5f * v, y, Sin60f * v);
        }

        public HexCoords ClosestAxialDirectionTo(HexCoords target)
        {
            HexCoords result = target - this;
            result.u = Math.Sign(result.u);
            result.v = Math.Sign(result.v);
            return result;
        }

        public HexCoords ChunkCoords(int depth)
        {
            return this >> depth;
        }

        public HexCoords NextIn(HexCoords direction)
        {
            return this + Depth * direction;
        }

        // public static operators

        public static HexCoords operator -(HexCoords rhs)
        {
            return new HexCoords(-rhs.u, -rhs.v);
        }

        public static HexCoords operator +(HexCoords lhs, HexCoords rhs)
        {
            return new HexCoords(lhs.u + rhs.u, lhs.v + rhs.v);
        }

        public static HexCoords operator -(HexCoords lhs, HexCoords rhs)
        {
            return new HexCoords(lhs.u - rhs.u, lhs.v - rhs.v);
        }

        public static HexCoords operator *(HexCoords lhs, long rhs)
        {
            return new HexCoords(lhs.u * rhs, lhs.v * rhs);
        }

        public static HexCoords operator *(long lhs, HexCoords rhs)
        {
            return new HexCoords(lhs * rhs.u, lhs * rhs.v);
        }

        public static HexCoords operator /(HexCoords lhs, long rhs)
        {
            return new HexCoords(lhs.u / rhs, lhs.v / rhs);
        }

        public static HexCoords operator %(HexCoords lhs, long rhs)
        {
            return new HexCoords(lhs.u % rhs, lhs.v % rhs);
        }

        public static HexCoords operator <<(HexCoords lhs, int rhs)
        {
            return new HexCoords(lhs.u << rhs, lhs.v << rhs);
        }

        public static HexCoords operator >>(HexCoords lhs, int rhs)
        {
            return new HexCoords(lhs.u >> rhs, lhs.v >> rhs);
        }

        //// Support for usage as dictionary key

        //public override bool Equals(Object obj)
        //{
        //    HexCoords other = (obj as HexCoords);
        //    if (other != null)
        //    {
        //        return u == other.u && v == other.v;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public override int GetHashCode()
        //{
        //    return (((int)u & 0xFF) << 16) | ((int)v & 0xFF);
        //}
    }

}

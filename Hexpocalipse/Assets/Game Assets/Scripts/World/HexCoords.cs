using System;

namespace World {

	[Serializable()]
	public class HexCoords  {

		private static int defD = 0x7FFF;

		// Stored properties

		private long _u;
		private long _v;

		private HexCoords _dep0 = null;
		private HexCoords _dep1 = null;

		private int _d = -1;

		// Computed properties

		public long u { get { return _u; } }
		public long v { get { return _v; } }

		public int d {
			get {
				if (_d < 0) {
					long t = u | v;
					if (t == 0) {
						_d = HexCoords.defD;
					} else {
						int i = 0;
						while ((t & 1<<i) == 0) {
							i++;
						}
						_d = i;
					}
				}
				return _d;
			}
		}

		public HexCoords dep0 {
			get {
				if (_dep0 == null) {
					CalculateDependencies();
				}
				return _dep0;
			}
		}

		public HexCoords dep1 {
			get {
				if (_dep1 == null) {
					CalculateDependencies();
				}
				return _dep1;
			}
		}


		// Lifecycle

		public HexCoords(long u, long v) {
			_u = u;
			_v = v;
		}

		// public methods

		public HexCoords ChunkCoords(int depth) {
			return new HexCoords (u >> depth, v >> depth);
		}

		// Support for usage as dictionary key

		public override bool Equals(System.Object obj) {
			HexCoords other = (obj as HexCoords);
			if (other != null) {
				return u == other.u && v == other.v;
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return (((int)u & 0xFF) << 16) | ((int)v & 0xFF);
		}
		
		// private methods 

		void CalculateDependencies() {
			int d2 = d + 1;
			long u0 = (u >> d2) << d2;
			long v0 = (v >> d2) << d2;
			_dep0 = new HexCoords (u0, v0);
			long du = (u - u0) > 0 ? 1 << d2 : 0;
			long dv = (v - v0) > 0 ? 1 << d2 : 0;
			_dep1 = new HexCoords (u0 + du, v0 + dv);
		}
	}

}

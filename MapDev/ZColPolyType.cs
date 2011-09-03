using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SharpOcarina
{
    public class ZColPolyType
    {
        public ZColPolyType() { }

        public ZColPolyType(ulong _Raw)
        {
            Raw = _Raw;
        }

        public ulong Raw;

        [XmlIgnore]
        public long ExitNumber
        {
            get { return (long)((Raw & 0x00000F0000000000) >> 40); }
            set { Raw = ((Raw & 0xFFFFF0FFFFFFFFFF) | ((ulong)(value & 0xF) << 40)); }
        }

        [XmlIgnore]
        public long ClimbingCrawlingFlags
        {
            get { return (long)((Raw & 0x00F0000000000000) >> 52); }
            set { Raw = ((Raw & 0xFF0FFFFFFFFFFFFF) | ((ulong)(value & 0xF) << 52)); }
        }

        [XmlIgnore]
        public long DamageSurfaceFlags
        {
            get { return (long)((Raw & 0x000FF00000000000) >> 44); }
            set { Raw = ((Raw & 0xFFF00FFFFFFFFFFF) | ((ulong)(value & 0xFF) << 44)); }
        }

        [XmlIgnore]
        public int EchoRange
        {
            get { return (int)((Raw & 0x000000000000F000) >> 12); }
            set { Raw = ((Raw & 0xFFFFFFFFFFFF0FFF) | ((uint)(value & 0xF) << 12)); }
        }

        [XmlIgnore]
        public int EnvNumber
        {
            get { return (int)((Raw & 0x0000000000000F00) >> 8); }
            set { Raw = ((Raw & 0xFFFFFFFFFFFFF0FF) | ((uint)(value & 0xF) << 8)); }
        }

        [XmlIgnore]
        public bool IsSteep
        {
            get { return (((Raw & 0x0000000000000030) >> 4) == 1); }
            set { Raw = ((Raw & 0xFFFFFFFFFFFFFFEF) | ((uint)Convert.ToByte(value) << 4)); }
        }

        [XmlIgnore]
        public int TerrainType
        {
            get { return (int)((Raw & 0x00000000000000F0) >> 4); }
            set { Raw = ((Raw & 0xFFFFFFFFFFFFFF0F) | ((uint)(value & 0xF) << 4)); }
        }

        [XmlIgnore]
        public int GroundType
        {
            get { return (int)(Raw & 0x000000000000000F); }
            set { Raw = ((Raw & 0xFFFFFFFFFFFFFFF0) | ((uint)(value & 0xF))); }
        }
    }
}

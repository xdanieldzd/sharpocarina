using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
/* DeathBasket, http://core.the-gcn.com/index.php?/topic/675-sharpocarina-zelda-oot-scene-development-system/page__view__findpost__p__11155
Also, the waterbox room data I promised. Instead of:
0000qqqq
It seems to be something like:
00rrrrrr
where r would be pppp00 || 00qqqq
and p would then be room_number << 5
so a waterbox in room 0xD would look something like 0001A000
*/

namespace SharpOcarina
{
    public class ZWaterbox
    {
        [XmlIgnore]
        private float _XPos, _YPos, _ZPos;
        [XmlIgnore]
        private float _XSize, _ZSize;
        [XmlIgnore]
        private uint _Properties;
        /*
        [XmlIgnore]
        public uint RoomNumber
        {
            get { return (uint)((_Properties & 0x00000000) >> 8); }
            set { _Properties = ((_Properties & 0xFFFFFFFF) | ((uint)(value & 0xF) << 40)); }
        }
        */
        public float XPos
        {
            get { return _XPos; }
            set { _XPos = value; }
        }

        public float YPos
        {
            get { return _YPos; }
            set { _YPos = value; }
        }

        public float ZPos
        {
            get { return _ZPos; }
            set { _ZPos = value; }
        }

        public float XSize
        {
            get { return _XSize; }
            set { _XSize = value; }
        }

        public float ZSize
        {
            get { return _ZSize; }
            set { _ZSize = value; }
        }

        public uint Properties
        {
            get { return _Properties; }
            set { _Properties = value; }
        }
        
        public ZWaterbox() { }

        public ZWaterbox(float XPos, float YPos, float ZPos, float XSize, float ZSize, uint Properties)
        {
            _XPos = XPos;
            _YPos = YPos;
            _ZPos = ZPos;
            _XSize = XSize;
            _ZSize = ZSize;
            _Properties = Properties;
        }
    }
}

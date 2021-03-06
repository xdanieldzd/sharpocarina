﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SharpOcarina
{
    public class ZActor
    {
        [XmlIgnore]
        public bool IsTransition = false;
        [XmlIgnore]
        private ushort _Number, _Variable;
        [XmlIgnore]
        private float _XPos, _YPos, _ZPos;
        [XmlIgnore]
        private float _XRot, _YRot, _ZRot;
        [XmlIgnore]
        private byte _FrontSwitchTo, _FrontCamera, _BackSwitchTo, _BackCamera;

        public ushort Number
        {
            get { return _Number; }
            set { _Number = value; }
        }

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

        public float XRot
        {
            get { return _XRot; }
            set { _XRot = value; }
        }

        public float YRot
        {
            get { return _YRot; }
            set { _YRot = value; }
        }

        public float ZRot
        {
            get { return _ZRot; }
            set { _ZRot = value; }
        }

        public ushort Variable
        {
            get { return _Variable; }
            set { _Variable = value; }
        }

        public byte FrontSwitchTo
        {
            get { return _FrontSwitchTo; }
            set { _FrontSwitchTo = value; }
        }

        public byte FrontCamera
        {
            get { return _FrontCamera; }
            set { _FrontCamera = value; }
        }

        public byte BackSwitchTo
        {
            get { return _BackSwitchTo; }
            set { _BackSwitchTo = value; }
        }

        public byte BackCamera
        {
            get { return _BackCamera; }
            set { _BackCamera = value; }
        }

        public ZActor() { }

        public ZActor(byte frontswitchto, byte frontcamera, byte backswitchto, byte backcamera, ushort number, float xpos, float ypos, float zpos, float yrot, ushort variable)
        {
            IsTransition = true;
            _Number = number;
            _FrontSwitchTo = frontswitchto;
            _FrontCamera = frontcamera;
            _BackSwitchTo = backswitchto;
            _BackCamera = backcamera;
            _XPos = xpos;
            _YPos = ypos;
            _ZPos = zpos;
            _XRot = 0.0f;
            _YRot = yrot;
            _ZRot = 0.0f;
            _Variable = variable;
        }

        public ZActor(ushort number, float xpos, float ypos, float zpos, float xrot, float yrot, float zrot, ushort variable)
        {
            IsTransition = false;
            _FrontSwitchTo = 0xDE;
            _FrontCamera = 0xAD;
            _BackSwitchTo = 0xBE;
            _BackCamera = 0xEF;
            _Number = number;
            _XPos = xpos;
            _YPos = ypos;
            _ZPos = zpos;
            _XRot = xrot;
            _YRot = yrot;
            _ZRot = zrot;
            _Variable = variable;
        }
    }
}

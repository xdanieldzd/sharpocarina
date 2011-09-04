using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace SharpOcarina
{
    public class ZScene
    {
        #region Classes, Variables, etc.

        public class ZRoom
        {
            public string ModelFilename = string.Empty;

            [XmlIgnore]
            private string _ModelShortFilename = string.Empty;

            public int InjectOffset;
            [XmlIgnore]
            public int FullDataLength;

            public List<ZUShort> ZObjects = null;
            public List<ZActor> ZActors = null;
            [XmlIgnore]
            public ObjFile ObjModel = null;
            [XmlIgnore]
            public List<NDisplayList> DLists = null;

            public string ModelShortFilename
            {
                get { return _ModelShortFilename; }
                set { _ModelShortFilename = value; }
            }

            public class ZGroupSettings
            {
                public uint[] TintAlpha = new uint[1];
                public int[] TileS = new int[1];
                public int[] TileT = new int[1];
                public int[] PolyType = new int[1];
                public bool[] BackfaceCulling = new bool[1];
            }

            public ZGroupSettings GroupSettings = new ZGroupSettings();

            [XmlIgnore]
            public List<byte> RoomData;

            public ZRoom() { }
        }

        public class ZUShort
        {
            [XmlIgnore]
            private ushort _Value;

            [XmlIgnore]
            public string ValueHex
            {
                get { return _Value.ToString("X4"); }
            }

            public ushort Value
            {
                get { return _Value; }
                set { _Value = value; }
            }

            public ZUShort() { }

            public ZUShort(ushort value)
            {
                _Value = value;
            }
        }

        public string Name;
        public float Scale;
        public byte Music;
        public bool IsOutdoors;
        public int InjectOffset = 0x02D00000;
        public int SceneNumber = 108;

        public List<ZRoom> Rooms
        {
            get { return _Rooms; }
            set { _Rooms = value; }
        }

        /* Scene data */
        public string CollisionFilename = string.Empty;
        [XmlIgnore]
        public ObjFile ColModel = null;
        public List<ZActor> Transitions = new List<ZActor>();
        public List<ZActor> SpawnPoints = new List<ZActor>();
        public List<ZEnvironment> Environments = new List<ZEnvironment>();
        public List<ZWaterbox> Waterboxes = new List<ZWaterbox>();
        public List<ZColPolyType> PolyTypes = new List<ZColPolyType>();
        public List<ZUShort> ExitList = new List<ZUShort>();

        [XmlIgnore]
        private List<byte> SceneData;

        [XmlIgnore]
        private int CmdCollisionOffset = -1, CmdMapListOffset = -1, CmdTransitionsOffset = -1, CmdExitListOffset = -1, CmdSpawnPointOffset = -1, CmdEnvironmentsOffset = -1, CmdEntranceListOffset = -1;

        /* Room data */
        private List<ZRoom> _Rooms = new List<ZRoom>();

        private string _BasePath = string.Empty;

        [XmlIgnore]
        const uint Dummy = 0xDEADBEEF;

        [XmlIgnore]
        private int CmdMeshHeaderOffset = -1, CmdObjectOffset = -1, CmdActorOffset = -1;
        [XmlIgnore]
        private int MeshHeaderOffset, ObjectOffset, ActorOffset;
        [XmlIgnore]
        private List<NTexture> Textures;

        [XmlIgnore]
        public string BasePath
        {
            get { return _BasePath; }
            set { _BasePath = value; }
        }

        #endregion

        #region Constructors, Basic Functions

        public ZScene() { }

        public ZScene(string name, float scale, byte music)
        {
            Name = name;
            Scale = scale;
            Music = music;
        }

        public void Prepare()
        {
            foreach (ZRoom Room in _Rooms)
            {
                Room.ObjModel.BasePath = BasePath;
                Room.ObjModel.Prepare();
            }
        }

        #endregion

        #region Helper Functions

        private void AddPadding(ref List<byte> Data, int Length)
        {
            int ToAdd = Length - (Data.Count % Length);
            if (ToAdd != Length) for (int i = 0; i < ToAdd; i++) Data.Add(0);
        }

        public void AddRoom(string Filename)
        {
            ZRoom NewRoom = new ZRoom();

            NewRoom.ModelFilename = Filename;
            NewRoom.ModelShortFilename = Path.GetFileNameWithoutExtension(Filename);
            NewRoom.ObjModel = new ObjFile(Filename);
            NewRoom.ZObjects = new List<ZUShort>();
            NewRoom.ZActors = new List<ZActor>();
            NewRoom.InjectOffset = 0x035CF000;
            NewRoom.GroupSettings.TintAlpha = new uint[NewRoom.ObjModel.Groups.Count];
            NewRoom.GroupSettings.TileS = new int[NewRoom.ObjModel.Groups.Count];
            NewRoom.GroupSettings.TileT = new int[NewRoom.ObjModel.Groups.Count];
            NewRoom.GroupSettings.PolyType = new int[NewRoom.ObjModel.Groups.Count];
            NewRoom.GroupSettings.BackfaceCulling = new bool[NewRoom.ObjModel.Groups.Count];
            for (int i = 0; i < NewRoom.ObjModel.Groups.Count; i++)
            {
                NewRoom.GroupSettings.TintAlpha[i] = 0xFFFFFFFF;
                NewRoom.GroupSettings.TileS[i] = GBI.G_TX_WRAP;
                NewRoom.GroupSettings.TileT[i] = GBI.G_TX_WRAP;
                NewRoom.GroupSettings.PolyType[i] = 0x0000000000000000;
                NewRoom.GroupSettings.BackfaceCulling[i] = true;
            }

            _Rooms.Add(NewRoom);
        }

        #endregion

        #region Saving/Injection...

        public void ConvertInject(string Filename, bool ConsecutiveRoomInject)
        {
            ConvertScene(ConsecutiveRoomInject);

            // Crude inject method he~re
            if (ConsecutiveRoomInject == true)
            {
                int RoomInjectOffset = _Rooms[0].InjectOffset;
                for (int i = 0; i < _Rooms.Count; i++)
                {
#if DEBUG
                    Console.WriteLine("INJECTING TO " + Filename + ", OFFSET " + RoomInjectOffset.ToString("X"));
#endif
                    Helpers.GenericInject(Filename, RoomInjectOffset, _Rooms[i].RoomData.ToArray(), _Rooms[i].RoomData.Count);
                    RoomInjectOffset += _Rooms[i].FullDataLength;
                }
            }
            else
            {
                for (int i = 0; i < _Rooms.Count; i++)
                {
#if DEBUG
                    Console.WriteLine("INJECTING TO " + Filename + ", OFFSET " + _Rooms[i].InjectOffset.ToString("X"));
#endif
                    Helpers.GenericInject(Filename, _Rooms[i].InjectOffset, _Rooms[i].RoomData.ToArray(), _Rooms[i].RoomData.Count);
                }
            }

#if DEBUG
            Console.WriteLine("INJECTING TO " + Filename + ", OFFSET " + InjectOffset.ToString("X"));
#endif
            Helpers.GenericInject(Filename, InjectOffset, SceneData.ToArray(), SceneData.Count);

            List<byte> Temp = new List<byte>();
            Helpers.Append32(ref Temp, (uint)InjectOffset);
            Helpers.Append32(ref Temp, (uint)(InjectOffset + SceneData.Count));

            BinaryWriter BWS = new BinaryWriter(File.OpenWrite(Filename));
            int TableOffset = 0xBA0BB0 + (SceneNumber * 0x14);
            BWS.Seek(TableOffset, SeekOrigin.Begin);
            BWS.Write(Temp.ToArray());
            BWS.Close();
        }

        public void ConvertSave(string Filepath, bool ConsecutiveRoomInject)
        {
            ConvertScene(ConsecutiveRoomInject);

            for (int i = 0; i < _Rooms.Count; i++)
            {
                string SaveRoomTo = Filepath + Helpers.MakeValidFileName(Name) + " (Room " + i.ToString() + ").zmap";
#if DEBUG
                Console.WriteLine("SAVING DATA TO " + SaveRoomTo);
#endif
                BinaryWriter BWR = new BinaryWriter(File.OpenWrite(SaveRoomTo));
                BWR.Write(_Rooms[i].RoomData.ToArray());
                BWR.Close();
            }

            string SaveSceneTo = Filepath + Helpers.MakeValidFileName(Name) + " (Scene).zscene";
#if DEBUG
            Console.WriteLine("SAVING DATA TO " + SaveSceneTo);
#endif
            BinaryWriter BWS = new BinaryWriter(File.OpenWrite(SaveSceneTo));
            BWS.Write(SceneData.ToArray());
            BWS.Close();
        }

        #endregion

        #region ... Conversion

        public void ConvertScene(bool ConsecutiveRoomInject)
        {
            /* Check if collision model is valid */
            if (ColModel == null) throw new Exception("No collision model defined");

            /* Process rooms... */
            for (int i = 0; i < _Rooms.Count; i++)
            {
                /* Get current room from list */
                ZRoom Room = _Rooms[i];

                /* Create new room file, DList offset list and texture list */
                Room.RoomData = new List<byte>();
                Room.DLists = new List<NDisplayList>();
                Textures = new List<NTexture>();

                /* Create room header */
                WriteRoomHeader(Room);

                /* Write objects */
                if (Room.ZObjects.Count != 0)
                {
                    ObjectOffset = Room.RoomData.Count;
                    foreach (ZUShort Obj in Room.ZObjects)
                        Helpers.Append16(ref Room.RoomData, Obj.Value);
                    AddPadding(ref Room.RoomData, 8);
                }

                /* Write actors */
                if (Room.ZActors.Count != 0)
                {
                    ActorOffset = Room.RoomData.Count;
                    foreach (ZActor Actor in Room.ZActors)
                    {
                        Helpers.Append16(ref Room.RoomData, Actor.Number);
                        Helpers.Append16(ref Room.RoomData, (ushort)Actor.XPos);
                        Helpers.Append16(ref Room.RoomData, (ushort)Actor.YPos);
                        Helpers.Append16(ref Room.RoomData, (ushort)Actor.ZPos);
                        Helpers.Append16(ref Room.RoomData, (ushort)Actor.XRot);
                        Helpers.Append16(ref Room.RoomData, (ushort)Actor.YRot);
                        Helpers.Append16(ref Room.RoomData, (ushort)Actor.ZRot);
                        Helpers.Append16(ref Room.RoomData, Actor.Variable);
                    }
                    AddPadding(ref Room.RoomData, 8);
                }

                /* Prepare dummy mesh header */
                MeshHeaderOffset = Room.RoomData.Count;
                Helpers.Append32(ref Room.RoomData, 0);  /* Mesh type X, Y meshes */
                Helpers.Append32(ref Room.RoomData, 0);  /* Start address */
                Helpers.Append32(ref Room.RoomData, 0);  /* End address */
                for (int j = 0; j < Room.ObjModel.Groups.Count; j++)
                {
                    Helpers.Append64(ref Room.RoomData, 0);
                    Helpers.Append32(ref Room.RoomData, 0);  /* Display List offset 1 */
                    Helpers.Append32(ref Room.RoomData, 0);  /* Display List offset 2 */
                }
                AddPadding(ref Room.RoomData, 8);

                /* Create textures */
                foreach (ObjFile.Material Mat in Room.ObjModel.Materials)
                {
                    if (Mat.TexImage == null) continue;

                    /* Create new texture, convert current material */
                    NTexture Texture = new NTexture();
                    Texture.Convert(Mat);

                    /* Add current offset to texture offset list */
                    Texture.TexOffset = ((uint)Room.RoomData.Count);
                    /* Write converted data to room file */
                    Room.RoomData.AddRange(Texture.Data);

                    /* See if we've got a CI-format texture... */
                    int Format = ((Texture.Type & 0xE0) >> 5);
#if DEBUG
                    Console.WriteLine("Texture format N64: " + Format.ToString("X2"));
#endif
                    if (Format == GBI.G_IM_FMT_CI)
                    {
                        /* If it's CI, add current offset to palette offset list */
                        Texture.PalOffset = ((uint)Room.RoomData.Count);
                        /* Write palette data to room file */
                        Room.RoomData.AddRange(Texture.Palette);
                    }
                    else
                    {
                        /* Add dummy entry to palette offset list */
                        Texture.PalOffset = Dummy;
                    }

                    Textures.Add(Texture);
                }

                /* Create Display Lists */
                for (int j = 0; j < Room.ObjModel.Groups.Count; j++)
                {
                    NDisplayList DList = new NDisplayList(Scale, Room.ObjModel.Groups[j].TintAlpha, 1.0f, IsOutdoors, Room.ObjModel.Groups[j].BackfaceCulling);
                    DList.Convert(Room.ObjModel, j, Textures, (uint)Room.RoomData.Count);

                    if (DList.Data != null)
                        Room.RoomData.AddRange(DList.Data);

                    Room.DLists.Add(DList);
                }

                /* Fix room header and add missing data */
                FixRoomHeader(Room);

                /* Add some padding for good measure */
                AddPadding(ref Room.RoomData, 0x1000);

                /* Store room data length */
                Room.FullDataLength = Room.RoomData.ToArray().Length;

                /* Put modified room info back into list */
                _Rooms[i] = Room;
            }

            /* Create new scene file */
            SceneData = new List<byte>();

            /* Write scene header */
            Helpers.Append64(ref SceneData, (ulong)(0x1502000000000000 | Music));       /* Sound settings */
            CmdMapListOffset = SceneData.Count;
            Helpers.Append64(ref SceneData, 0x0400000000000000);                        /* Map list */
            CmdTransitionsOffset = SceneData.Count;
            Helpers.Append64(ref SceneData, 0x0E00000000000000);                        /* Transition list */
            Helpers.Append64(ref SceneData, 0x1900000000000003);                        /* Cutscenes */
            CmdCollisionOffset = SceneData.Count;
            Helpers.Append64(ref SceneData, 0x0300000000000000);                        /* Collision header */
            CmdEntranceListOffset = SceneData.Count;
            Helpers.Append64(ref SceneData, 0x0600000000000000);                        /* Entrance index */

            if (IsOutdoors == true)                                                     /* Special objects */
                Helpers.Append64(ref SceneData, 0x0701000000000002);
            else
                Helpers.Append64(ref SceneData, 0x0702000000000003);

            CmdSpawnPointOffset = SceneData.Count;
            Helpers.Append64(ref SceneData, 0x0000000000000000);                        /* Spawn point list */

            if (IsOutdoors == true)                                                     /* Skybox / lighting settings */
                Helpers.Append64(ref SceneData, 0x1100000001000000);
            else
                Helpers.Append64(ref SceneData, 0x1100000000000100);

            CmdExitListOffset = SceneData.Count;
            Helpers.Append64(ref SceneData, 0x1300000000000000);                        /* Exit list */
            CmdEnvironmentsOffset = SceneData.Count;
            Helpers.Append64(ref SceneData, 0x0F00000000000000);                        /* Environments */
            Helpers.Append64(ref SceneData, 0x1400000000000000);                        /* End marker */

            /* Fix scene header; map list ... */
            Helpers.Overwrite32(ref SceneData, CmdMapListOffset, (uint)(0x04000000 | (_Rooms.Count << 16)));
            Helpers.Overwrite32(ref SceneData, CmdMapListOffset + 4, (uint)(0x02000000 | SceneData.Count));
            if (ConsecutiveRoomInject == true)
            {
                int RoomInjectOffset = _Rooms[0].InjectOffset;
                foreach (ZRoom Room in _Rooms)
                {
                    Helpers.Append32(ref SceneData, (uint)RoomInjectOffset);
                    Helpers.Append32(ref SceneData, (uint)(RoomInjectOffset + Room.FullDataLength));
                    RoomInjectOffset += Room.FullDataLength;
                }
            }
            else
            {
                foreach (ZRoom Room in _Rooms)
                {
                    Helpers.Append32(ref SceneData, (uint)Room.InjectOffset);
                    Helpers.Append32(ref SceneData, (uint)(Room.InjectOffset + Room.FullDataLength));
                }
            }
            AddPadding(ref SceneData, 8);

            /* ... transition list ... */
            Helpers.Overwrite32(ref SceneData, CmdTransitionsOffset, (uint)(0x0E000000 | (Transitions.Count << 16)));
            Helpers.Overwrite32(ref SceneData, CmdTransitionsOffset + 4, (uint)(0x02000000 | SceneData.Count));
            foreach (ZActor Trans in Transitions)
            {
                SceneData.Add(Trans.FrontSwitchTo);
                SceneData.Add(Trans.FrontCamera);
                SceneData.Add(Trans.BackSwitchTo);
                SceneData.Add(Trans.BackCamera);
                Helpers.Append16(ref SceneData, Trans.Number);
                Helpers.Append16(ref SceneData, (ushort)Trans.XPos);
                Helpers.Append16(ref SceneData, (ushort)Trans.YPos);
                Helpers.Append16(ref SceneData, (ushort)Trans.ZPos);
                Helpers.Append16(ref SceneData, (ushort)Trans.YRot);
                Helpers.Append16(ref SceneData, Trans.Variable);
            }
            AddPadding(ref SceneData, 8);

            /* ... exit list ... */
            Helpers.Overwrite32(ref SceneData, CmdExitListOffset + 4, (uint)(0x02000000 | SceneData.Count));
            foreach (ZUShort Exit in ExitList)
            {
                Helpers.Append16(ref SceneData, Exit.Value);
            }
            AddPadding(ref SceneData, 8);

            /* ... spawn point list ... */
            Helpers.Overwrite32(ref SceneData, CmdSpawnPointOffset, (uint)(0x00000000 | (SpawnPoints.Count << 16)));
            Helpers.Overwrite32(ref SceneData, CmdSpawnPointOffset + 4, (uint)(0x02000000 | SceneData.Count));
            foreach (ZActor Spawn in SpawnPoints)
            {
                Helpers.Append16(ref SceneData, Spawn.Number);
                Helpers.Append16(ref SceneData, (ushort)Spawn.XPos);
                Helpers.Append16(ref SceneData, (ushort)Spawn.YPos);
                Helpers.Append16(ref SceneData, (ushort)Spawn.ZPos);
                Helpers.Append16(ref SceneData, (ushort)Spawn.XRot);
                Helpers.Append16(ref SceneData, (ushort)Spawn.YRot);
                Helpers.Append16(ref SceneData, (ushort)Spawn.ZRot);
                Helpers.Append16(ref SceneData, Spawn.Variable);
            }
            AddPadding(ref SceneData, 8);

            /* ... environments ... */
            Helpers.Overwrite32(ref SceneData, CmdEnvironmentsOffset, (uint)(0x0F000000 | (Environments.Count << 16)));
            Helpers.Overwrite32(ref SceneData, CmdEnvironmentsOffset + 4, (uint)(0x02000000 | SceneData.Count));
            foreach (ZEnvironment Env in Environments)
            {
                Helpers.Append48(ref SceneData, (ulong)(Env.C1C.ToArgb() & 0xFFFFFF));
                Helpers.Append48(ref SceneData, (ulong)(Env.C2C.ToArgb() & 0xFFFFFF));
                Helpers.Append48(ref SceneData, (ulong)(Env.C3C.ToArgb() & 0xFFFFFF));
                Helpers.Append48(ref SceneData, (ulong)(Env.C4C.ToArgb() & 0xFFFFFF));
                Helpers.Append48(ref SceneData, (ulong)(Env.C5C.ToArgb() & 0xFFFFFF));
                Helpers.Append48(ref SceneData, (ulong)(Env.FogColorC.ToArgb() & 0xFFFFFF));
                Helpers.Append16(ref SceneData, Env.FogDistance);
                Helpers.Append16(ref SceneData, Env.DrawDistance);
            }
            AddPadding(ref SceneData, 8);

            /* ... entrance list ... */
            Helpers.Overwrite32(ref SceneData, CmdEntranceListOffset + 4, (uint)(0x02000000 | SceneData.Count));
            Helpers.Append16(ref SceneData, 0x0000);    /* Map 0, spawn point 0 */
            AddPadding(ref SceneData, 8);

            /* ... collision */
            WriteSceneCollision();
        }

        #region Collision

        private void WriteSceneCollision()
        {
            /* Fix scene header */
            Helpers.Overwrite32(ref SceneData, CmdCollisionOffset + 4, (uint)(0x02000000 | SceneData.Count));

            /* Determine collision's minimum/maximum coordinates... */
            OpenTK.Vector3d MinCoordinate = new OpenTK.Vector3d(0, 0, 0);
            OpenTK.Vector3d MaxCoordinate = new OpenTK.Vector3d(0, 0, 0);

            foreach (ObjFile.Vertex Vtx in ColModel.Vertices)
            {
                /* Minimum... */
                MinCoordinate.X = Math.Min(MinCoordinate.X, Vtx.X * Scale);
                MinCoordinate.Y = Math.Min(MinCoordinate.Y, Vtx.Y * Scale);
                MinCoordinate.Z = Math.Min(MinCoordinate.Z, Vtx.Z * Scale);

                /* Maximum... */
                MaxCoordinate.X = Math.Max(MaxCoordinate.X, Vtx.X * Scale);
                MaxCoordinate.Y = Math.Max(MaxCoordinate.Y, Vtx.Y * Scale);
                MaxCoordinate.Z = Math.Max(MaxCoordinate.Z, Vtx.Z * Scale);
            }

            /* Prepare variables */
            int CmdVertexArray = -1, CmdPolygonArray = -1, CmdPolygonTypes = -1, CmdWaterBoxes = -1;
            int VertexArrayOffset = -1, PolygonArrayOffset = -1, PolygonTypesOffset = -1, WaterBoxesOffset = -1;

            /* Write collision header */
            Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(MinCoordinate.X));  /* Absolute minimum X/Y/Z */
            Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(MinCoordinate.Y));
            Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(MinCoordinate.Z));
            Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(MaxCoordinate.X));  /* Absolute maximum X/Y/Z */
            Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(MaxCoordinate.Y));
            Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(MaxCoordinate.Z));
            CmdVertexArray = SceneData.Count;
            Helpers.Append32(ref SceneData, 0x00000000);                                /* Vertex count */
            Helpers.Append32(ref SceneData, 0x00000000);                                /* Vertex array offset */
            CmdPolygonArray = SceneData.Count;
            Helpers.Append32(ref SceneData, 0x00000000);                                /* Polygon count */
            Helpers.Append32(ref SceneData, 0x00000000);                                /* Polygon array offset */
            CmdPolygonTypes = SceneData.Count;
            Helpers.Append32(ref SceneData, 0x00000000);                                /* Polygon type offset */
            Helpers.Append32(ref SceneData, 0x00000000);                                /* Camera data offset (NULL) */
            CmdWaterBoxes = SceneData.Count;
            Helpers.Append32(ref SceneData, 0x00000000);                                /* Waterbox count */
            Helpers.Append32(ref SceneData, 0x00000000);                                /* Waterbox offset */

            AddPadding(ref SceneData, 8);

            /* Write vertex array & fix command */
            VertexArrayOffset = SceneData.Count;
            foreach (ObjFile.Vertex Vtx in ColModel.Vertices)
            {
                Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(Vtx.X * Scale));
                Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(Vtx.Y * Scale));
                Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(Vtx.Z * Scale));
            }
            Helpers.Overwrite32(ref SceneData, CmdVertexArray, (uint)(ColModel.Vertices.Count << 16));
            Helpers.Overwrite32(ref SceneData, CmdVertexArray + 4, (uint)(0x02000000 | VertexArrayOffset));

            AddPadding(ref SceneData, 8);

            /* Write polygon array & fix command */
            PolygonArrayOffset = SceneData.Count;
            int TriangleTotal = 0;
            foreach (ObjFile.Group Group in ColModel.Groups)
            {
                foreach (ObjFile.Triangle Tri in Group.Triangles)
                {
                    Helpers.Append16(ref SceneData, (ushort)Group.PolyType);    /* Polygon type */
                    Helpers.Append16(ref SceneData, (ushort)Tri.VertIndex[0]);  /* Index of vertex 1 */
                    Helpers.Append16(ref SceneData, (ushort)Tri.VertIndex[1]);  /* Index of vertex 2 */
                    Helpers.Append16(ref SceneData, (ushort)Tri.VertIndex[2]);  /* Index of vertex 3 */
                    Helpers.Append16(ref SceneData, 0x7FFF);                    /* Collision normals X/Y/Z */
                    Helpers.Append16(ref SceneData, 0x7FFF);
                    Helpers.Append16(ref SceneData, 0x7FFF);
                    Helpers.Append16(ref SceneData, 0x0000);                    /* Distance from origin */
                }
                TriangleTotal += Group.Triangles.Count;
            }
            Helpers.Overwrite32(ref SceneData, CmdPolygonArray, (uint)(TriangleTotal << 16));
            Helpers.Overwrite32(ref SceneData, CmdPolygonArray + 4, (uint)(0x02000000 | PolygonArrayOffset));

            FixCollision(ref SceneData, VertexArrayOffset, PolygonArrayOffset, TriangleTotal);

            AddPadding(ref SceneData, 8);

            /* Write polygon types & fix command */
            PolygonTypesOffset = SceneData.Count;
            foreach (ZColPolyType PT in PolyTypes)
                Helpers.Append64(ref SceneData, PT.Raw);
            Helpers.Overwrite32(ref SceneData, CmdPolygonTypes, (uint)(0x02000000 | PolygonTypesOffset));

            AddPadding(ref SceneData, 8);

            /* Write waterboxes & fix command */
            WaterBoxesOffset = SceneData.Count;
            foreach (ZWaterbox WBox in Waterboxes)
            {
                Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(WBox.XPos));
                Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(WBox.YPos));
                Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(WBox.ZPos));
                Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(WBox.XSize));
                Helpers.Append16(ref SceneData, (ushort)Convert.ToInt16(WBox.ZSize));
                Helpers.Append32(ref SceneData, 0x00000000);
                Helpers.Append16(ref SceneData, WBox.Properties);
            }
            Helpers.Overwrite32(ref SceneData, CmdWaterBoxes, (uint)(Waterboxes.Count << 16));
            Helpers.Overwrite32(ref SceneData, CmdWaterBoxes + 4, (uint)(0x02000000 | WaterBoxesOffset));

            /* Padding for good measure */
            AddPadding(ref SceneData, 0x800);
        }

        /* Algorithm by MN, implementation by JSA, C version by spinout: http://wiki.spinout182.com/w/Zelda_64:_Collision_Normals
         * Fixed for good by DeathBasket: http://core.the-gcn.com/index.php?/topic/675-sharpocarina-zelda-oot-scene-development-system/page__view__findpost__p__11060
         */
        private void FixCollision(ref List<byte> Data, int VertOff, int TriOff, int TriCount)
        {
            int i, pos, end = TriOff + (TriCount << 4);
            int v1, v2, v3, dn;
            int[] p1 = new int[3], p2 = new int[3], p3 = new int[3], dx = new int[2], dy = new int[2], dz = new int[2], ni = new int[3];
            float nd;
            float[] nf = new float[3], uv = new float[3];

            for (pos = TriOff; pos < end; pos += 0x10)
            {
                v1 = Helpers.Read16(Data, pos + 2);
                v2 = Helpers.Read16(Data, pos + 4);
                v3 = Helpers.Read16(Data, pos + 6);

                for (i = 0; i < 3; i++)
                {
                    p1[i] = Helpers.Read16S(Data, VertOff + (v1 * 0x6) + (i << 1));
                    p2[i] = Helpers.Read16S(Data, VertOff + (v2 * 0x6) + (i << 1));
                    p3[i] = Helpers.Read16S(Data, VertOff + (v3 * 0x6) + (i << 1));
                }

                dx[0] = p1[0] - p2[0]; dx[1] = p2[0] - p3[0];
                dy[0] = p1[1] - p2[1]; dy[1] = p2[1] - p3[1];
                dz[0] = p1[2] - p2[2]; dz[1] = p2[2] - p3[2];

                nf[0] = (float)(dy[0] * dz[1]) - (dz[0] * dy[1]);
                nf[1] = (float)(dz[0] * dx[1]) - (dx[0] * dz[1]);
                nf[2] = (float)(dx[0] * dy[1]) - (dy[0] * dx[1]);

                /* calculate length of normal vector */
                nd = (float)Math.Sqrt((nf[0] * nf[0]) + (nf[1] * nf[1]) + (nf[2] * nf[2]));

                for (i = 0; i < 3; i++)
                {
                    if (nd != 0)
                        uv[i] = nf[i] / nd; /* uv being the unit normal vector */
                    nf[i] = uv[i] * 0x7FFF;   /* nf being the way OoT uses it */
                }

                /* distance from origin... */
                dn = (int)Math.Round(((uv[0] * p1[0]) + (uv[1] * p1[1]) + (uv[2] * p1[2])) * -1);

                if (dn < 0)
                    dn += 0x10000;
                Helpers.Overwrite16(ref Data, pos + 0xE, (ushort)(dn & 0xFFFF));
                for (i = 0; i < 3; i++)
                {
                    ni[i] = (int)Math.Round(nf[i]);
                    if (ni[i] < 0)
                        ni[i] += 0x10000;
                    Helpers.Overwrite16(ref Data, (pos + 8 + (i << 1)), (ushort)(ni[i] & 0xFFFF));
                }
            }
        }

        #endregion

        #region Header Writing/Fixing

        private void WriteRoomHeader(ZRoom Room)
        {
            /* Write room header */
            Helpers.Append64(ref Room.RoomData, 0x1600000000000000);        /* Sound settings */
            Helpers.Append64(ref Room.RoomData, 0x0800000000000000);        /* Unknown */
            Helpers.Append64(ref Room.RoomData, 0x1200000000000000);        /* Skybox modifier */

            if (IsOutdoors == true)                                         /* Time settings */
                Helpers.Append64(ref Room.RoomData, 0x10000000FFFF0A00);
            else
                Helpers.Append64(ref Room.RoomData, 0x10000000FFFF0000);

            CmdMeshHeaderOffset = Room.RoomData.Count;
            Helpers.Append64(ref Room.RoomData, 0x0A00000000000000);        /* Mesh header */

            /* Objects */
            if (Room.ZObjects.Count != 0)
            {
                CmdObjectOffset = Room.RoomData.Count;
                Helpers.Append64(ref Room.RoomData, 0x0B00000000000000);
            }

            /* Actors */
            if (Room.ZActors.Count != 0)
            {
                CmdActorOffset = Room.RoomData.Count;
                Helpers.Append64(ref Room.RoomData, 0x0100000000000000);
            }

            Helpers.Append64(ref Room.RoomData, 0x1400000000000000);        /* End marker */
        }

        private void FixRoomHeader(ZRoom Room)
        {
            /* Fix room header commands; mesh header... */
            if (CmdMeshHeaderOffset != -1)
                Helpers.Overwrite32(ref Room.RoomData, CmdMeshHeaderOffset + 4, (uint)(0x03000000 | MeshHeaderOffset)); /* Mesh header */

            /* ...object list... */
            if (Room.ZObjects.Count != 0 && CmdObjectOffset != -1)
            {
                Helpers.Overwrite32(ref Room.RoomData, CmdObjectOffset, (uint)(0x0B000000 | (Room.ZObjects.Count << 16))); /* Objects */
                Helpers.Overwrite32(ref Room.RoomData, CmdObjectOffset + 4, (uint)(0x03000000 | ObjectOffset));
            }

            /* ...actor list */
            if (Room.ZActors.Count != 0 && CmdActorOffset != -1)
            {
                Helpers.Overwrite32(ref Room.RoomData, CmdActorOffset, (uint)(0x01000000 | (Room.ZActors.Count << 16))); /* Actors */
                Helpers.Overwrite32(ref Room.RoomData, CmdActorOffset + 4, (uint)(0x03000000 | ActorOffset));
            }

            /* Write mesh header */
            if (MeshHeaderOffset != -1)
            {
                //Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset, (uint)(0x02000000 | (Room.DLists.Count << 16)));
                //Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset + 4, (uint)(0x03000000 | MeshHeaderOffset + 12));
                //Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset + 8, (uint)(0x03000000 | (MeshHeaderOffset + 12) + 16));
                //MeshHeaderOffset += 12;

                //foreach (NDisplayList DList in Room.DLists)
                //{
                //    ushort MaxX = (ushort)System.Convert.ToInt16(DList.MaxCoordinate.X);
                //    ushort MaxZ = (ushort)System.Convert.ToInt16(DList.MaxCoordinate.Z);
                //    ushort MinX = (ushort)System.Convert.ToInt16(DList.MinCoordinate.X);
                //    ushort MinZ = (ushort)System.Convert.ToInt16(DList.MinCoordinate.Z);

                //    Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset, (uint)((MaxX << 16) | MaxZ));
                //    Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset + 4, (uint)((MinX << 16) | MinZ));

                //    if (DList.TranslucentAlpha == 0xFF)
                //        Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset + 8, (uint)(0x03000000 | DList.Offset));     /* Primary Display List, is opaque */
                //    else
                //        Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset + 12, (uint)(0x03000000 | DList.Offset));    /* Secondary Display List, is translucent */

                //    MeshHeaderOffset += 16;
                //}

                Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset, (uint)(0x00000000 | ((Room.DLists.Count) << 16)));
                Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset + 4, (uint)(0x03000000 | MeshHeaderOffset + 12));
                Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset + 8, (uint)(0x03000000 | (MeshHeaderOffset + 12) + (Room.DLists.Count * 4)));
                MeshHeaderOffset += 12;

                /* Opaque Display Lists */
                foreach (NDisplayList DList in Room.DLists.FindAll(delegate(NDisplayList DL) { return (DL.TintAlpha >> 24) == 255; }))
                {
                    Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset, (uint)(0x03000000 | DList.Offset));
                    MeshHeaderOffset += 4;
                }

                /* Translucent Display List */
                foreach (NDisplayList DList in Room.DLists.FindAll(delegate(NDisplayList DL) { return (DL.TintAlpha >> 24) != 255; }))
                {
                    Helpers.Overwrite32(ref Room.RoomData, MeshHeaderOffset, (uint)(0x03000000 | DList.Offset));
                    MeshHeaderOffset += 4;
                }
            }
        }

        #endregion

        #endregion
    }
}

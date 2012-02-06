using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace SharpOcarina
{
    public class NDisplayList
    {
        #region Variables/Constructor/etc.

        public byte[] Data;
        public uint Offset = 0;

        public float Scale;
        public uint TintAlpha;
        public float TexScale;
        public bool IsOutdoors;
        public bool Culling;

        public Vector3d MinCoordinate = new Vector3d(0, 0, 0);
        public Vector3d MaxCoordinate = new Vector3d(0, 0, 0);

        public class NVertex
        {
            public Vector3d Position;
            public Vector2d TexCoord;
            public Color4 Colors;
            public Vector3d Normals;

            public NVertex(Vector3d _Position, Vector2d _TexCoord, Color4 _Colors, Vector3d _Normals)
            {
                Position = _Position;
                TexCoord = _TexCoord;
                Colors = _Colors;
                Normals = _Normals;
            }
        }

        public struct SurfaceBundle
        {
            public ObjFile.Material Material;
            public List<ObjFile.Triangle> Triangles;
        }

        public NDisplayList()
        {
            Scale = 1.0f;
            TintAlpha = 0xFFFFFFFF;
            TexScale = 1.0f;
            IsOutdoors = false;
            Culling = true;
        }

        public NDisplayList(float _Scale, uint _TintAlpha, float _TexScale, bool _IsOutdoors, bool _Culling)
        {
            Scale = _Scale;
            TintAlpha = _TintAlpha;
            TexScale = _TexScale;
            IsOutdoors = _IsOutdoors;
            Culling = _Culling;
        }

        #endregion

        #region General Macros

        private ulong NoParam(byte Cmd)
        {
            return ((ulong)Helpers.ShiftL(Cmd, 24, 8) << 32) | 0;
        }

        private ulong Param(byte Cmd, uint Param)
        {
            return ((ulong)Helpers.ShiftL(Cmd, 24, 8) << 32) | Param;
        }

        private ulong FullSync()
        {
            return NoParam(GBI.G_RDPFULLSYNC);
        }

        private ulong TileSync()
        {
            return NoParam(GBI.G_RDPTILESYNC);
        }

        private ulong PipeSync()
        {
            return NoParam(GBI.G_RDPPIPESYNC);
        }

        private ulong LoadSync()
        {
            return NoParam(GBI.G_RDPLOADSYNC);
        }

        private ulong NoOp()
        {
            return NoParam(GBI.G_NOOP);
        }

        #endregion

        #region Mode Macros

        private ulong SetRenderMode(uint C0, uint C1)
        {
            return SetOtherMode(GBI.G_SETOTHERMODE_L, GBI.G_MDSFT_RENDERMODE, 29, (C0) | (C1));
        }

        private ulong SetOtherMode(byte Cmd, uint Sft, uint Len_, ulong Data)
        {
            return ((ulong)(Helpers.ShiftL(Cmd, 24, 8) | Helpers.ShiftL((uint)(32 - (Sft) - (Len_)), 8, 8) | Helpers.ShiftL((uint)((Len_) - 1), 0, 8)) << 32) | (Data & 0xFFFFFFFF);
        }

        private ulong GeometryMode(int Clear, int Set)
        {
            return ((ulong)(Helpers.ShiftL(GBI.G_GEOMETRYMODE, 24, 8) | Helpers.ShiftL((~(uint)Clear) & 0xFFFFFFFF, 0, 24)) << 32) | ((uint)Set & 0xFFFFFFFF);
        }

        private ulong SetGeometryMode(int Mode)
        {
            return GeometryMode(0, Mode);
        }

        private ulong ClearGeometryMode(int Mode)
        {
            return GeometryMode(Mode, 0);
        }

        private ulong LoadGeometryMode(int Mode)
        {
            return GeometryMode(-1, Mode);
        }

        #endregion

        #region Texture & Palette Macros

        private ulong SetTextureLUT(uint Type_)
        {
            return SetOtherMode(GBI.G_SETOTHERMODE_H, GBI.G_MDSFT_TEXTLUT, 2, Type_);
        }

        private ulong SetTextureLOD(uint Type_)
        {
            return SetOtherMode(GBI.G_SETOTHERMODE_H, GBI.G_MDSFT_TEXTLOD, 1, Type_);
        }

        private ulong Texture(int S, int T, uint Level, uint Tile, uint On)
        {
            return ((ulong)(Helpers.ShiftL(GBI.G_TEXTURE, 24, 8) | Helpers.ShiftL(GBI.BOWTIE_VAL, 16, 8) | Helpers.ShiftL((Level), 11, 3) | Helpers.ShiftL((Tile), 8, 3) | Helpers.ShiftL((On), 1, 7)) << 32) | (Helpers.ShiftL((uint)(S), 16, 16) | Helpers.ShiftL((uint)(T), 0, 16));
        }

        private ulong SetImage(byte Cmd, int Fmt, int Siz, uint Width, uint I)
        {
            return ((ulong)(Helpers.ShiftL(Cmd, 24, 8) | Helpers.ShiftL((uint)Fmt, 21, 3) | Helpers.ShiftL((uint)Siz, 19, 2) | Helpers.ShiftL((Width) - 1, 0, 12)) << 32) | (I);
        }

        private ulong SetTextureImage(int F, int S, uint W, uint I)
        {
            return SetImage(GBI.G_SETTIMG, F, S, W, I);
        }

        private ulong SetTile(int Fmt, int Siz, int Line, int TMEM, int Tile, int Palette, uint CMT, uint MaskT, uint ShiftT, uint CMS, uint MaskS, uint ShiftS)
        {
            return (
                ((ulong)(Helpers.ShiftL(GBI.G_SETTILE, 24, 8) | Helpers.ShiftL((uint)Fmt, 21, 3) | Helpers.ShiftL((uint)Siz, 19, 2) |
                Helpers.ShiftL((uint)Line, 9, 9) | Helpers.ShiftL((uint)TMEM, 0, 9)) << 32) |
                (Helpers.ShiftL((uint)Tile, 24, 3) | Helpers.ShiftL((uint)Palette, 20, 4) | Helpers.ShiftL(CMT, 18, 2) |
                Helpers.ShiftL(MaskT, 14, 4) | Helpers.ShiftL(ShiftT, 10, 4) | Helpers.ShiftL(CMS, 8, 2) |
                Helpers.ShiftL(MaskS, 4, 4) | Helpers.ShiftL(ShiftS, 0, 4)));
        }

        private ulong LoadBlock(int Tile, int ULS, int ULT, int LRS, int DXT)
        {
            return (
                ((ulong)(Helpers.ShiftL(GBI.G_LOADBLOCK, 24, 8) | Helpers.ShiftL((uint)ULS, 12, 12) | Helpers.ShiftL((uint)ULT, 0, 12)) << 32) |
                (Helpers.ShiftL((uint)Tile, 24, 3) | Helpers.ShiftL(((uint)Math.Min(LRS, GBI.G_TX_LDBLK_MAX_TXL)), 12, 12) |
                Helpers.ShiftL((uint)DXT, 0, 12)));
        }

        private ulong LoadTileGeneric(int C, int Tile, int ULS, int ULT, int LRS, int LRT)
        {
            return (
                ((ulong)(Helpers.ShiftL((uint)C, 24, 8) | Helpers.ShiftL((uint)ULS, 12, 12) | Helpers.ShiftL((uint)ULT, 0, 12)) << 32) |
                Helpers.ShiftL((uint)Tile, 24, 3) | Helpers.ShiftL((uint)LRS, 12, 12) | Helpers.ShiftL((uint)LRT, 0, 12));
        }

        private ulong SetTileSize(int Tile, int ULS, int ULT, int LRS, int LRT)
        {
            return LoadTileGeneric(GBI.G_SETTILESIZE, Tile, ULS, ULT, LRS, LRT);
        }

        private ulong LoadTile(int Tile, int ULS, int ULT, int LRS, int LRT)
        {
            return LoadTileGeneric(GBI.G_LOADTILE, Tile, ULS, ULT, LRS, LRT);
        }

        private void LoadTextureBlock(ref List<byte> DList, uint TImg, int Fmt, int Siz, uint Width, uint Height, uint Pal, uint CMS, uint CMT, uint MaskS, uint MaskT, uint ShiftS, uint ShiftT)
        {
            LoadMultiBlock(ref DList, TImg, 0, GBI.G_TX_RENDERTILE, Fmt, Siz, Width, Height, Pal, CMS, CMT, MaskS, MaskT, ShiftS, ShiftT);
        }

        private void LoadMultiBlock(ref List<byte> DList, uint TImg, int TMem, int RTile, int Fmt, int Siz, uint Width, uint Height, uint Pal, uint CMS, uint CMT, uint MaskS, uint MaskT, uint ShiftS, uint ShiftT)
        {
            Helpers.Append64(ref DList, SetTextureImage(Fmt, GBI.G_IM_LOAD_BLOCK.Get(Siz), 1, TImg));
            Helpers.Append64(ref DList, SetTile(Fmt, GBI.G_IM_LOAD_BLOCK.Get(Siz), 0, TMem, GBI.G_TX_LOADTILE,
                0, CMT, MaskT, ShiftT, CMS, MaskS, ShiftS));
            Helpers.Append64(ref DList, LoadSync());
            Helpers.Append64(ref DList, LoadBlock(GBI.G_TX_LOADTILE, 0, 0,
                (int)((((Width * Height) + GBI.G_IM_INCR.Get(Siz)) >> GBI.G_IM_SHIFT.Get(Siz)) - 1),
                GBI.CALC_DXT((int)Width, GBI.G_IM_BYTES.Get(Siz))));
            Helpers.Append64(ref DList, PipeSync());
            Helpers.Append64(ref DList, SetTile(Fmt, Siz,
                (int)((((Width) * GBI.G_IM_LINE_BYTES.Get(Siz)) + 7) >> 3), TMem,
                RTile, (int)Pal, CMT, MaskT, ShiftT, CMS, MaskS, ShiftS));
            Helpers.Append64(ref DList, SetTileSize(RTile, 0, 0,
                (int)((Width - 1) << GBI.G_TEXTURE_IMAGE_FRAC),
                (int)((Height - 1) << GBI.G_TEXTURE_IMAGE_FRAC)));
        }

        private void LoadTextureBlock_4b(ref List<byte> DList, uint TImg, int Fmt, uint Width, uint Height, uint Pal, uint CMS, uint CMT, uint MaskS, uint MaskT, uint ShiftS, uint ShiftT)
        {
            LoadMultiBlock_4b(ref DList, TImg, 0, GBI.G_TX_RENDERTILE, Fmt, Width, Height, Pal, CMS, CMT, MaskS, MaskT, ShiftS, ShiftT);
        }

        private void LoadMultiBlock_4b(ref List<byte> DList, uint TImg, int TMem, int RTile, int Fmt, uint Width, uint Height, uint Pal, uint CMS, uint CMT, uint MaskS, uint MaskT, uint ShiftS, uint ShiftT)
        {
            Helpers.Append64(ref DList, SetTextureImage(Fmt, GBI.G_IM_SIZ_16b, 1, TImg));
            Helpers.Append64(ref DList, SetTile(Fmt, GBI.G_IM_SIZ_16b, 0, TMem, GBI.G_TX_LOADTILE,
                0, CMT, MaskT, ShiftT, CMS, MaskS, ShiftS));
            Helpers.Append64(ref DList, LoadSync());
            Helpers.Append64(ref DList, LoadBlock(GBI.G_TX_LOADTILE, 0, 0,
                (int)((((Width * Height) + 3) >> 2) - 1),
                GBI.CALC_DXT_4b((int)Width)));
            Helpers.Append64(ref DList, PipeSync());
            Helpers.Append64(ref DList, SetTile(Fmt, GBI.G_IM_SIZ_4b,
                (int)((((Width) >> 1) + 7) >> 3), TMem,
                RTile, (int)Pal, CMT, MaskT, ShiftT, CMS, MaskS, ShiftS));
            Helpers.Append64(ref DList, SetTileSize(RTile, 0, 0,
                (int)((Width - 1) << GBI.G_TEXTURE_IMAGE_FRAC),
                (int)((Height - 1) << GBI.G_TEXTURE_IMAGE_FRAC)));
        }

        private ulong LoadTLUTCmd(int Tile, int Count)
        {
            return ((ulong)Helpers.ShiftL(GBI.G_LOADTLUT, 24, 8) << 32) | (Helpers.ShiftL((uint)Tile, 24, 3) | Helpers.ShiftL((uint)Count, 14, 10));
        }

        private void LoadTLUT16(ref List<byte> DList, int Pal, uint DRAM)
        {
#if DEBUG
            Console.WriteLine("LoadTLUT16 -> pal: " + Pal.ToString() + ", address: " + DRAM.ToString("X8"));
#endif
            Helpers.Append64(ref DList, SetTextureImage(GBI.G_IM_FMT_RGBA, GBI.G_IM_SIZ_16b, 1, (DRAM & 0xFFFFFFFF)));
            Helpers.Append64(ref DList, TileSync());
            Helpers.Append64(ref DList, SetTile(0, 0, 0, (256 + (((Pal) & 0xF) * 16)), GBI.G_TX_LOADTILE, 0, 0, 0, 0, 0, 0, 0));
            Helpers.Append64(ref DList, LoadSync());
            Helpers.Append64(ref DList, LoadTLUTCmd(GBI.G_TX_LOADTILE, 15));
            Helpers.Append64(ref DList, PipeSync());
        }

        private void LoadTLUT256(ref List<byte> DList, uint DRAM)
        {
#if DEBUG
            Console.WriteLine("LoadTLUT256 -> offset: " + DRAM.ToString("X8"));
#endif
            Helpers.Append64(ref DList, SetTextureImage(GBI.G_IM_FMT_RGBA, GBI.G_IM_SIZ_16b, 1, (DRAM & 0xFFFFFFFF)));
            Helpers.Append64(ref DList, TileSync());
            Helpers.Append64(ref DList, SetTile(0, 0, 0, 256, GBI.G_TX_LOADTILE, 0, 0, 0, 0, 0, 0, 0));
            Helpers.Append64(ref DList, LoadSync());
            Helpers.Append64(ref DList, LoadTLUTCmd(GBI.G_TX_LOADTILE, 255));
            Helpers.Append64(ref DList, PipeSync());
        }

        #endregion

        #region Combiner Macros

        private ulong SetPrimColor(uint ARGB)
        {
            return Param(GBI.G_SETPRIMCOLOR, (uint)(((ARGB >> 16) & 0xFF) << 24 | ((ARGB >> 8) & 0xFF) << 16 | (ARGB & 0xFF) << 8 | (ARGB >> 24)));
        }

        private ulong SetPrimColor(byte R, byte G, byte B, byte A)
        {
            return Param(GBI.G_SETPRIMCOLOR, (uint)((R << 24) | (G << 16) | (B << 8) | A));
        }

        private ulong SetCombine(uint MuxS0, uint MuxS1)
        {
            return ((ulong)(Helpers.ShiftL(GBI.G_SETCOMBINE, 24, 8) | Helpers.ShiftL(MuxS0, 0, 24)) << 32) | (MuxS1 & 0xFFFFFFFF);
        }

        #endregion

        #region Conversion

        public void InsertTextureLoad(ref List<byte> DList, int Width, int Height, NTexture ThisTexture, int TexPal, int RenderTile, int CMS, int CMT, int MultiShiftS, int MultiShiftT)
        {
            /* If texture is in CI format, add correct SetTextureLUT and LoadTLUT macro */
            if (ThisTexture.Format == GBI.G_IM_FMT_CI)
            {
                Helpers.Append64(ref DList, SetTextureLUT(GBI.G_TT_RGBA16));

                if (ThisTexture.Size == GBI.G_IM_SIZ_4b)
                    LoadTLUT16(ref DList, TexPal, 0x03000000 | ThisTexture.PalOffset);
                else if (ThisTexture.Size == GBI.G_IM_SIZ_8b)
                    LoadTLUT256(ref DList, 0x03000000 | ThisTexture.PalOffset);
            }
            else
            {
                Helpers.Append64(ref DList, SetTextureLUT(GBI.G_TT_NONE));
            }

            /* Select appropriate load block macro to use (LoadTextureBlock or LoadMultiBlock) */
            if (RenderTile == GBI.G_TX_RENDERTILE)
            {
                /* Select appropriate LoadTextureBlock macro to use (4-bit or standard) */
                if (ThisTexture.Size == GBI.G_IM_SIZ_4b)
                {
                    LoadTextureBlock_4b(ref DList, 0x03000000 | ThisTexture.TexOffset,
                        ThisTexture.Format, (uint)Width, (uint)Height, (uint)TexPal,
                        (uint)CMS, (uint)CMT,
                        (uint)Helpers.Log2(Width), (uint)Helpers.Log2(Height),
                        GBI.G_TX_NOLOD, GBI.G_TX_NOLOD);
                }
                else
                {
                    LoadTextureBlock(ref DList, 0x03000000 | ThisTexture.TexOffset,
                        ThisTexture.Format, ThisTexture.Size, (uint)Width, (uint)Height, (uint)TexPal,
                        (uint)CMS, (uint)CMT,
                        (uint)Helpers.Log2(Width), (uint)Helpers.Log2(Height),
                        GBI.G_TX_NOLOD, GBI.G_TX_NOLOD);
                }
            }
            else
            {
                if (ThisTexture.Size == GBI.G_IM_SIZ_4b)
                {
                    LoadMultiBlock_4b(ref DList, 0x03000000 | ThisTexture.TexOffset, RenderTile * 256, RenderTile,
                        ThisTexture.Format, (uint)Width, (uint)Height, (uint)TexPal,
                        (uint)CMS, (uint)CMT,
                        (uint)Helpers.Log2(Width), (uint)Helpers.Log2(Height),
                        (uint)MultiShiftS, (uint)MultiShiftT);
                }
                else
                {
                    LoadMultiBlock(ref DList, 0x03000000 | ThisTexture.TexOffset, RenderTile * 256, RenderTile,
                        ThisTexture.Format, ThisTexture.Size, (uint)Width, (uint)Height, (uint)TexPal,
                        (uint)CMS, (uint)CMT,
                        (uint)Helpers.Log2(Width), (uint)Helpers.Log2(Height),
                        (uint)MultiShiftS, (uint)MultiShiftT);          // multitex scale HERE!!!
                }
            }
        }

        public void Convert(ObjFile Obj, int Group, List<NTexture> Textures, uint BaseOffset)
        {
            /* Illegal group number? */
            if (Obj.Groups.Count < Group) return;

            /* Create lists, etc. */
            List<byte> DList = new List<byte>();
            List<SurfaceBundle> SurfBundles = new List<SurfaceBundle>();

            List<NVertex> VertList = new List<NVertex>();
            List<byte> VertData = new List<byte>();

            /* Parse all known materials */
            foreach (ObjFile.Material Mat in Obj.Materials)
            {
                /* Create new surface bundle */
                SurfaceBundle Surf = new SurfaceBundle();

                /* Assign material and create triangle list */
                Surf.Material = Mat;
                Surf.Triangles = new List<ObjFile.Triangle>();

                /* Parse triangles and group appropriate tris to bundle */
                foreach (ObjFile.Triangle Tri in Obj.Groups[Group].Triangles)
                {
                    /* If tri's material name matches current material, add it to bundle */
                    if (Tri.MaterialName == Mat.Name) Surf.Triangles.Add(Tri);
                }

                /* Add new surface bundle to list */
                if (Surf.Triangles.Count != 0)
                    SurfBundles.Add(Surf);
            }

            /* Parse surface bundles to create the actual display list */
            foreach (SurfaceBundle Surf in SurfBundles)
            {
                /* General variables, etc. */
                List<byte> AsmTris = new List<byte>();
                bool CommToggle = true;

                /* Generate initial commands */
                Helpers.Append64(ref DList, NoParam(GBI.G_RDPPIPESYNC));
                Helpers.Append64(ref DList, SetTextureLOD(GBI.G_TL_TILE));
                Helpers.Append64(ref DList, Texture(-1, -1, 0, GBI.G_TX_RENDERTILE, GBI.G_ON));

                /* Get texture information */
                NTexture ThisTexture = Textures[Obj.Materials.IndexOf(Surf.Material)];

                /* Texture variables */
                float TexXR = Surf.Material.Width / (32.0f * TexScale);
                float TexYR = Surf.Material.Height / (32.0f * TexScale);
                int TexPal = 0;

                /* Insert texture loading commands */
                InsertTextureLoad(ref DList, Surf.Material.Width, Surf.Material.Height, ThisTexture, TexPal, GBI.G_TX_RENDERTILE, Obj.Groups[Group].TileS, Obj.Groups[Group].TileT, 0, 0);

                if (Obj.Groups[Group].MultiTexMaterial != -1)
                    InsertTextureLoad(ref DList, Obj.Materials[Obj.Groups[Group].MultiTexMaterial].Width, Obj.Materials[Obj.Groups[Group].MultiTexMaterial].Height,
                        Textures[Obj.Groups[Group].MultiTexMaterial], TexPal, GBI.G_TX_RENDERTILE + 1, Obj.Groups[Group].TileS, Obj.Groups[Group].TileT,
                        Obj.Groups[Group].ShiftS, Obj.Groups[Group].ShiftT);

                /* Is surface translucent? (needed later) */
                bool IsTranslucent = ((TintAlpha >> 24) != 255);

                /* Generate GeometryMode commands */
                //Helpers.Append64(ref DList, ClearGeometryMode(GBI.G_TEXTURE_GEN | GBI.G_TEXTURE_GEN_LINEAR | (Culling == false ? GBI.G_CULL_BACK : 0)));
                //Helpers.Append64(ref DList, SetGeometryMode(GBI.G_FOG | GBI.G_LIGHTING | (IsOutdoors == true ? 0 : GBI.G_SHADING_SMOOTH)));
                if (IsOutdoors == true)
                {
                    if (IsTranslucent == true)
                    {
                        Helpers.Append64(ref DList, 0xD9F3FBFF00000000);
                        Helpers.Append64(ref DList, 0xD9FFFFFF00030000);
                    }
                    else
                    {
                        Helpers.Append64(ref DList, 0xD9F3FFFF00000000);
                        Helpers.Append64(ref DList, 0xD9FFFFFF00030400);
                    }
                }
                else
                {
                    if (IsTranslucent == true)
                    {
                        Helpers.Append64(ref DList, 0xD9F1FBFF00000000);
                        Helpers.Append64(ref DList, 0xD9FFFFFF00010000);
                    }
                    else
                    {
                        Helpers.Append64(ref DList, 0xD9F1FFFF00000000);
                        Helpers.Append64(ref DList, 0xD9FFFFFF00010400);
                    }
                }

                /* Generate SetCombine/RenderMode commands */
                if (IsTranslucent == true)
                {
                    /* Translucent surface */
                    Helpers.Append64(ref DList, SetCombine(0x167E03, 0xFF0FFDFF));
                    Helpers.Append64(ref DList, SetRenderMode(0x1C, 0xC81049D8));
                }
                else if (ThisTexture.HasAlpha == true)
                {
                    /* Texture with alpha channel */
                    Helpers.Append64(ref DList, SetCombine(0x127E03, 0xFFFFF3F8));
                    Helpers.Append64(ref DList, SetRenderMode(0x1C, 0xC8103078));
                }
                else
                {
                    /* Solid surface */
                    if (Obj.Groups[Group].MultiTexMaterial != -1)
                        Helpers.Append64(ref DList, SetCombine(0x267E04, 0x1FFCFDF8));
                    else
                        Helpers.Append64(ref DList, SetCombine(0x127E03, 0xFFFFFDF8));

                    Helpers.Append64(ref DList, SetRenderMode(0x1C, 0xC8113078));
                }

                /* Insert SetPrimColor command */
                Helpers.Append64(ref DList, SetPrimColor(TintAlpha));

                /* Parse triangles, generate VTX and TRI commands */
                /* Very heavily based on code from spinout's .obj importer r13 */
                foreach (ObjFile.Triangle Tri in Surf.Triangles)
                {
                    int TriIndex = Surf.Triangles.IndexOf(Tri);

                    int[] TriPoints = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        NVertex NewVert = new NVertex(
                            new Vector3d(Obj.Vertices[Tri.VertIndex[i]].X, Obj.Vertices[Tri.VertIndex[i]].Y, Obj.Vertices[Tri.VertIndex[i]].Z),
                            new Vector2d(Obj.TextureCoordinates[Tri.TexCoordIndex[i]].U * TexXR, Obj.TextureCoordinates[Tri.TexCoordIndex[i]].V * TexYR),
                            new Color4(Obj.Materials[Obj.Materials.IndexOf(Surf.Material)].Kd[0] * 255, Obj.Materials[Obj.Materials.IndexOf(Surf.Material)].Kd[1] * 255, Obj.Materials[Obj.Materials.IndexOf(Surf.Material)].Kd[2] * 255, 0xFF),
                            //new Color4(0x7F, 0x7F, 0x7F, 0xFF),   // dunno <.<
                            Obj.Vertices[Tri.VertIndex[i]].VN);

                        int VtxNo = VertList.FindIndex(FindObj =>
                            FindObj.Position == NewVert.Position &&
                            FindObj.TexCoord == NewVert.TexCoord &&
                            FindObj.Colors == NewVert.Colors &&
                            FindObj.Normals == NewVert.Normals);

                        if (VtxNo == -1)
                        {
                            if (VertList.Count <= 29 + i)
                                VertList.Add(NewVert);
                            else
                                throw new Exception("Vertex buffer overflow; this should never happen!");

                            VtxNo = VertList.Count - 1;
                        }

                        TriPoints[i] = (VtxNo << 1);
                    }

                    Helpers.Append32(ref AsmTris, (uint)(((CommToggle ? 0x06 : 0x00) << 24) | (TriPoints[0] << 16) | (TriPoints[1] << 8) | TriPoints[2]));

                    CommToggle = !CommToggle;

                    if (VertList.Count > 29 || TriIndex == Surf.Triangles.Count - 1)
                    {
                        uint VertOffset = BaseOffset + (uint)VertData.Count;

                        for (int j = 0; j < VertList.Count; j++)
                        {
                            Helpers.Append16(ref VertData, (ushort)(System.Convert.ToInt16(VertList[j].Position.X * Scale)));
                            Helpers.Append16(ref VertData, (ushort)(System.Convert.ToInt16(VertList[j].Position.Y * Scale)));
                            Helpers.Append16(ref VertData, (ushort)(System.Convert.ToInt16(VertList[j].Position.Z * Scale)));
                            Helpers.Append16(ref VertData, 0);
                            Helpers.Append16(ref VertData, (ushort)(System.Convert.ToInt32(VertList[j].TexCoord.X * 1024.0f) & 0xFFFF));
                            Helpers.Append16(ref VertData, (ushort)(System.Convert.ToInt32(VertList[j].TexCoord.Y * 1024.0f) & 0xFFFF));
                            if (IsOutdoors == true)
                            {
                                VertData.Add((byte)System.Convert.ToByte(((int)(VertList[j].Normals.X * 255.0f)) & 0xFF));
                                VertData.Add((byte)System.Convert.ToByte(((int)(VertList[j].Normals.Y * 255.0f)) & 0xFF));
                                VertData.Add((byte)System.Convert.ToByte(((int)(VertList[j].Normals.Z * 255.0f)) & 0xFF));
                            }
                            else
                            {
                                uint Color = (uint)VertList[j].Colors.ToArgb();
                                VertData.Add((byte)((Color >> 16) & 0xFF));
                                VertData.Add((byte)((Color >> 8) & 0xFF));
                                VertData.Add((byte)(Color & 0xFF));
                            }
                            VertData.Add(0xFF);
                        }

                        if ((AsmTris.Count & 4) != 0)
                        {
                            AsmTris[AsmTris.Count - 4] = 0x05;
                            Helpers.Append32(ref AsmTris, 0);
                        }

                        Helpers.Append64(ref DList, ((ulong)(Helpers.ShiftL(GBI.G_VTX, 24, 8) | (uint)(VertList.Count << 12) | (uint)(VertList.Count * 2)) << 32) | (0x03 << 24 | VertOffset));
                        DList.AddRange(AsmTris);

                        /* Determine minimum/maximum coordinate changes... */
                        foreach (NVertex Vtx in VertList)
                        {
                            /* Minimum... */
                            MinCoordinate.X = Math.Min(MinCoordinate.X, Vtx.Position.X * Scale);
                            MinCoordinate.Y = Math.Min(MinCoordinate.Y, Vtx.Position.Y * Scale);
                            MinCoordinate.Z = Math.Min(MinCoordinate.Z, Vtx.Position.Z * Scale);

                            /* Maximum... */
                            MaxCoordinate.X = Math.Max(MaxCoordinate.X, Vtx.Position.X * Scale);
                            MaxCoordinate.Y = Math.Max(MaxCoordinate.Y, Vtx.Position.Y * Scale);
                            MaxCoordinate.Z = Math.Max(MaxCoordinate.Z, Vtx.Position.Z * Scale);
                        }

                        VertList.Clear();
                        AsmTris.Clear();
                        CommToggle = true;
                    }
                }
            }

            /* End of display list */
            Helpers.Append64(ref DList, NoParam(GBI.G_ENDDL));

            /* Finish conversion */
            List<byte> FinalData = new List<byte>();
            FinalData.AddRange(VertData);
            FinalData.AddRange(DList);
            Data = FinalData.ToArray();

            Offset = (uint)(BaseOffset + VertData.Count);
        }

        #endregion
    }
}

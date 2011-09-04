/*
 * ObjFile.cs / Simple(?) Wavefront .obj loader and renderer
 * Class for loading and rendering Wavefront .obj files via OpenTK
 * Written in 2011 by xdaniel
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Xml.Serialization;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

using TexLib;

namespace SharpOcarina
{
    public class ObjFile
    {
        #region Constructors

        public ObjFile() { }

        public ObjFile(string Filename)
            : this(Filename, false) { }

        public ObjFile(string Filename, bool IgnoreMats)
        {
            _IgnoreMaterials = IgnoreMats;
            TexUtil.InitTexturing();
            if (Filename != string.Empty)
                ParseObj(Filename);
        }

        #endregion

        #region Element Classes

        public class Triangle
        {
            public string MaterialName;
            public int[] VertIndex;
            public int[] TexCoordIndex;
            public int[] NormalIndex;

            public Triangle() { }

            public Triangle(int[] _VertIndex, int[] _TexCoordIndex, int[] _NormalIndex)
            {
                MaterialName = string.Empty;
                VertIndex = _VertIndex; TexCoordIndex = _TexCoordIndex; NormalIndex = _NormalIndex;
            }

            public Triangle(string _MaterialName, int[] _VertIndex, int[] _TexCoordIndex, int[] _NormalIndex)
            {
                MaterialName = _MaterialName;
                VertIndex = _VertIndex; TexCoordIndex = _TexCoordIndex; NormalIndex = _NormalIndex;
            }
        }

        public class Vertex
        {
            public double X = 0.0f, Y = 0.0f, Z = 0.0f, W = 0.0f;
            public Vector3d VN = new Vector3d();

            public Vertex() { }

            public Vertex(double _X, double _Y, double _Z)
            {
                X = _X; Y = _Y; Z = _Z; W = 0.0f;
            }

            public Vertex(double _X, double _Y, double _Z, double _W)
            {
                X = _X; Y = _Y; Z = _Z; W = _W;
            }
        }

        public class TextureCoord
        {
            public double U = 0.0f, V = 0.0f, W = 0.0f;

            public TextureCoord() { }

            public TextureCoord(double _U, double _V)
            {
                U = _U; V = _V; W = 0.0f;
            }
            public TextureCoord(double _U, double _V, double _W)
            {
                U = _U; V = _V; W = _W;
            }
        }

        public class Normal
        {
            public double X = 0.0f, Y = 0.0f, Z = 0.0f;

            public Normal() { }

            public Normal(double _X, double _Y, double _Z)
            {
                X = _X; Y = _Y; Z = _Z;
            }
        }

        public class Material
        {
            public string Name;
            public float[] Ka, Kd, Ks;
            public float Tr;
            public int illum;
            public string map_Ka, map_Kd, map_Ks, map_d, map_bump;

            [XmlIgnore]
            public Bitmap TexImage;
            [XmlIgnore]
            public int Width, Height;
            [XmlIgnore]
            public int GLID;

            public Material()
            {
                Ka = new float[] { 0.2f, 0.2f, 0.2f };
                Kd = new float[] { 0.8f, 0.8f, 0.8f };
                Ks = new float[] { 1.0f, 1.0f, 1.0f };
                Tr = 1.0f;
                illum = 0;
            }

            public string DisplayName
            {
                get { return Name; }
            }
        }

        public class Group
        {
            public string Name;

            [XmlIgnore]
            public int GLID;

            [XmlIgnore]
            public uint TintAlpha = 0xFFFFFFFF;
            [XmlIgnore]
            public int TileS = 0, TileT = 0, PolyType = 0;
            [XmlIgnore]
            public bool BackfaceCulling = true;
            
            private List<Triangle> _Tris = new List<Triangle>();

            public List<Triangle> Triangles
            {
                get { return _Tris; }
            }

            public string DisplayName
            {
                get { return Name; }
            }
        }

        #endregion

        #region Element Lists

        private List<Group> _Groups = new List<Group>();
        private List<Vertex> _Verts = new List<Vertex>();
        private List<TextureCoord> _TexCoords = new List<TextureCoord>();
        private List<Normal> _Norms = new List<Normal>();
        private List<Material> _Mats = new List<Material>();

        public List<Group> Groups
        {
            get { return _Groups; }
        }

        public List<Vertex> Vertices
        {
            get { return _Verts; }
        }

        public List<TextureCoord> TextureCoordinates
        {
            get { return _TexCoords; }
        }

        public List<Normal> Normals
        {
            get { return _Norms; }
        }

        public List<Material> Materials
        {
            get { return _Mats; }
        }

        #endregion

        #region Other Variables

        private string _BasePath = string.Empty;

        [XmlIgnore]
        public string BasePath
        {
            get { return _BasePath; }
            set { _BasePath = value; }
        }

        private string Line = string.Empty;

        private char[] TokenSeperator = { ' ', '\t' };
        private char[] TokenValSeperator = { '/' };

        private string MtlFilename = string.Empty;
        private string CurrentMtlName = string.Empty;

        private double X, Y, Z, U, V, W;

        private bool GroupIsOpen;
        private bool MaterialIsOpen;

        private bool _MaterialLighting = false;

        [XmlIgnore]
        public bool MaterialLighting
        {
            get { return _MaterialLighting; }
            set { _MaterialLighting = value; }
        }

        private bool _IgnoreMaterials = false;

        [XmlIgnore]
        public bool IgnoreMaterials
        {
            get { return _IgnoreMaterials; }
            set { _IgnoreMaterials = value; }
        }

        #endregion

        #region Loading & Setup Functions

        public void Load(string Filename)
        {
            ParseObj(Filename);
        }

        #endregion

        #region Model Parser

        private void ParseObj(string Filename)
        {
            StreamReader SR = File.OpenText(Filename);

            Group NewGroup = new Group();
            GroupIsOpen = false;

            while ((Line = SR.ReadLine()) != null)
            {
                Line = Line.TrimStart(TokenSeperator);
                if (Line == string.Empty) continue;

                string[] Tokenized = Line.Split(TokenSeperator, StringSplitOptions.RemoveEmptyEntries);

                switch (Tokenized[0])
                {
                    case "#":
                        /* Comment */
                        break;

                    case "g":
                        /* Group */
                        if (GroupIsOpen == true)
                            AddGroup(NewGroup);

                        GroupIsOpen = true;
                        NewGroup = new Group();
                        NewGroup.Name = Line.Substring(Line.IndexOf(' ') + 1);
                        break;

                    case "mtllib":
                        /* Material lib reference */
                        MtlFilename = Line.Substring(Line.IndexOf(' ') + 1);
                        //ParseMtl(Filename.Substring(0, Filename.LastIndexOf('\\')) + "\\" + MtlFilename);
                        ParseMtl(Path.IsPathRooted(MtlFilename) == true ? Path.GetFileName(MtlFilename) : Path.GetDirectoryName(Path.GetFullPath(Filename)) + Path.DirectorySeparatorChar + Path.GetFileName(MtlFilename));
                        break;

                    case "v":
                        /* Vertex */
                        X = Y = Z = W = 0;
                        double.TryParse(Tokenized[1], NumberStyles.Float, CultureInfo.InvariantCulture, out X);
                        double.TryParse(Tokenized[2], NumberStyles.Float, CultureInfo.InvariantCulture, out Y);
                        double.TryParse(Tokenized[3], NumberStyles.Float, CultureInfo.InvariantCulture, out Z);
                        if (Tokenized.Length == 5)
                            double.TryParse(Tokenized[4], NumberStyles.Float, CultureInfo.InvariantCulture, out W);

                        _Verts.Add(new Vertex(X, Y, Z, W));
                        break;

                    case "vt":
                        /* Texture coordinates */
                        U = V = W = 0;
                        double.TryParse(Tokenized[1], NumberStyles.Float, CultureInfo.InvariantCulture, out U);
                        double.TryParse(Tokenized[2], NumberStyles.Float, CultureInfo.InvariantCulture, out V);
                        if (Tokenized.Length == 4)
                            double.TryParse(Tokenized[3], NumberStyles.Float, CultureInfo.InvariantCulture, out W);

                        _TexCoords.Add(new TextureCoord(U, -V, W));
                        break;

                    case "vn":
                        /* Normals */
                        X = Y = Z = 0;
                        double.TryParse(Tokenized[1], NumberStyles.Float, CultureInfo.InvariantCulture, out X);
                        double.TryParse(Tokenized[2], NumberStyles.Float, CultureInfo.InvariantCulture, out Y);
                        double.TryParse(Tokenized[3], NumberStyles.Float, CultureInfo.InvariantCulture, out Z);

                        _Norms.Add(new Normal(X, Y, Z));
                        break;

                    case "usemtl":
                        /* Material to use */
                        CurrentMtlName = Tokenized[1];
                        break;

                    case "f":
                        /* Face/triangle */
                        int[] VIndex = new int[] { 0, 0, 0 };
                        int[] TIndex = new int[] { 0, 0, 0 };
                        int[] NIndex = new int[] { 0, 0, 0 };

                        for (int i = 0; i < 3; i++)
                        {
                            string[] TokenizedVals = Tokenized[i + 1].Split(TokenValSeperator);

                            int.TryParse(TokenizedVals[0], out VIndex[i]);
                            int.TryParse(TokenizedVals[1], out TIndex[i]);
                            if (TokenizedVals.Length == 3)
                                int.TryParse(TokenizedVals[2], out NIndex[i]);

                            VIndex[i] -= 1;
                            TIndex[i] -= 1;
                            NIndex[i] -= 1;
                        }

                        if (VIndex[0] != -1 && VIndex[1] != -1 && VIndex[2] != -1)
                            NewGroup.Triangles.Add(new Triangle(CurrentMtlName, VIndex, TIndex, NIndex));
                        break;
                }
            }

            AddGroup(NewGroup);

            SR.Close();

            Prepare();
        }

        private void CalculateVertexNormals(ref Group Grp)
        {
            if (_Norms == null || _Norms.Count == 0) return;

            for (int i = 0; i < _Verts.Count; i++)
            {
                foreach (Triangle Tri in Grp.Triangles)
                {
                    if (Tri.VertIndex[0] == i || Tri.VertIndex[1] == i || Tri.VertIndex[2] == i)
                    {
                        _Verts[i].VN.X += (_Norms[Tri.NormalIndex[0]].X + _Norms[Tri.NormalIndex[1]].X + _Norms[Tri.NormalIndex[2]].X);
                        _Verts[i].VN.Y += (_Norms[Tri.NormalIndex[0]].Y + _Norms[Tri.NormalIndex[1]].Y + _Norms[Tri.NormalIndex[2]].Y);
                        _Verts[i].VN.Z += (_Norms[Tri.NormalIndex[0]].Z + _Norms[Tri.NormalIndex[1]].Z + _Norms[Tri.NormalIndex[2]].Z);
                        _Verts[i].VN.Normalize();
                    }
                }
            }
        }

        private void AddGroup(Group GroupToAdd)
        {
            if (GroupIsOpen == true)
            {
                CalculateVertexNormals(ref GroupToAdd);
                _Groups.Add(GroupToAdd);

                GroupIsOpen = false;
            }
        }

        #endregion

        #region Material Parser

        private void ParseMtl(string Filename)
        {
            if (_IgnoreMaterials == true) return;

            //_BasePath = Filename.Substring(0, Filename.LastIndexOf('\\')) + "\\";
            _BasePath = Path.GetDirectoryName(Filename) + Path.DirectorySeparatorChar;

            StreamReader SR = File.OpenText(Filename);

            Material NewMaterial = null;
            MaterialIsOpen = false;

            while ((Line = SR.ReadLine()) != null)
            {
                Line = Line.TrimStart(TokenSeperator);
                if (Line == string.Empty) continue;

                string[] Tokenized = Line.Split(TokenSeperator, StringSplitOptions.RemoveEmptyEntries);

                switch (Tokenized[0])
                {
                    case "#":
                        /* Comment */
                        break;

                    case "newmtl":
                        /* New material */
                        AddMaterial(NewMaterial);

                        MaterialIsOpen = true;
                        NewMaterial = new Material();
                        NewMaterial.Name = Tokenized[1];
                        break;

                    case "Ka":
                        NewMaterial.Ka = new float[3];

                        float.TryParse(Tokenized[1], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.Ka[0]);
                        float.TryParse(Tokenized[2], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.Ka[1]);
                        float.TryParse(Tokenized[3], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.Ka[2]);
                        break;

                    case "Kd":
                        NewMaterial.Kd = new float[3];

                        float.TryParse(Tokenized[1], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.Kd[0]);
                        float.TryParse(Tokenized[2], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.Kd[1]);
                        float.TryParse(Tokenized[3], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.Kd[2]);
                        break;

                    case "Ks":
                        NewMaterial.Ks = new float[3];

                        float.TryParse(Tokenized[1], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.Ks[0]);
                        float.TryParse(Tokenized[2], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.Ks[1]);
                        float.TryParse(Tokenized[3], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.Ks[2]);
                        break;

                    case "Tr":
                    case "d":
                        float.TryParse(Tokenized[1], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.Tr);
                        break;

                    case "illum":
                        int.TryParse(Tokenized[1], NumberStyles.Float, CultureInfo.InvariantCulture, out NewMaterial.illum);
                        break;

                    case "map_Ka":
                    case "mapKa":
                        NewMaterial.map_Ka = Line.Substring(Line.IndexOf(' ') + 1);
                        break;

                    case "map_Kd":
                    case "mapKd":
                        NewMaterial.map_Kd = Line.Substring(Line.IndexOf(' ') + 1);
                        break;

                    case "map_Ks":
                    case "mapKs":
                        NewMaterial.map_Ks = Line.Substring(Line.IndexOf(' ') + 1);
                        break;

                    case "map_d":
                        NewMaterial.map_d = Line.Substring(Line.IndexOf(' ') + 1);
                        break;

                    case "bump":
                    case "map_bump":
                        NewMaterial.map_bump = Line.Substring(Line.IndexOf(' ') + 1);
                        break;
                }
            }

            AddMaterial(NewMaterial);
        }

        private void AddMaterial(Material MatToAdd)
        {
            if (MaterialIsOpen == true)
            {
                /* If map_Ka is empty, set it to map_Kd */
                if (MatToAdd.map_Ka == null && MatToAdd.map_Kd != null)
                    MatToAdd.map_Ka = MatToAdd.map_Kd;

                /* Else if map_Kd is empty, set it to map_Ka */
                else if (MatToAdd.map_Kd == null && MatToAdd.map_Ka != null)
                    MatToAdd.map_Kd = MatToAdd.map_Ka;

                /* Only add the material if both, map_Ka and map_Kd, aren't empty */
                if (MatToAdd.map_Ka != null && MatToAdd.map_Kd != null)
                    Materials.Add(MatToAdd);

                MaterialIsOpen = false;
            }
        }

        #endregion

        #region Model Rendering

        public Material GetMaterial(string Name)
        {
            if (_IgnoreMaterials == true) return null;

            foreach (Material Mat in Materials)
            {
                if (string.Compare(Mat.Name, Name) == 0)
                    return Mat;
            }

            return null;
        }

        public void Prepare()
        {
            Prepare(true);
        }

        public void Prepare(bool All)
        {
            if (All == true) LoadTextures();
            PrepareDisplayLists();
        }

        private void LoadTextures()
        {
            string LoadPath = string.Empty;

            for (int i = 0; i < _Mats.Count; i++)
            {
                if (_Mats[i].map_Ka != null)
                {
                    try
                    {
                        if (GL.IsTexture(_Mats[i].GLID) == true) GL.DeleteTexture(_Mats[i].GLID);

                        if (_Mats[i].TexImage != null) _Mats[i].TexImage.Dispose();

                        LoadPath = Path.IsPathRooted(_Mats[i].map_Ka) == true ? _Mats[i].map_Ka : _BasePath + _Mats[i].map_Ka;
                        _Mats[i].TexImage = new Bitmap(Bitmap.FromFile(LoadPath));
                        _Mats[i].GLID = TexUtil.CreateTextureFromBitmap(_Mats[i].TexImage);
                        _Mats[i].Width = _Mats[i].TexImage.Width;
                        _Mats[i].Height = _Mats[i].TexImage.Height;
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("Texture image " + LoadPath + " not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }
                }
            }
        }

        private void PrepareDisplayLists()
        {
            for (int i = 0; i < _Groups.Count; i++)
            {
                if (GL.IsList(_Groups[i].GLID) == true) GL.DeleteLists(_Groups[i].GLID, 1);

                _Groups[i].GLID = GL.GenLists(1);
                GL.NewList(_Groups[i].GLID, ListMode.Compile);

                GL.ActiveTexture(TextureUnit.Texture0);
                
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);

                if (_Groups[i].BackfaceCulling == true)
                    GL.Enable(EnableCap.CullFace);
                else
                    GL.Disable(EnableCap.CullFace);

                foreach (Triangle Tri in _Groups[i].Triangles)
                {
                    Material Mat = GetMaterial(Tri.MaterialName);
                    if (Mat != null)
                    {
                        GL.BindTexture(TextureTarget.Texture2D, Mat.GLID);

                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);

                        if (_Groups[i].TileS != 0 || _Groups[i].TileT != 0)
                        {
                            if (_Groups[i].TileS == 1)
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.MirroredRepeatArb);
                            else if (_Groups[i].TileS == 2)
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);

                            if (_Groups[i].TileT == 1)
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.MirroredRepeatArb);
                            else if (_Groups[i].TileT == 2)
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
                        }
                    }

                    DrawTriangle(Tri, Mat);
                }

                GL.EndList();
            }
        }

        private void DrawTriangle(Triangle Tri, Material Mat)
        {
            if (Mat != null)
            {
                /* Normal, textured model rendering */
                if (_MaterialLighting == true)
                {
                    GL.Enable(EnableCap.ColorMaterial);
                    GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, Mat.Ka);
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, Mat.Kd);
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, Mat.Ks);
                }

                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);

                DrawNormalTriangle(Tri, Mat);
            }
            else
            {
                DrawHighlightedTriangle(Tri, new Color4(1.0f, 0.0f, 0.0f, 0.25f), true);
            }
        }

        private void DrawHighlightedTriangle(Triangle Tri, Color4 Color, bool Outlined)
        {
            /* Setup */
            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(-1.0f, -1.0f);

            /* Polygons */
            GL.Color4(Color);
            DrawNormalTriangle(Tri, null);

            /* Outlines */
            if (Outlined == true)
            {
                GL.Color4(0.0f, 0.0f, 0.0f, 1.0f);
                GL.LineWidth(3.0f);
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
                DrawNormalTriangle(Tri, null);
            }

            /* Reset */
            GL.LineWidth(1.0f);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.PolygonOffset(0.0f, 0.0f);
        }

        private void DrawNormalTriangle(Triangle Tri, Material Mat)
        {
            GL.Begin(BeginMode.Triangles);

            for (int j = 0; j < 3; j++)
            {
                if (Mat != null)
                {
                    if (TextureCoordinates.Count != 0 && Tri.TexCoordIndex[j] != -1)
                        GL.TexCoord3(
                            TextureCoordinates[Tri.TexCoordIndex[j]].U,
                            TextureCoordinates[Tri.TexCoordIndex[j]].V,
                            TextureCoordinates[Tri.TexCoordIndex[j]].W);
                }

                if (Normals.Count != 0 && Tri.NormalIndex[j] != -1)
                    GL.Normal3(Normals[Tri.NormalIndex[j]].X, Normals[Tri.NormalIndex[j]].Y, Normals[Tri.NormalIndex[j]].Z);

                /* Should never trip!! */
                if (Tri.VertIndex[j] != -1)
                    GL.Vertex3(Vertices[Tri.VertIndex[j]].X, Vertices[Tri.VertIndex[j]].Y, Vertices[Tri.VertIndex[j]].Z);
                else
                    throw new Exception("Invalid vertex index detected; should've been filtered out beforehand!");
            }

            GL.End();
        }

        public void Render(Group Grp)
        {
            GL.CallList(Grp.GLID);
        }

        public void Render(int Grp)
        {
            GL.CallList(_Groups[Grp].GLID);
        }

        public void Render()
        {
            foreach (Group G in _Groups)
                GL.CallList(G.GLID);
        }

        #endregion
    }
}

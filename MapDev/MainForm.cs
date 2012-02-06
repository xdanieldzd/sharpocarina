/*
 * GUI code "disclaimer":
 * This file's code sucks balls and I have no intention to clean it, heavily optimize it, etc.; if anything, it gets minor fixes and additions as needed.
 * The other elements of the project are much more useful, anyway, so glance over this, leave it be and think "well, it works somehow!"
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace SharpOcarina
{
    public partial class MainForm : Form
    {
        #region Variables/Structs/Constructor

        bool NowLoading = false;

        ZScene CurrentScene = null;
        bool SceneLoaded = false;

        bool IsReady = false;

        bool[] KeysDown = new bool[256];

        bool ShowCollisionModel = false;
        bool ShowRoomModels = true;
        bool ApplyEnvLighting = false;
        bool ConsecutiveRoomInject = true;
        bool ForceRGBATextures = false;
        bool SimulateN64Gfx = false;

        int ActorCubeGLID = 0, ActorPyramidGLID = 0, AxisMarkerGLID = 0;

        public struct MouseStruct
        {
            public Vector2d Center, Move;
            public bool LDown, RDown, MDown;
        }

        MouseStruct Mouse = new MouseStruct();

        public MainForm()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            textBox2.ContextMenu = new ContextMenu();

            checkBox5.LostFocus += new EventHandler(checkBox5_LostFocus);
            glControl1.LostFocus += new EventHandler(glControl1_LostFocus);
        }

        #endregion

        #region Main Functions, Rendering

        public void ProgramMainLoop()
        {
            Camera.KeyUpdate(KeysDown);

            glControl1.Invalidate();
        }

        public void SetViewport(int VPWidth, int VPHeight)
        {
            GL.Viewport(0, 0, VPWidth, VPHeight);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 PerspMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60.0f), (float)VPWidth / (float)VPHeight, 0.001f, 10000.0f);
            GL.MultMatrix(ref PerspMatrix);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            AxisMarkerGLID = GL.GenLists(1);
            ActorCubeGLID = GL.GenLists(1);
            ActorPyramidGLID = GL.GenLists(1);

            // Axis marker
            GL.NewList(AxisMarkerGLID, ListMode.Compile);

            GL.LineWidth(2);

            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.Blue);
            GL.Vertex3(20.0f, 0.1f, 0.1f);
            GL.Vertex3(-20.0f, 0.1f, 0.1f);
            GL.End();
            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(0.1f, 20.0f, 0.1f);
            GL.Vertex3(0.1f, -20.0f, 0.1f);
            GL.End();

            GL.LineWidth(5);
            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.Green);
            GL.Vertex3(0.1f, 0.1f, 20.0f);
            GL.Vertex3(0.1f, 0.1f, -20.0f);
            GL.Vertex3(0.1f, 0.1f, 20.0f);
            GL.End();

            GL.LineWidth(1);

            GL.EndList();

            // Actor cube
            GL.NewList(ActorCubeGLID, ListMode.Compile);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Begin(BeginMode.Quads);
            // Back Face
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, -1.0f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-1.0f, 1.0f, -1.0f);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, -1.0f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, -1.0f);
            // Top Face
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1.0f, 1.0f, -1.0f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1.0f, 1.0f, 1.0f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 1.0f, 1.0f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, -1.0f);
            // Bottom Face
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-1.0f, -1.0f, -1.0f);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(1.0f, -1.0f, -1.0f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, 1.0f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, 1.0f);
            // Right face
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, -1.0f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, -1.0f);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 1.0f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, 1.0f);
            // Left Face
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, -1.0f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, 1.0f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(-1.0f, 1.0f, 1.0f);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1.0f, 1.0f, -1.0f);
            // Front Face
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, 1.0f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, 1.0f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 1.0f);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-1.0f, 1.0f, 1.0f);
            GL.End();

            GL.Disable(EnableCap.CullFace);

            GL.EndList();

            // Actor pyramid
            GL.NewList(ActorPyramidGLID, ListMode.Compile);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Begin(BeginMode.Triangles);
            // Front face
            GL.Vertex3(0.0f, 2.0f, 0.0f);
            GL.Vertex3(-2.0f, -2.0f, 2.0f);
            GL.Vertex3(2.0f, -2.0f, 2.0f);
            // Right face
            GL.Vertex3(0.0f, 2.0f, 0.0f);
            GL.Vertex3(2.0f, -2.0f, 2.0f);
            GL.Vertex3(2.0f, -2.0f, -2.0f);
            // Back face
            GL.Vertex3(0.0f, 2.0f, 0.0f);
            GL.Vertex3(2.0f, -2.0f, -2.0f);
            GL.Vertex3(-2.0f, -2.0f, -2.0f);
            // Left face
            GL.Vertex3(0.0f, 2.0f, 0.0f);
            GL.Vertex3(-2.0f, -2.0f, -2.0f);
            GL.Vertex3(-2.0f, -2.0f, 2.0f);
            GL.End();

            GL.Begin(BeginMode.Quads);
            // Bottom face
            GL.Vertex3(-2.0f, -2.0f, -2.0f);
            GL.Vertex3(2.0f, -2.0f, -2.0f);
            GL.Vertex3(2.0f, -2.0f, 2.0f);
            GL.Vertex3(-2.0f, -2.0f, 2.0f);
            GL.End();

            GL.Disable(EnableCap.CullFace);

            GL.EndList();

            // continue...
            IsReady = true;

            GL.ClearColor(Color.Black);
            SetViewport(glControl1.Width, glControl1.Height);

            Camera.Initialize();
        }

        private void DrawActorModel(ZActor Actor, Color FillColor, int DrawModelGLID, bool DrawAxis)
        {
            GL.PushMatrix();

            GL.Translate(Actor.XPos, Actor.YPos, Actor.ZPos);
            GL.Rotate(Actor.XRot / 182.04444444444444444444444444444f, 1.0f, 0.0f, 0.0f);
            GL.Rotate(Actor.YRot / 182.04444444444444444444444444444f, 0.0f, 1.0f, 0.0f);
            GL.Rotate(Actor.ZRot / 182.04444444444444444444444444444f, 0.0f, 0.0f, 1.0f);

            GL.Scale(10.0f, 10.0f, 10.0f);

            GL.Color4(FillColor);

            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable((EnableCap)All.FragmentProgram);
            GL.Disable(EnableCap.Lighting);

            for (int j = 0; j < 2; j++)
            {
                // If we're doing the outline, set some stuff prior to rendering
                if (j == 1)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    GL.Color4(Color.Black.R, Color.Black.G, Color.Black.B, 1.0f);

                    GL.Enable(EnableCap.PolygonOffsetLine);
                    GL.PolygonOffset(-3.0f, -3.0f);
                }

                GL.CallList(DrawModelGLID);

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Disable(EnableCap.PolygonOffsetLine);
            }

            if (DrawAxis == true)
                GL.CallList(AxisMarkerGLID);

            GL.PopMatrix();
            GL.PopAttrib();
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (IsReady == false)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.5f, 0.5f, 0.5f, 0.5f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Position, new float[] { 0.0f, 25.0f, 0.0f, 0.0f });

            Camera.Position();

            GL.Enable(EnableCap.DepthTest);

            GL.Scale(0.005f, 0.005f, 0.005f);

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Normalize);
            GL.Disable(EnableCap.Lighting);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (CurrentScene != null && NowLoading == false)
            {
                /* Rendering base settings... */
                if (CurrentScene.IsOutdoors == true)
                    GL.ClearColor(Color.FromArgb(255, 51, 128, 179));
                else
                    GL.ClearColor(Color.Black);

                if (CurrentScene.Environments.Count != 0)
                {
                    GL.Light(LightName.Light0, LightParameter.Diffuse, Color.FromArgb(
                        CurrentScene.Environments[(int)numericUpDown11.Value - 1].C3C.A,
                        CurrentScene.Environments[(int)numericUpDown11.Value - 1].C3C.R,
                        CurrentScene.Environments[(int)numericUpDown11.Value - 1].C3C.G,
                        CurrentScene.Environments[(int)numericUpDown11.Value - 1].C3C.B));
                }

                foreach (ZScene.ZRoom Room in CurrentScene.Rooms)
                {
                    /* Render room actors... */
                    if (Room == ((ZScene.ZRoom)listBox1.SelectedItem))
                    {
                        GL.Disable(EnableCap.Texture2D);
                        for (int i = 0; i < Room.ZActors.Count; i++)
                        {
                            DrawActorModel(Room.ZActors[i],
                                (i == actorEditControl1.ActorNumber - 1 ? Color.FromArgb(0, 255, 0) : Color.FromArgb(192, 255, 192)),
                                ActorCubeGLID,
                                Convert.ToBoolean(i == actorEditControl1.ActorNumber - 1));
                        }
                    }

                    if (ShowRoomModels == true)
                    {
                        GL.PushAttrib(AttribMask.AllAttribBits);

                        if (SimulateN64Gfx == false)
                        {
                            /* Prepare... */
                            GL.PushMatrix();
                            GL.Scale(CurrentScene.Scale, CurrentScene.Scale, CurrentScene.Scale);

                            GL.Enable(EnableCap.CullFace);
                            GL.CullFace(CullFaceMode.Back);

                            /* Faked environmental lighting... */
                            if (ApplyEnvLighting == true)
                            {
                                GL.Enable(EnableCap.ColorMaterial);
                                GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);
                                GL.Enable(EnableCap.Normalize);
                                GL.Enable(EnableCap.Lighting);
                                GL.Enable(EnableCap.Light0);
                            }

                            /* Render groups... */
                            for (int i = 0; i < Room.ObjModel.Groups.Count; i++)
                            {
                                GL.Enable(EnableCap.Texture2D);
                                GL.Color4(Color.FromArgb((int)Room.ObjModel.Groups[i].TintAlpha));
                                Room.ObjModel.Render(i);
                            }

                            GL.PopMatrix();
                        }
                        else if (Room.N64DLists != null)
                        {
                            GL.Enable(EnableCap.Light0);
                            foreach (SayakaGL.UcodeSimulator.DisplayListStruct DL in Room.N64DLists)
                            {
                                GL.CallList(DL.GLID);
                            }
                        }
                        GL.PopAttrib();
                    }
                }

                /* Render spawn points... */
                GL.Disable(EnableCap.Texture2D);
                for (int i = 0; i < CurrentScene.SpawnPoints.Count; i++)
                    DrawActorModel(CurrentScene.SpawnPoints[i],
                        (i == actorEditControl3.ActorNumber - 1 ? Color.FromArgb(0, 0, 255) : Color.FromArgb(192, 192, 255)),
                        ActorPyramidGLID,
                        Convert.ToBoolean(i == actorEditControl3.ActorNumber - 1));

                /* Render transition actors... */
                for (int i = 0; i < CurrentScene.Transitions.Count; i++)
                    DrawActorModel(CurrentScene.Transitions[i],
                        (i == actorEditControl2.ActorNumber - 1 ? Color.FromArgb(255, 0, 0) : Color.FromArgb(255, 192, 192)),
                        ActorPyramidGLID,
                        Convert.ToBoolean(i == actorEditControl2.ActorNumber - 1));

                /* Render waterboxes... */
                GL.Disable(EnableCap.CullFace);
                foreach (ZWaterbox WBox in CurrentScene.Waterboxes)
                {
                    GL.PushMatrix();

                    GL.Translate(WBox.XPos, WBox.YPos, WBox.ZPos);

                    for (int i = 0; i < 2; i++)
                    {
                        if (i == 0)
                        {
                            GL.Color4(0.0f, 0.0f, 1.0f, 0.5f);
                        }
                        else
                        {
                            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
                            GL.Color4(Color.Black);
                        }

                        GL.Begin(BeginMode.Quads);
                        GL.Vertex3(0.0f, 1.0f, 0.0f);
                        GL.Vertex3(0.0f, 1.0f, WBox.ZSize);
                        GL.Vertex3(WBox.XSize, 1.0f, WBox.ZSize);
                        GL.Vertex3(WBox.XSize, 1.0f, 0.0f);
                        GL.End();
                    }

                    GL.PopMatrix();

                    GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                }

                /* Render collision model... */
                if (ShowCollisionModel == true && CurrentScene.ColModel != null)
                {
                    GL.PushMatrix();
                    GL.Scale(CurrentScene.Scale, CurrentScene.Scale, CurrentScene.Scale);
                    CurrentScene.ColModel.Render();
                    GL.PopMatrix();
                }

                /* Render group highlight... */
                if (((ObjFile.Group)listBox2.SelectedItem) != null)
                {
                    GL.PushMatrix();
                    GL.PushAttrib(AttribMask.AllAttribBits);
                    GL.Scale(CurrentScene.Scale, CurrentScene.Scale, CurrentScene.Scale);

                    GL.Disable((EnableCap)All.FragmentProgram);
                    GL.Disable(EnableCap.Texture2D);
                    GL.Enable(EnableCap.Blend);
                    GL.Enable(EnableCap.PolygonOffsetFill);
                    GL.PolygonOffset(-5.0f, -5.0f);

                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    GL.Color4(1.0f, 0.5f, 0.0f, 0.5f);
                    ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Render(((ObjFile.Group)listBox2.SelectedItem));

                    GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                    GL.PolygonOffset(0.0f, 0.0f);

                    GL.PopMatrix();
                    GL.PopAttrib();
                }
            }

            glControl1.SwapBuffers();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            if (IsReady == false)
                return;

            SetViewport(glControl1.Width, glControl1.Height);
            glControl1.Invalidate();
        }

        void glControl1_KeyDown(object sender, KeyEventArgs e)
        {
            KeysDown[e.KeyValue] = true;
        }

        void glControl1_KeyUp(object sender, KeyEventArgs e)
        {
            KeysDown[e.KeyValue] = false;
        }

        void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Mouse.LDown = true;
            else if (e.Button == MouseButtons.Right)
                Mouse.RDown = true;
            else if (e.Button == MouseButtons.Middle)
                Mouse.MDown = true;

            Mouse.Center = new Vector2d(e.X, e.Y);

            if (Mouse.LDown == true)
            {
                if (Mouse.Center != Mouse.Move)
                    Camera.MouseMove(Mouse.Move);
                else
                    Camera.MouseCenter(Mouse.Move);
            }
        }

        void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            Mouse.Move = new Vector2d(e.X, e.Y);

            if (Mouse.LDown == true)
            {
                if (Mouse.Center != Mouse.Move)
                    Camera.MouseMove(Mouse.Move);
                else
                    Camera.MouseCenter(Mouse.Move);
            }
        }

        void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Mouse.LDown = false;
            else if (e.Button == MouseButtons.Right)
                Mouse.RDown = false;
            else if (e.Button == MouseButtons.Middle)
                Mouse.MDown = false;
        }

        #endregion

        #region Form & Control Updates, Helpers

        private void ValidateGroupSettings(ref ZScene.ZRoom.ZGroupSettings GSet, int GroupCount)
        {
            if (GSet.TintAlpha.Length != GroupCount)
            {
                GSet.TintAlpha = new uint[GroupCount];
                GSet.TintAlpha.Fill(new uint[] { 0xFFFFFFFF });
            }

            if (GSet.TileS.Length != GroupCount)
            {
                GSet.TileS = new int[GroupCount];
                GSet.TileS.Fill(new int[] { GBI.G_TX_WRAP });
            }

            if (GSet.TileT.Length != GroupCount)
            {
                GSet.TileT = new int[GroupCount];
                GSet.TileT.Fill(new int[] { GBI.G_TX_WRAP });
            }

            if (GSet.PolyType.Length != GroupCount)
            {
                GSet.PolyType = new int[GroupCount];
                GSet.PolyType.Fill(new int[] { 0x0000000000000000 });
            }

            if (GSet.BackfaceCulling.Length != GroupCount)
            {
                GSet.BackfaceCulling = new bool[GroupCount];
                GSet.BackfaceCulling.Fill(new bool[] { true });
            }

            if (GSet.MultiTexMaterial.Length != GroupCount)
            {
                GSet.MultiTexMaterial = new int[GroupCount];
                GSet.MultiTexMaterial.Fill(new int[] { -1 });
            }

            if (GSet.ShiftS.Length != GroupCount)
            {
                GSet.ShiftS = new int[GroupCount];
                GSet.ShiftS.Fill(new int[] { GBI.G_TX_NOLOD });
            }

            if (GSet.ShiftT.Length != GroupCount)
            {
                GSet.ShiftT = new int[GroupCount];
                GSet.ShiftT.Fill(new int[] { GBI.G_TX_NOLOD });
            }
        }

        private bool ColorPicker(ref PictureBox PB)
        {
            ColorDialog CD = new ColorDialog();
            CD.FullOpen = true;
            CD.Color = PB.BackColor;
            if (CD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PB.BackColor = CD.Color;
                return true;
            }
            return false;
        }

        private Vector3d GetCenterPoint(List<ObjFile.Vertex> Vertices)
        {
            Vector3d MinCoordinate = new Vector3d(0, 0, 0);
            Vector3d MaxCoordinate = new Vector3d(0, 0, 0);

            foreach (ObjFile.Vertex Vtx in Vertices)
            {
                /* Minimum... */
                MinCoordinate.X = Math.Min(MinCoordinate.X, Vtx.X * CurrentScene.Scale);
                MinCoordinate.Y = Math.Min(MinCoordinate.Y, Vtx.Y * CurrentScene.Scale);
                MinCoordinate.Z = Math.Min(MinCoordinate.Z, Vtx.Z * CurrentScene.Scale);

                /* Maximum... */
                MaxCoordinate.X = Math.Max(MaxCoordinate.X, Vtx.X * CurrentScene.Scale);
                MaxCoordinate.Y = Math.Max(MaxCoordinate.Y, Vtx.Y * CurrentScene.Scale);
                MaxCoordinate.Z = Math.Max(MaxCoordinate.Z, Vtx.Z * CurrentScene.Scale);
            }

            return Vector3d.Lerp(MinCoordinate, MaxCoordinate, 0.5f);
        }

        private void UpdateForm()
        {
            if (CurrentScene != null)
            {
                if (NowLoading == false)
                {
                    this.SuspendLayout();

                    this.Text = Program.ApplicationTitle + " - " + CurrentScene.Name;

                    saveBinaryToolStripMenuItem.Enabled = true;
                    saveSceneToolStripMenuItem.Enabled = true;
                    injectToROMToolStripMenuItem.Enabled = true;

                    showCollisionModelToolStripMenuItem.Checked = ShowCollisionModel;
                    showRoomModelsToolStripMenuItem.Checked = ShowRoomModels;
                    applyEnvironmentLightingToolStripMenuItem.Checked = ApplyEnvLighting;
                    consecutiveRoomInjectionToolStripMenuItem.Checked = ConsecutiveRoomInject;
                    forceRGBATexturesToolStripMenuItem.Checked = ForceRGBATextures;
                    checkBox5.Checked = SimulateN64Gfx;

                    optionsToolStripMenuItem.Enabled = true;
                    tabControl1.Enabled = true;

                    textBox1.Text = CurrentScene.Name;
                    numericUpDown1.Value = (decimal)CurrentScene.Scale;
                    textBox2.Text = CurrentScene.Music.ToString();
                    textBox3.Text = System.IO.Path.GetFileName(CurrentScene.CollisionFilename);
                    numericTextBox3.Text = CurrentScene.InjectOffset.ToString("X8");
                    textBox4.Text = CurrentScene.SceneNumber.ToString();
                    checkBox1.Checked = CurrentScene.IsOutdoors;

                    actorEditControl2.Enabled = true;
                    actorEditControl3.Enabled = true;

                    groupBox6.Enabled = true;
                    UpdateExitEdit();

                    if (listBox1.SelectedItem != null)
                    {
                        groupBox2.Enabled = true;
                        actorEditControl1.Enabled = true;

                        if (ConsecutiveRoomInject == true && listBox1.SelectedIndex > 0)
                        {
                            numericTextBox4.Enabled = false;
                            numericTextBox4.Text = string.Empty;
                        }
                        else
                        {
                            numericTextBox4.Enabled = true;
                            numericTextBox4.Text = ((ZScene.ZRoom)listBox1.SelectedItem).InjectOffset.ToString("X8");
                        }

                        UpdateObjectEdit();

                        //if (listBox2.SelectedIndex != -1)
                        //((CurrencyManager)listBox2.BindingContext[((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups]).Refresh();
                    }
                    else
                    {
                        groupBox2.Enabled = false;
                        actorEditControl1.Enabled = false;
                    }

                    if (listBox1.Items.Count == 0 || listBox1.SelectedIndex == -1)
                        button3.Enabled = false;
                    else
                        button3.Enabled = true;

                    if (listBox3.Items.Count == 0 || listBox3.SelectedIndex == -1)
                        button7.Enabled = false;
                    else
                        button7.Enabled = true;

                    if (listBox4.Items.Count == 0 || listBox4.SelectedIndex == -1)
                        button13.Enabled = false;
                    else
                        button13.Enabled = true;

                    this.ResumeLayout();
                }

                UpdateWaterboxEdit();
                UpdateEnvironmentEdit();
                UpdateGroupSelect();
                UpdatePolyTypeEdit();
            }
        }

        private void UpdateGroupSelect()
        {
            if (listBox2.SelectedItem != null)
            {
                /* ---- Multitex stuff START ---- */

                /* Multitex material selector */
                comboBox3.BeginUpdate();
                comboBox3.Items.Clear();
                comboBox3.DisplayMember = "DisplayName";
                ObjFile.Material Dummy = new ObjFile.Material();
                Dummy.Name = "(none)";
                comboBox3.Items.Add(Dummy);
                foreach (ObjFile.Material Mat in ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Materials)
                    comboBox3.Items.Add(Mat);

                comboBox3.SelectedIndexChanged -= comboBox3_SelectedIndexChanged;
                if (((ObjFile.Group)listBox2.SelectedItem).MultiTexMaterial != -1)
                    comboBox3.SelectedIndex = ((ObjFile.Group)listBox2.SelectedItem).MultiTexMaterial + 1;
                else
                    comboBox3.SelectedIndex = 0;
                comboBox3.SelectedIndexChanged += comboBox3_SelectedIndexChanged;
                comboBox3.EndUpdate();

                /* Multitex controls */
                numericUpDown5.Value = ((ObjFile.Group)listBox2.SelectedItem).ShiftS;
                numericUpDown6.Value = ((ObjFile.Group)listBox2.SelectedItem).ShiftT;

                /* ---- Multitex stuff END ---- */

                numericUpDown4.Minimum = 1;
                numericUpDown4.Maximum = CurrentScene.PolyTypes.Count;

                groupBox1.Enabled = true;
                pictureBox7.BackColor = Color.FromArgb((int)(0xFF000000 | (((ObjFile.Group)listBox2.SelectedItem).TintAlpha & 0xFFFFFF)));
                numericUpDown2.Value = (((ObjFile.Group)listBox2.SelectedItem).TintAlpha >> 24);
                comboBox1.SelectedIndex = ((ObjFile.Group)listBox2.SelectedItem).TileS;
                comboBox2.SelectedIndex = ((ObjFile.Group)listBox2.SelectedItem).TileT;
                numericUpDown4.Value = ((ObjFile.Group)listBox2.SelectedItem).PolyType + 1;
                checkBox3.Checked = ((ObjFile.Group)listBox2.SelectedItem).BackfaceCulling;

                ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Prepare(false);
            }
            else
            {
                numericUpDown4.Minimum = 0;
                numericUpDown4.Maximum = 0;

                groupBox1.Enabled = false;
                numericUpDown2.Value = 0;
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
                pictureBox7.BackColor = Control.DefaultBackColor;
                numericUpDown4.Value = 0;
                checkBox3.Checked = false;
            }
        }

        private void SelectRoom()
        {
            SelectRoom(listBox1.Items.Count - 1);
        }

        private void SelectRoom(int Index)
        {
            listBox2.BeginUpdate();
            if (Index >= 0)
                listBox2.DataSource = ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups;
            else
                listBox2.DataSource = null;

            if (listBox1.SelectedItem != null)
            {
                actorEditControl1.SetActors(ref ((ZScene.ZRoom)listBox1.SelectedItem).ZActors);
                actorEditControl1.CenterPoint = GetCenterPoint(((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Vertices);
            }
            else
                actorEditControl1.ClearActors();

            listBox2.DisplayMember = "DisplayName";
            listBox2.EndUpdate();

            if (NowLoading == false) UpdateForm();
        }

        private NumericTextBox ListEditBox;
        private int itemSelected;

        private void CreateEditBox(object sender)
        {
            ListBox LB = (ListBox)sender;

            itemSelected = LB.SelectedIndex;
            if (itemSelected == -1) return;

            Rectangle r = LB.GetItemRectangle(itemSelected);
            string itemText = ((ZScene.ZUShort)LB.Items[itemSelected]).ValueHex;

            ListEditBox = new NumericTextBox();
            ListEditBox.AllowHex = true;
            ListEditBox.MaxLength = 4;
            ListEditBox.CharacterCasing = CharacterCasing.Upper;

            ListEditBox.BackColor = Color.Beige;
            ListEditBox.Font = listBox3.Font;
            ListEditBox.BorderStyle = BorderStyle.FixedSingle;

            ListEditBox.Location = new Point(r.X, r.Y);
            ListEditBox.Size = new Size(r.Width, r.Height);
            ListEditBox.Show();
            ListEditBox.Text = itemText;
            ListEditBox.Focus();
            ListEditBox.SelectAll();
        }

        private void checkBox5_Click(object sender, EventArgs e)
        {
            SimulateN64Gfx = checkBox5.Checked;

            if (SimulateN64Gfx == true)
                CurrentScene.ConvertPreview(ConsecutiveRoomInject, ForceRGBATextures);
        }

        private void checkBox5_LostFocus(object sender, EventArgs e)
        {
            if (glControl1.Focused == false)
                SimulateN64Gfx = checkBox5.Checked = false;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)
                checkBox5.BackColor = Color.LightGreen;
            else
                checkBox5.BackColor = SystemColors.Control;
        }

        private void glControl1_LostFocus(object sender, EventArgs e)
        {
            if (checkBox5.Focused == false)
                SimulateN64Gfx = checkBox5.Checked = false;
        }

        #endregion

        #region Menu Functions

        private void newSceneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NowLoading = true;

            /* Reset camera */
            Camera.Initialize();

            /* Clear form */
            listBox1.DataSource = null;
            listBox2.DataSource = null;
            listBox3.DataSource = null;

            /* Generate new scene */
            CurrentScene = new ZScene();
            CurrentScene.Name = "unnamed scene";
            CurrentScene.Scale = 1.0f;
            CurrentScene.Music = 0x02;

            /* Add Link's default spawn point */
            CurrentScene.SpawnPoints.Add(new ZActor(0x0000, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0x0FFF));

            /* Add default environment settings */
            SetDefaultEnvironments();

            /* Add default collision polygon type */
            CurrentScene.PolyTypes.Add(new ZColPolyType(0x0000000000000000));

            /* Setup interface */
            listBox1.DataSource = CurrentScene.Rooms;
            listBox1.DisplayMember = "ModelShortFilename";

            ActorEditControl.UpdateFormDelegate TempDelegate = new ActorEditControl.UpdateFormDelegate(UpdateForm);
            actorEditControl1.SetUpdateDelegate(TempDelegate);
            actorEditControl1.SetLabels("Actor", "Actors");

            actorEditControl2.SetUpdateDelegate(TempDelegate);
            actorEditControl2.SetLabels("Transition", "Transitions");
            actorEditControl2.IsTransitionActor = true;
            actorEditControl2.SetActors(ref CurrentScene.Transitions);

            actorEditControl3.SetUpdateDelegate(TempDelegate);
            actorEditControl3.SetLabels("Spawn", "Spawn Points");
            actorEditControl3.SetActors(ref CurrentScene.SpawnPoints);

            SetPolyTypesInCollision();

            NowLoading = false;

            UpdateForm();

            SceneLoaded = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void showCollisionModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowCollisionModel = showCollisionModelToolStripMenuItem.Checked;
        }

        private void showRoomModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowRoomModels = showRoomModelsToolStripMenuItem.Checked;
        }

        private void applyEnvironmentLightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyEnvLighting = applyEnvironmentLightingToolStripMenuItem.Checked;
        }

        private void consecutiveRoomInjectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConsecutiveRoomInject = consecutiveRoomInjectionToolStripMenuItem.Checked;
            UpdateForm();
        }

        private void forceRGBATexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ForceRGBATextures = forceRGBATexturesToolStripMenuItem.Checked;
        }

        private void injectToROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.CheckFileExists = true;
            saveFileDialog1.FileName = "";
            saveFileDialog1.Filter = "Nintendo 64 ROMs (*.z64)|*.z64|All Files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                CurrentScene.ConvertInject(saveFileDialog1.FileName, ConsecutiveRoomInject, ForceRGBATextures);
            }
        }

        private void saveSceneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.CheckFileExists = false;
            if (SceneLoaded == false && Helpers.MakeValidFileName(CurrentScene.Name) != string.Empty)
                saveFileDialog1.FileName = Helpers.MakeValidFileName(CurrentScene.Name) + ".xml";
            else
                saveFileDialog1.FileName = "";
            saveFileDialog1.Filter = "XML Scene File (*.xml)|*.xml|All Files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IO.Export<ZScene>(CurrentScene, saveFileDialog1.FileName);
            }
        }

        private void openSceneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "XML Scene File (*.xml)|*.xml|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                NowLoading = true;

                Camera.Initialize();

                listBox1.DataSource = null;
                listBox2.DataSource = null;
                listBox3.DataSource = null;

                CurrentScene = IO.Import<ZScene>(openFileDialog1.FileName);
                CurrentScene.BasePath = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
                CurrentScene.ColModel = new ObjFile(CurrentScene.CollisionFilename, true);
                for (int i = 0; i < CurrentScene.Rooms.Count; i++)
                {
                    CurrentScene.Rooms[i].ObjModel = new ObjFile(CurrentScene.Rooms[i].ModelFilename);
                    ValidateGroupSettings(ref CurrentScene.Rooms[i].GroupSettings, CurrentScene.Rooms[i].ObjModel.Groups.Count);

                    for (int j = 0; j < CurrentScene.Rooms[i].ObjModel.Groups.Count; j++)
                    {
                        CurrentScene.Rooms[i].ObjModel.Groups[j].TintAlpha = CurrentScene.Rooms[i].GroupSettings.TintAlpha[j];
                        CurrentScene.Rooms[i].ObjModel.Groups[j].TileS = CurrentScene.Rooms[i].GroupSettings.TileS[j];
                        CurrentScene.Rooms[i].ObjModel.Groups[j].TileT = CurrentScene.Rooms[i].GroupSettings.TileT[j];
                        CurrentScene.Rooms[i].ObjModel.Groups[j].PolyType = CurrentScene.Rooms[i].GroupSettings.PolyType[j];
                        CurrentScene.Rooms[i].ObjModel.Groups[j].BackfaceCulling = CurrentScene.Rooms[i].GroupSettings.BackfaceCulling[j];
                        CurrentScene.Rooms[i].ObjModel.Groups[j].MultiTexMaterial = CurrentScene.Rooms[i].GroupSettings.MultiTexMaterial[j];
                        CurrentScene.Rooms[i].ObjModel.Groups[j].ShiftS = CurrentScene.Rooms[i].GroupSettings.ShiftS[j];
                        CurrentScene.Rooms[i].ObjModel.Groups[j].ShiftT = CurrentScene.Rooms[i].GroupSettings.ShiftT[j];
                    }
                    CurrentScene.Rooms[i].ObjModel.Prepare();
                }

                ActorEditControl.UpdateFormDelegate TempDelegate = new ActorEditControl.UpdateFormDelegate(UpdateForm);
                actorEditControl1.SetUpdateDelegate(TempDelegate);
                actorEditControl1.SetLabels("Actor", "Actors");

                actorEditControl2.SetUpdateDelegate(TempDelegate);
                actorEditControl2.SetLabels("Transition", "Transitions");
                actorEditControl2.IsTransitionActor = true;
                actorEditControl2.SetActors(ref CurrentScene.Transitions);

                actorEditControl3.SetUpdateDelegate(TempDelegate);
                actorEditControl3.SetLabels("Spawn", "Spawn Points");
                actorEditControl3.SetActors(ref CurrentScene.SpawnPoints);

                listBox1.DataSource = CurrentScene.Rooms;
                listBox1.DisplayMember = "ModelShortFilename";

                if (listBox1.SelectedItem != null)
                    if (CurrentScene.Rooms.Count != -1)
                        SelectRoom(0);
                SelectObject(-1);

                SetPolyTypesInCollision();

                NowLoading = false;

                UpdateForm();

                SceneLoaded = true;
            }
        }

        private void saveBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Select folder for scene/room file saving:";
            folderBrowserDialog1.SelectedPath = CurrentScene.BasePath;
            folderBrowserDialog1.ShowNewFolderButton = true;

            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderBrowserDialog1.SelectedPath += System.IO.Path.DirectorySeparatorChar;
                CurrentScene.ConvertSave(folderBrowserDialog1.SelectedPath, ConsecutiveRoomInject, ForceRGBATextures);
            }
        }

        private void showReadmeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Application.StartupPath + System.IO.Path.DirectorySeparatorChar + "ReadMe.txt");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                Program.ApplicationTitle + " - Zelda OoT Scene Development System" + Environment.NewLine + Environment.NewLine +
                "Written in 2011/2012 by xdaniel, partially based on code by spinout; see the Readme for more",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Editor - Scene

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && CurrentScene != null)
            {
                CurrentScene.Name = textBox1.Text;
                UpdateForm();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (CurrentScene != null)
                CurrentScene.Scale = (float)numericUpDown1.Value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Wavefront .obj Models (*.obj)|*.obj|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                CurrentScene.AddRoom(openFileDialog1.FileName);
                ((CurrencyManager)listBox1.BindingContext[CurrentScene.Rooms]).Refresh();
                UpdateForm();
                SelectRoom();
            }
        }

        private void textBox2_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) == false)
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && CurrentScene != null)
            {
                CurrentScene.Music = byte.Parse(textBox2.Text, System.Globalization.NumberStyles.Integer);
                UpdateForm();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Wavefront .obj Models (*.obj)|*.obj|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                CurrentScene.ColModel = new ObjFile(openFileDialog1.FileName, true);
                CurrentScene.CollisionFilename = openFileDialog1.FileName;
                UpdateForm();
            }
        }

        private void numericTextBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && CurrentScene != null)
            {
                CurrentScene.InjectOffset = numericTextBox3.IntValue;
                UpdateForm();
            }
        }

        private void textBox4_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) == false)
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && CurrentScene != null)
            {
                CurrentScene.SceneNumber = int.Parse(textBox4.Text, System.Globalization.NumberStyles.Integer);
                UpdateForm();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CurrentScene.IsOutdoors = checkBox1.Checked;
        }

        #endregion

        #region Editor - Rooms, Objects & Actors

        private void FocusOverObjEd(object sender, EventArgs e)
        {
            ApplyObjectEdit();
        }

        private void EditOverObjEd(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                ApplyObjectEdit();
        }

        private void UpdateObjectEdit()
        {
            listBox3.DataSource = ((ZScene.ZRoom)listBox1.SelectedItem).ZObjects;
            listBox3.DisplayMember = "ValueHex";
            ((CurrencyManager)listBox3.BindingContext[((ZScene.ZRoom)listBox1.SelectedItem).ZObjects]).Refresh();
        }

        private void ApplyObjectEdit()
        {
            ((ZScene.ZUShort)listBox3.SelectedItem).Value = ushort.Parse(ListEditBox.Text.PadLeft(4, '0'), System.Globalization.NumberStyles.HexNumber);
            ListEditBox.Hide();
            UpdateForm();
            listBox3.Focus();
        }

        private void SelectObject()
        {
            SelectObject(listBox3.Items.Count - 1);
        }

        private void SelectObject(int Index)
        {
            if (Index >= 0)
            {
                listBox3.DataSource = ((ZScene.ZRoom)listBox1.SelectedItem).ZObjects;
                listBox3.SelectedIndex = Index;
            }
            else
                listBox3.DataSource = null;

            listBox3.DisplayMember = "ValueHex";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                CurrentScene.Rooms.Remove(((ZScene.ZRoom)listBox1.SelectedItem));
                ((CurrencyManager)listBox1.BindingContext[CurrentScene.Rooms]).Refresh();
            }

            listBox2.DataSource = null;

            UpdateForm();
            SelectRoom();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectRoom(listBox1.SelectedIndex);
        }

        private void listBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Y > listBox2.ItemHeight * listBox2.Items.Count)
                listBox2.SelectedIndex = -1;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGroupSelect();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                ((ObjFile.Group)listBox2.SelectedItem).TintAlpha = (uint)(((byte)numericUpDown2.Value << 24) | pictureBox7.BackColor.ToArgb() & 0xFFFFFF);

                int Index = ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups.IndexOf(((ObjFile.Group)listBox2.SelectedItem));
                ((ZScene.ZRoom)listBox1.SelectedItem).GroupSettings.TintAlpha[Index] = ((ObjFile.Group)listBox2.SelectedItem).TintAlpha;

                UpdateGroupSelect();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                ((ObjFile.Group)listBox2.SelectedItem).TileS = comboBox1.SelectedIndex;

                int Index = ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups.IndexOf(((ObjFile.Group)listBox2.SelectedItem));
                ((ZScene.ZRoom)listBox1.SelectedItem).GroupSettings.TileS[Index] = ((ObjFile.Group)listBox2.SelectedItem).TileS;

                UpdateGroupSelect();
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                ((ObjFile.Group)listBox2.SelectedItem).TileT = comboBox2.SelectedIndex;

                int Index = ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups.IndexOf(((ObjFile.Group)listBox2.SelectedItem));
                ((ZScene.ZRoom)listBox1.SelectedItem).GroupSettings.TileT[Index] = ((ObjFile.Group)listBox2.SelectedItem).TileT;

                UpdateGroupSelect();
            }
        }

        private void pictureBox7_DoubleClick(object sender, EventArgs e)
        {
            PictureBox PB = (PictureBox)sender;
            if (ColorPicker(ref PB) == true)
            {
                ((ObjFile.Group)listBox2.SelectedItem).TintAlpha = (uint)(((byte)numericUpDown2.Value << 24) | PB.BackColor.ToArgb() & 0xFFFFFF);

                int Index = ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups.IndexOf(((ObjFile.Group)listBox2.SelectedItem));
                ((ZScene.ZRoom)listBox1.SelectedItem).GroupSettings.TintAlpha[Index] = ((ObjFile.Group)listBox2.SelectedItem).TintAlpha;

                UpdateGroupSelect();
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                ((ObjFile.Group)listBox2.SelectedItem).BackfaceCulling = checkBox3.Checked;

                int Index = ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups.IndexOf(((ObjFile.Group)listBox2.SelectedItem));
                ((ZScene.ZRoom)listBox1.SelectedItem).GroupSettings.BackfaceCulling[Index] = ((ObjFile.Group)listBox2.SelectedItem).BackfaceCulling;

                UpdateGroupSelect();
            }
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                ((ObjFile.Group)listBox2.SelectedItem).ShiftS = (int)numericUpDown5.Value;

                int Index = ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups.IndexOf(((ObjFile.Group)listBox2.SelectedItem));
                ((ZScene.ZRoom)listBox1.SelectedItem).GroupSettings.ShiftS[Index] = ((ObjFile.Group)listBox2.SelectedItem).ShiftS;

                UpdateGroupSelect();
            }
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                ((ObjFile.Group)listBox2.SelectedItem).ShiftT = (int)numericUpDown6.Value;

                int Index = ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups.IndexOf(((ObjFile.Group)listBox2.SelectedItem));
                ((ZScene.ZRoom)listBox1.SelectedItem).GroupSettings.ShiftT[Index] = ((ObjFile.Group)listBox2.SelectedItem).ShiftT;

                UpdateGroupSelect();
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                ((ObjFile.Group)listBox2.SelectedItem).MultiTexMaterial = comboBox3.SelectedIndex - 1; ;

                int Index = ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups.IndexOf(((ObjFile.Group)listBox2.SelectedItem));
                ((ZScene.ZRoom)listBox1.SelectedItem).GroupSettings.MultiTexMaterial[Index] = ((ObjFile.Group)listBox2.SelectedItem).MultiTexMaterial;

                UpdateGroupSelect();
            }
        }

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && CurrentScene != null)
            {
                ((ZScene.ZRoom)listBox1.SelectedItem).InjectOffset = numericTextBox4.IntValue;
                UpdateForm();
            }
        }

        private void listBox3_DoubleClick(object sender, EventArgs e)
        {
            CreateEditBox(sender);

            ListEditBox.KeyPress += new KeyPressEventHandler(this.EditOverObjEd);
            ListEditBox.LostFocus += new EventHandler(this.FocusOverObjEd);
            listBox3.Controls.AddRange(new System.Windows.Forms.Control[] { this.ListEditBox });
            this.ListEditBox.Focus();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ((ZScene.ZRoom)listBox1.SelectedItem).ZObjects.Add(new ZScene.ZUShort(0x0000));
            UpdateForm();
            SelectObject();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedItem != null)
                ((ZScene.ZRoom)listBox1.SelectedItem).ZObjects.Remove(((ZScene.ZUShort)listBox3.SelectedItem));

            SelectObject(-1);
            UpdateForm();
        }

        #endregion

        #region Editor - Environments

        private void SetDefaultEnvironments()
        {
            /* Clear existing environments */
            CurrentScene.Environments.Clear();

            /* Now add default environments ... normal environment */
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x46, 0x2D, 0x39), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0xB4, 0x9A, 0x8A), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x14, 0x14, 0x3C),
                Color.FromArgb(0x8C, 0x78, 0x6E), 0x07E1, 0x3200));
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x69, 0x5A, 0x5A), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0xFF, 0xFF, 0xF0), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x32, 0x32, 0x5A),
                Color.FromArgb(0x64, 0x64, 0x78), 0x07E4, 0x3200));
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x78, 0x5A, 0x00), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0xFA, 0x87, 0x32), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x1E, 0x1E, 0x3C),
                Color.FromArgb(0x78, 0x46, 0x32), 0x07E3, 0x3200));
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x28, 0x46, 0x64), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0x14, 0x14, 0x23), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x32, 0x32, 0x64),
                Color.FromArgb(0x00, 0x00, 0x1E), 0x07E0, 0x3200));

            /* ... underwater environment */
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x3C, 0x28, 0x46), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0x50, 0x1E, 0x3C), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x50, 0x32, 0x96),
                Color.FromArgb(0x46, 0x2B, 0x2D), 0xFFD2, 0x3200));
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x4B, 0x5A, 0x64), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0x37, 0xFF, 0xF0), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x0A, 0x96, 0xBE),
                Color.FromArgb(0x14, 0x5A, 0x6E), 0xFFD2, 0x3200));
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x3C, 0x28, 0x50), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0x3C, 0x4B, 0x96), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x3C, 0x37, 0x96),
                Color.FromArgb(0x32, 0x1E, 0x1E), 0xFFD2, 0x3200));
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x00, 0x28, 0x50), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0x14, 0x32, 0x4B), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x32, 0x64, 0x96),
                Color.FromArgb(0x00, 0x0A, 0x14), 0xFFD2, 0x3200));

            /* ... rainy environment */
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x32, 0x19, 0x25), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0xA0, 0x86, 0x76), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x1E, 0x0A, 0x0A),
                Color.FromArgb(0x28, 0x0F, 0x0F), 0x07DA, 0x3200));
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x5F, 0x50, 0x50), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0x91, 0x91, 0x82), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x28, 0x28, 0x50),
                Color.FromArgb(0x96, 0xA0, 0xAA), 0x07DA, 0x3200));
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x64, 0x46, 0x00), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0x96, 0x46, 0x23), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x0A, 0x0A, 0x19),
                Color.FromArgb(0x14, 0x00, 0x00), 0x07DC, 0x3200));
            CurrentScene.Environments.Add(new ZEnvironment(
                Color.FromArgb(0x14, 0x14, 0x32), Color.FromArgb(0x49, 0x49, 0x49), Color.FromArgb(0x00, 0x00, 0x0F), Color.FromArgb(0xB7, 0xB7, 0xB7), Color.FromArgb(0x1E, 0x1E, 0x50),
                Color.FromArgb(0x00, 0x00, 0x0A), 0x07DC, 0x3200));
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            CurrentScene.Environments.Add(new ZEnvironment(Color.Red, Color.Green, Color.Blue, Color.Blue, Color.White, Color.Pink, 0x07E4, 0x3200));
            UpdateForm();
            numericUpDown11.Value = numericUpDown11.Maximum;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ZEnvironment DelEnv = CurrentScene.Environments[(int)numericUpDown11.Value - 1];
            CurrentScene.Environments.Remove(DelEnv);
            UpdateForm();
        }

        private void UpdateEnvironmentEdit()
        {
            if (CurrentScene.Environments.Count != 0)
            {
                numericUpDown11.Minimum = 1;
                numericUpDown11.Maximum = CurrentScene.Environments.Count;
                numericUpDown11.Enabled = true;

                pictureBox1.BackColor = CurrentScene.Environments[(int)numericUpDown11.Value - 1].C1C;
                pictureBox2.BackColor = CurrentScene.Environments[(int)numericUpDown11.Value - 1].C2C;
                pictureBox3.BackColor = CurrentScene.Environments[(int)numericUpDown11.Value - 1].C3C;
                pictureBox4.BackColor = CurrentScene.Environments[(int)numericUpDown11.Value - 1].C4C;
                pictureBox5.BackColor = CurrentScene.Environments[(int)numericUpDown11.Value - 1].C5C;
                pictureBox6.BackColor = CurrentScene.Environments[(int)numericUpDown11.Value - 1].FogColorC;
                numericTextBox7.Text = CurrentScene.Environments[(int)numericUpDown11.Value - 1].FogDistance.ToString("X4");
                numericTextBox8.Text = CurrentScene.Environments[(int)numericUpDown11.Value - 1].DrawDistance.ToString("X4");

                foreach (Control Ctrl in panel3.Controls)
                    Ctrl.Enabled = true;

                button11.Enabled = true;
            }
            else
            {
                numericUpDown11.Minimum = 0;
                numericUpDown11.Maximum = 0;
                numericUpDown11.Value = 0;
                numericUpDown11.Enabled = false;

                pictureBox1.BackColor = Color.White;
                pictureBox2.BackColor = Color.White;
                pictureBox3.BackColor = Color.White;
                pictureBox4.BackColor = Color.White;
                pictureBox5.BackColor = Color.White;
                pictureBox6.BackColor = Color.White;
                numericTextBox7.Text = string.Empty;
                numericTextBox8.Text = string.Empty;

                foreach (Control Ctrl in panel3.Controls)
                    Ctrl.Enabled = false;

                button11.Enabled = false;
            }
        }

        private void UpdateEnvironmentData()
        {
            CurrentScene.Environments[(int)numericUpDown11.Value - 1].C1C = pictureBox1.BackColor;
            CurrentScene.Environments[(int)numericUpDown11.Value - 1].C2C = pictureBox2.BackColor;
            CurrentScene.Environments[(int)numericUpDown11.Value - 1].C3C = pictureBox3.BackColor;
            CurrentScene.Environments[(int)numericUpDown11.Value - 1].C4C = pictureBox4.BackColor;
            CurrentScene.Environments[(int)numericUpDown11.Value - 1].C5C = pictureBox5.BackColor;
            CurrentScene.Environments[(int)numericUpDown11.Value - 1].FogColorC = pictureBox6.BackColor;
            CurrentScene.Environments[(int)numericUpDown11.Value - 1].FogDistance = ushort.Parse(numericTextBox7.Text, System.Globalization.NumberStyles.HexNumber);
            CurrentScene.Environments[(int)numericUpDown11.Value - 1].DrawDistance = ushort.Parse(numericTextBox8.Text, System.Globalization.NumberStyles.HexNumber);

            UpdateForm();
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            PictureBox PB = (PictureBox)sender;
            if (ColorPicker(ref PB) == true)
                UpdateEnvironmentData();
        }

        private void pictureBox2_DoubleClick(object sender, EventArgs e)
        {
            PictureBox PB = (PictureBox)sender;
            if (ColorPicker(ref PB) == true)
                UpdateEnvironmentData();
        }

        private void pictureBox3_DoubleClick(object sender, EventArgs e)
        {
            PictureBox PB = (PictureBox)sender;
            if (ColorPicker(ref PB) == true)
                UpdateEnvironmentData();
        }

        private void pictureBox4_DoubleClick(object sender, EventArgs e)
        {
            PictureBox PB = (PictureBox)sender;
            if (ColorPicker(ref PB) == true)
                UpdateEnvironmentData();
        }

        private void pictureBox5_DoubleClick(object sender, EventArgs e)
        {
            PictureBox PB = (PictureBox)sender;
            if (ColorPicker(ref PB) == true)
                UpdateEnvironmentData();
        }

        private void pictureBox6_DoubleClick(object sender, EventArgs e)
        {
            PictureBox PB = (PictureBox)sender;
            if (ColorPicker(ref PB) == true)
                UpdateEnvironmentData();
        }

        private void numericTextBox8_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                UpdateEnvironmentData();
        }

        private void numericTextBox7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                UpdateEnvironmentData();
        }

        #endregion

        #region Editor - Waterboxes

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (CurrentScene.ColModel == null) return;

            OpenTK.Vector3d MinCoordinate = new OpenTK.Vector3d(0, 0, 0);
            OpenTK.Vector3d MaxCoordinate = new OpenTK.Vector3d(0, 0, 0);

            foreach (ObjFile.Vertex Vtx in CurrentScene.ColModel.Vertices)
            {
                /* Minimum... */
                MinCoordinate.X = Math.Min(MinCoordinate.X, Vtx.X * CurrentScene.Scale);
                MinCoordinate.Y = Math.Min(MinCoordinate.Y, Vtx.Y * CurrentScene.Scale);
                MinCoordinate.Z = Math.Min(MinCoordinate.Z, Vtx.Z * CurrentScene.Scale);

                /* Maximum... */
                MaxCoordinate.X = Math.Max(MaxCoordinate.X, Vtx.X * CurrentScene.Scale);
                MaxCoordinate.Y = Math.Max(MaxCoordinate.Y, Vtx.Y * CurrentScene.Scale);
                MaxCoordinate.Z = Math.Max(MaxCoordinate.Z, Vtx.Z * CurrentScene.Scale);
            }

            CurrentScene.Waterboxes.Add(new ZWaterbox((float)MinCoordinate.X, (float)-20.0f, (float)MinCoordinate.Z, (float)(MaxCoordinate.X - MinCoordinate.X), (float)(MaxCoordinate.Z - MinCoordinate.Z), 0x00000100));
            UpdateForm();
            numericUpDown10.Value = numericUpDown10.Maximum;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ZWaterbox DelWBox = CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1];
            CurrentScene.Waterboxes.Remove(DelWBox);
            UpdateForm();
        }

        private void UpdateWaterboxEdit()
        {
            if (CurrentScene.Waterboxes.Count != 0)
            {
                numericUpDown10.Minimum = 1;
                numericUpDown10.Maximum = CurrentScene.Waterboxes.Count;
                numericUpDown10.Enabled = true;

                numericTextBox6.Text = CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].Properties.ToString("X8");
                numericUpDownEx2.Value = (decimal)CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].XPos;
                numericUpDownEx4.Value = (decimal)CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].YPos;
                numericUpDownEx6.Value = (decimal)CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].ZPos;
                numericUpDownEx5.Value = (decimal)CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].XSize;
                numericUpDownEx1.Value = (decimal)CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].ZSize;

                foreach (Control Ctrl in panel1.Controls)
                    Ctrl.Enabled = true;

                button9.Enabled = true;
            }
            else
            {
                numericUpDown10.Minimum = 0;
                numericUpDown10.Maximum = 0;
                numericUpDown10.Value = 0;
                numericUpDown10.Enabled = false;

                numericTextBox6.Text = string.Empty;
                numericUpDownEx2.Value = 0;
                numericUpDownEx4.Value = 0;
                numericUpDownEx6.Value = 0;
                numericUpDownEx5.Value = 0;
                numericUpDownEx1.Value = 0;

                foreach (Control Ctrl in panel1.Controls)
                    Ctrl.Enabled = false;

                button9.Enabled = false;
            }
        }

        private void UpdateWaterboxData()
        {
            CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].Properties = ushort.Parse(numericTextBox6.Text, System.Globalization.NumberStyles.HexNumber);
            CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].XPos = (float)numericUpDownEx2.Value;
            CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].YPos = (float)numericUpDownEx4.Value;
            CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].ZPos = (float)numericUpDownEx6.Value;
            CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].XSize = (float)numericUpDownEx5.Value;
            CurrentScene.Waterboxes[(int)numericUpDown10.Value - 1].ZSize = (float)numericUpDownEx1.Value;
            UpdateForm();
        }

        private void numericTextBox6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                UpdateWaterboxData();
        }

        private void numericUpDownEx2_ValueChanged(object sender, EventArgs e)
        {
            UpdateWaterboxData();
        }

        private void numericUpDownEx4_ValueChanged(object sender, EventArgs e)
        {
            UpdateWaterboxData();
        }

        private void numericUpDownEx6_ValueChanged(object sender, EventArgs e)
        {
            UpdateWaterboxData();
        }

        private void numericUpDownEx5_ValueChanged(object sender, EventArgs e)
        {
            UpdateWaterboxData();
        }

        private void numericUpDownEx1_ValueChanged(object sender, EventArgs e)
        {
            UpdateWaterboxData();
        }

        #endregion

        #region Editor - Collision

        /* Special FX flags
         * 400 -> climbable ladder
         * 800 -> whole surface climbable
         * 008 -> quicksand (shallow)
         * 018 -> quicksand (deep, kills)
         * 004 -> lava damage
         */
        private void UpdatePolyTypeEdit()
        {
            if (CurrentScene.PolyTypes.Count != 0)
            {
                numericUpDown3.Minimum = 1;
                numericUpDown3.Maximum = CurrentScene.PolyTypes.Count;
                numericUpDown3.Enabled = true;

                if (CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].ExitNumber >= CurrentScene.ExitList.Count)
                    CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].ExitNumber = CurrentScene.ExitList.Count;
                numericUpDownEx10.Value = CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].ExitNumber;
                numericUpDownEx10.Maximum = CurrentScene.ExitList.Count;

                if (CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].EnvNumber >= CurrentScene.Environments.Count)
                    CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].EnvNumber = CurrentScene.Environments.Count;
                numericUpDownEx7.Value = CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].EnvNumber;
                numericUpDownEx7.Maximum = CurrentScene.Environments.Count;

                numericUpDownEx3.Value = CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].EchoRange;
                numericUpDownEx9.Value = CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].GroundType;
                numericUpDownEx8.Value = CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].TerrainType;
                checkBox2.Checked = CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].IsSteep;
                checkBox4.Checked = CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].IsHookshotable;

                if (CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].ClimbingCrawlingFlags == 0x0)
                    radioButton1.Checked = true;
                else if (CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].ClimbingCrawlingFlags == 0x4)
                    radioButton2.Checked = true;
                else if (CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].ClimbingCrawlingFlags == 0x8)
                    radioButton3.Checked = true;
                else
                {
                    radioButton1.Checked = false;
                    radioButton2.Checked = false;
                    radioButton3.Checked = false;
                }

                if (CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].DamageSurfaceFlags == 0x00)
                    radioButton7.Checked = true;
                else if (CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].DamageSurfaceFlags == 0x08)
                    radioButton4.Checked = true;
                else if (CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].DamageSurfaceFlags == 0x18)
                    radioButton5.Checked = true;
                else if (CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].DamageSurfaceFlags == 0x04)
                    radioButton6.Checked = true;
                else
                {
                    radioButton7.Checked = false;
                    radioButton4.Checked = false;
                    radioButton5.Checked = false;
                    radioButton6.Checked = false;
                }

                numericTextBox1.Text = CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].Raw.ToString("X16");

                foreach (Control Ctrl in panel2.Controls)
                    Ctrl.Enabled = true;

                button1.Enabled = true;
            }
            else
            {
                numericUpDown3.Minimum = 0;
                numericUpDown3.Maximum = 0;
                numericUpDown3.Value = 0;
                numericUpDown3.Enabled = false;

                numericUpDownEx10.Value = 0;
                numericUpDownEx7.Value = 0;
                numericUpDownEx3.Value = 0;
                numericUpDownEx9.Value = 0;
                numericUpDownEx8.Value = 0;
                checkBox2.Checked = false;
                checkBox4.Checked = false;

                radioButton1.Checked = false;
                radioButton2.Checked = false;
                radioButton3.Checked = false;

                radioButton7.Checked = false;
                radioButton4.Checked = false;
                radioButton5.Checked = false;
                radioButton6.Checked = false;

                numericTextBox1.Text = string.Empty;

                foreach (Control Ctrl in panel2.Controls)
                    Ctrl.Enabled = false;

                button1.Enabled = false;
            }
        }

        private void UpdateClimbableCrawlableFlags()
        {
            if (radioButton1.Checked == true)
                CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].ClimbingCrawlingFlags = 0x0;
            else if (radioButton2.Checked == true)
                CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].ClimbingCrawlingFlags = 0x4;
            else if (radioButton3.Checked == true)
                CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].ClimbingCrawlingFlags = 0x8;
        }

        private void UpdateDamageSurfaceFlags()
        {
            if (radioButton7.Checked == true)
                CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].DamageSurfaceFlags = 0x00;
            else if (radioButton4.Checked == true)
                CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].DamageSurfaceFlags = 0x08;
            else if (radioButton5.Checked == true)
                CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].DamageSurfaceFlags = 0x18;
            else if (radioButton6.Checked == true)
                CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].DamageSurfaceFlags = 0x04;
        }

        private void numericTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].Raw = ulong.Parse(numericTextBox1.Text, System.Globalization.NumberStyles.HexNumber);
                UpdateForm();
            }
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            CurrentScene.PolyTypes.Add(new ZColPolyType());
            UpdateForm();
            numericUpDown3.Value = numericUpDown3.Maximum;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ZColPolyType DelPT = CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1];
            CurrentScene.PolyTypes.Remove(DelPT);
            UpdateForm();
        }

        private void SetPolyTypesInCollision()
        {
            /* See this? Pure lazyness. Collision poly types are stored in the -room's- model data, while later on they're read from the -scene's- collision model.
             * So what do I do here? For each group in each room, I go through the collision model's groups and see if their names match up. If they do, I copy over
             * the poly type from the room to the collision model. I -could've- done this differently from the start, but a brainfart hindered me... *cough*
             */
            foreach (ZScene.ZRoom Room in CurrentScene.Rooms)
            {
                foreach (ObjFile.Group RoomGrp in Room.ObjModel.Groups)
                {
                    foreach (ObjFile.Group SceneGrp in CurrentScene.ColModel.Groups)
                    {
                        if (RoomGrp.Name == SceneGrp.Name)
                            SceneGrp.PolyType = RoomGrp.PolyType;
                    }
                }
            }
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null && CurrentScene.ColModel != null)
            {
                ((ObjFile.Group)listBox2.SelectedItem).PolyType = ((int)numericUpDown4.Value - 1);

                foreach (ObjFile.Group Grp in CurrentScene.ColModel.Groups)
                {
                    if (Grp.Name == ((ObjFile.Group)listBox2.SelectedItem).Name)
                        Grp.PolyType = ((int)numericUpDown4.Value - 1);
                }

                int Index = ((ZScene.ZRoom)listBox1.SelectedItem).ObjModel.Groups.IndexOf(((ObjFile.Group)listBox2.SelectedItem));
                ((ZScene.ZRoom)listBox1.SelectedItem).GroupSettings.PolyType[Index] = ((ObjFile.Group)listBox2.SelectedItem).PolyType;

                UpdateGroupSelect();
            }
        }

        private void numericUpDownEx10_ValueChanged(object sender, EventArgs e)
        {
            CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].ExitNumber = (int)numericUpDownEx10.Value;
            UpdateForm();
        }

        private void numericUpDownEx3_ValueChanged(object sender, EventArgs e)
        {
            CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].EchoRange = (int)numericUpDownEx3.Value;
            UpdateForm();
        }

        private void numericUpDownEx7_ValueChanged(object sender, EventArgs e)
        {
            CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].EnvNumber = (int)numericUpDownEx7.Value;
            UpdateForm();
        }

        private void numericUpDownEx9_ValueChanged(object sender, EventArgs e)
        {
            CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].GroundType = (int)numericUpDownEx9.Value;
            UpdateForm();
        }

        private void numericUpDownEx8_ValueChanged(object sender, EventArgs e)
        {
            CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].TerrainType = (int)numericUpDownEx8.Value;
            UpdateForm();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].IsSteep = checkBox2.Checked;
            UpdateForm();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            CurrentScene.PolyTypes[(int)numericUpDown3.Value - 1].IsHookshotable = checkBox4.Checked;
            UpdateForm();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateClimbableCrawlableFlags();
            UpdateForm();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            UpdateClimbableCrawlableFlags();
            UpdateForm();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            UpdateClimbableCrawlableFlags();
            UpdateForm();
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDamageSurfaceFlags();
            UpdateForm();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDamageSurfaceFlags();
            UpdateForm();
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDamageSurfaceFlags();
            UpdateForm();
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDamageSurfaceFlags();
            UpdateForm();
        }

        #endregion

        #region Editor - Exit List

        private void listBox4_DoubleClick(object sender, EventArgs e)
        {
            CreateEditBox(sender);

            ListEditBox.KeyPress += new KeyPressEventHandler(this.EditOverExitEd);
            ListEditBox.LostFocus += new EventHandler(this.FocusOverExitEd);
            listBox4.Controls.AddRange(new System.Windows.Forms.Control[] { this.ListEditBox });
            this.ListEditBox.Focus();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            CurrentScene.ExitList.Add(new ZScene.ZUShort(0x0000));
            UpdateForm();
            SelectExit();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (listBox4.SelectedItem != null)
                CurrentScene.ExitList.Remove(((ZScene.ZUShort)listBox4.SelectedItem));

            SelectExit(-1);
            UpdateForm();
        }

        private void FocusOverExitEd(object sender, EventArgs e)
        {
            ApplyExitEdit();
        }

        private void EditOverExitEd(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                ApplyExitEdit();
        }

        private void UpdateExitEdit()
        {
            listBox4.DataSource = CurrentScene.ExitList;
            listBox4.DisplayMember = "ValueHex";
            ((CurrencyManager)listBox4.BindingContext[CurrentScene.ExitList]).Refresh();
        }

        private void ApplyExitEdit()
        {
            ((ZScene.ZUShort)listBox4.SelectedItem).Value = ushort.Parse(ListEditBox.Text.PadLeft(4, '0'), System.Globalization.NumberStyles.HexNumber);
            ListEditBox.Hide();
            UpdateForm();
            listBox4.Focus();
        }

        private void SelectExit()
        {
            SelectExit(listBox4.Items.Count - 1);
        }

        private void SelectExit(int Index)
        {
            if (Index >= 0)
            {
                listBox4.DataSource = CurrentScene.ExitList;
                listBox4.SelectedIndex = Index;
            }
            else
                listBox4.DataSource = null;

            listBox4.DisplayMember = "ValueHex";
        }

        #endregion
    }

    #region Extensions

    public static class ArrayExtensions
    {
        public static void Init<T>(this T[] array, T defaultValue)
        {
            if (array == null)
                return;

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = defaultValue;
            }
        }

        public static void Fill<T>(this T[] array, T[] data)
        {
            if (array == null)
                return;

            for (int i = 0; i < array.Length; i += data.Length)
            {
                for (int j = 0; j < data.Length; j++)
                {
                    try
                    {
                        array[i + j] = data[j];
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }
    }

    #endregion
}

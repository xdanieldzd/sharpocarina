using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpOcarina
{
    public partial class ActorEditControl : UserControl
    {
        public delegate void UpdateFormDelegate();
        public UpdateFormDelegate UpdateForm = null;

        public OpenTK.Vector3d CenterPoint = new OpenTK.Vector3d();

        List<ZActor> Actors = new List<ZActor>();

        public ActorEditControl()
        {
            InitializeComponent();
        }

        public bool IsTransitionActor = false;

        public int ActorNumber
        {
            get { return (int)numericUpDown3.Value; }
            set { numericUpDown3.Value = value; }
        }

        public void SetUpdateDelegate(UpdateFormDelegate Delegate)
        {
            UpdateForm = Delegate;
            UpdateActorEdit();
        }

        public void SetLabels(string TypeName, string GroupLabel)
        {
            groupBox3.Text = GroupLabel;
            button6.Text = "Add " + TypeName;
            button5.Text = "Delete " + TypeName;
        }

        public void SetActors(ref List<ZActor> ActorList)
        {
            Actors = ActorList;
            UpdateActorEdit();
        }

        public void ClearActors()
        {
            Actors = new List<ZActor>();
            CenterPoint = new OpenTK.Vector3d();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (Actors == null || UpdateForm == null) throw new Exception("Interface values not set");

            if (IsTransitionActor == false)
                Actors.Add(new ZActor(0, (float)CenterPoint.X, (float)CenterPoint.Y, (float)CenterPoint.Z, 0.0f, 0.0f, 0.0f, 0));
            else
                Actors.Add(new ZActor(0x00, 0x00, 0x00, 0x00, 0x0000, 0.0f, 0.0f, 0.0f, 0.0f, 0x0000));

            UpdateActorEdit();
            UpdateForm();
            numericUpDown3.Value = numericUpDown3.Maximum;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (Actors == null || UpdateForm == null) throw new Exception("Interface values not set");

            Actors.Remove(Actors[(int)numericUpDown3.Value - 1]);
            UpdateActorEdit();
            UpdateForm();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (Actors == null || UpdateForm == null) throw new Exception("Interface values not set");

            UpdateActorEdit();
            UpdateForm();
        }

        private void ShowHideControls()
        {
            if (IsTransitionActor == false)
            {
                // show
                numericUpDown9.Visible = true;
                numericUpDown7.Visible = true;
                label16.Visible = true;
                label14.Visible = true;

                // hide
                label2.Visible = false;
                label3.Visible = false;
                label1.Visible = false;
                label4.Visible = false;
                numericTextBox4.Visible = false;
                numericTextBox5.Visible = false;
                numericTextBox6.Visible = false;
                numericTextBox3.Visible = false;
            }
            else
            {
                // show
                label2.Visible = true;
                label3.Visible = true;
                label1.Visible = true;
                label4.Visible = true;
                numericTextBox4.Visible = true;
                numericTextBox5.Visible = true;
                numericTextBox6.Visible = true;
                numericTextBox3.Visible = true;

                // hide
                numericUpDown9.Visible = false;
                numericUpDown7.Visible = false;
                label16.Visible = false;
                label14.Visible = false;
            }
        }

        private void UpdateActorEdit()
        {
            ShowHideControls();

            if (Actors.Count != 0)
            {
                numericUpDown3.Minimum = 1;
                numericUpDown3.Maximum = Actors.Count;
                numericUpDown3.Enabled = true;

                numericTextBox1.Text = Actors[(int)numericUpDown3.Value - 1].Number.ToString("X4");
                numericTextBox2.Text = Actors[(int)numericUpDown3.Value - 1].Variable.ToString("X4");
                numericUpDown4.Value = (decimal)Actors[(int)numericUpDown3.Value - 1].XPos;
                numericUpDown5.Value = (decimal)Actors[(int)numericUpDown3.Value - 1].YPos;
                numericUpDown6.Value = (decimal)Actors[(int)numericUpDown3.Value - 1].ZPos;
                numericUpDown8.Value = (decimal)Actors[(int)numericUpDown3.Value - 1].YRot;

                if (IsTransitionActor == false)
                {
                    numericUpDown9.Value = (decimal)Actors[(int)numericUpDown3.Value - 1].XRot;
                    numericUpDown7.Value = (decimal)Actors[(int)numericUpDown3.Value - 1].ZRot;
                }
                else
                {
                    numericTextBox4.Text = Actors[(int)numericUpDown3.Value - 1].FrontSwitchTo.ToString("X2");
                    numericTextBox5.Text = Actors[(int)numericUpDown3.Value - 1].FrontCamera.ToString("X2");
                    numericTextBox6.Text = Actors[(int)numericUpDown3.Value - 1].BackSwitchTo.ToString("X2");
                    numericTextBox3.Text = Actors[(int)numericUpDown3.Value - 1].BackCamera.ToString("X2");
                }

                foreach (Control Ctrl in panel2.Controls)
                    Ctrl.Enabled = true;

                button5.Enabled = true;
                button6.Enabled = true;
            }
            else
            {
                numericUpDown3.Minimum = 0;
                numericUpDown3.Maximum = 0;
                numericUpDown3.Value = 0;
                numericUpDown3.Enabled = false;

                numericTextBox1.Text = string.Empty;
                numericTextBox2.Text = string.Empty;
                numericUpDown4.Value = 0;
                numericUpDown5.Value = 0;
                numericUpDown6.Value = 0;
                numericUpDown8.Value = 0;

                numericUpDown9.Value = 0;
                numericUpDown7.Value = 0;

                foreach (Control Ctrl in panel2.Controls)
                    Ctrl.Enabled = false;

                button5.Enabled = false;
                button6.Enabled = true;
            }
        }

        private void UpdateActorData()
        {
            if (numericTextBox1.Text == string.Empty || numericTextBox2.Text == string.Empty) return;

            Actors[(int)numericUpDown3.Value - 1].Number = ushort.Parse(numericTextBox1.Text, System.Globalization.NumberStyles.HexNumber);
            Actors[(int)numericUpDown3.Value - 1].Variable = ushort.Parse(numericTextBox2.Text, System.Globalization.NumberStyles.HexNumber);
            Actors[(int)numericUpDown3.Value - 1].XPos = (short)numericUpDown4.Value;
            Actors[(int)numericUpDown3.Value - 1].YPos = (short)numericUpDown5.Value;
            Actors[(int)numericUpDown3.Value - 1].ZPos = (short)numericUpDown6.Value;
            Actors[(int)numericUpDown3.Value - 1].YRot = (short)numericUpDown8.Value;

            if (IsTransitionActor == false)
            {
                Actors[(int)numericUpDown3.Value - 1].XRot = (short)numericUpDown9.Value;
                Actors[(int)numericUpDown3.Value - 1].ZRot = (short)numericUpDown7.Value;
            }
            else
            {
                Actors[(int)numericUpDown3.Value - 1].FrontSwitchTo = byte.Parse(numericTextBox4.Text, System.Globalization.NumberStyles.HexNumber);
                Actors[(int)numericUpDown3.Value - 1].FrontCamera = byte.Parse(numericTextBox5.Text, System.Globalization.NumberStyles.HexNumber);
                Actors[(int)numericUpDown3.Value - 1].BackSwitchTo = byte.Parse(numericTextBox6.Text, System.Globalization.NumberStyles.HexNumber);
                Actors[(int)numericUpDown3.Value - 1].BackCamera = byte.Parse(numericTextBox3.Text, System.Globalization.NumberStyles.HexNumber);
            }

            UpdateActorEdit();
            UpdateForm();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            UpdateActorData();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            UpdateActorData();
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            UpdateActorData();
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            UpdateActorData();
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            UpdateActorData();
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            UpdateActorData();
        }

        private void numericTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                UpdateActorData();
        }

        private void numericTextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                UpdateActorData();
        }

        private void numericTextBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                UpdateActorData();
        }

        private void numericTextBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                UpdateActorData();
        }

        private void numericTextBox6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                UpdateActorData();
        }

        private void numericTextBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                UpdateActorData();
        }
    }
}

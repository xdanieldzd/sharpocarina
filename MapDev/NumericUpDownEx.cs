using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SharpOcarina
{
    public class NumericUpDownEx : NumericUpDown
    {
        int displayDigits = 1;

        bool doValueRollover = true;

        EventHandler eventHandler = null;
        bool alwaysFireValueChanged = false;

        [Category("Appearance")]
        [Description("Amount of digits displayed.")]
        public int DisplayDigits
        {
            set
            {
                displayDigits = value;
            }
            get
            {
                return this.displayDigits;
            }
        }

        protected override void UpdateEditText()
        {
            if (base.Hexadecimal == true)
                base.Text = string.Format(@"{0:X" + displayDigits + "}", (Int32)base.Value);
            else
                base.Text = string.Format(@"{0:D" + displayDigits + "}", (Int32)base.Value);
        }

        [Category("Behavior")]
        [Description("Roll over value when minimum or maximum is hit.")]
        public bool DoValueRollover
        {
            set
            {
                doValueRollover = value;
            }
            get
            {
                return this.doValueRollover;
            }
        }

        [Category("Behavior")]
        [Description("Always fire the ValueChanged event, regardless of the value being changed by the user or code.")]
        public bool AlwaysFireValueChanged
        {
            set
            {
                alwaysFireValueChanged = value;
            }
            get
            {
                return this.alwaysFireValueChanged;
            }
        }

        public new event EventHandler ValueChanged
        {
            add
            {
                eventHandler += value;
                base.ValueChanged += value;
            }

            remove
            {
                eventHandler -= value;
                base.ValueChanged -= value;
            }
        }

        public new decimal Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (alwaysFireValueChanged == true)
                {
                    base.Value = value;
                }
                else
                {
                    base.ValueChanged -= eventHandler;
                    base.Value = value;
                    base.ValueChanged += eventHandler;
                }
            }
        }

        public new decimal Maximum
        {
            get
            {
                if (doValueRollover)
                    return base.Maximum - 1;
                else
                    return base.Maximum;
            }
            set
            {
                if (doValueRollover)
                    base.Maximum = value + 1;
                else
                    base.Maximum = value;
            }
        }

        public new decimal Minimum
        {
            get
            {
                if (doValueRollover)
                    return base.Minimum + 1;
                else
                    return base.Minimum;
            }
            set
            {
                if (doValueRollover)
                    base.Minimum = value - 1;
                else
                    base.Minimum = value;
            }
        }

        protected override void OnValueChanged(EventArgs e)
        {
            // Make value roll over once (minimum + 1) or (maximum - 1) is hit
            if (doValueRollover)
            {
                if (Value > base.Maximum - 1)
                    Value = base.Minimum + 1;
                else if (Value < base.Minimum + 1)
                    Value = base.Maximum - 1;
            }

            base.OnValueChanged(e);
        }
    }
}

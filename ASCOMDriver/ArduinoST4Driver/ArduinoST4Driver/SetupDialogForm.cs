using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASCOM.ArduinoST4
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        public SetupDialogForm()
        {
            InitializeComponent();
            populateSerialComboBox(Telescope.comPort);
            // Initialise current values of user settings from the ASCOM Profile 
            this.traceStateCheckBox.Checked = Telescope.traceState;
            this.rightAscensionPlusSideralRateTextBox.Text = Telescope.rightAscensionSideralRatePlus.ToString();
            this.rightAscensionMinusSideralRateTextBox.Text = Telescope.rightAscensionSideralRateMinus.ToString();
            this.declinationPlusSideralRateTextBox.Text = Telescope.declinationSideralRatePlus.ToString();
            this.declinationMinusSideralRateTextBox.Text = Telescope.declinationSideralRateMinus.ToString();
            this.mountCompensatesEarthRotationInSlewCheckBox.Checked = Telescope.mountCompensatesEarthRotationInSlew;
            this.meridianFlipCheckBox.Checked = Telescope.meridianFlip;
        }

        /// <summary>
        /// Reads the available COM ports on the computer and adds them to the COM Port combobox
        /// </summary>
        private void populateSerialComboBox(string selected)
        {
            string[] serialPorts = System.IO.Ports.SerialPort.GetPortNames();
            Array.Sort(serialPorts);
            fillComboBoxFromArray(this.comPortComboBox, serialPorts, selected);
        }
  
        /// <summary>
        /// Fills a combobox with the given string values
        /// </summary>
        private void fillComboBoxFromArray(ComboBox combobox, string[] values, string selected)
        {
            combobox.Items.Clear();
            if (values.Length == 0)
            {
                return;
            }
            bool selectionOccurred = false;
            foreach (string value in values)
            {
                combobox.Items.Add(value);
                if (value == selected)
                {
                    combobox.Text = value;
                    selectionOccurred = true;
                }
            }
            if (!selectionOccurred)
            {
                combobox.Text = values[0];
            }
        }

        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Update the state variables with results from the dialogue
            Telescope.comPort = comPortComboBox.Text;
            Telescope.traceState = traceStateCheckBox.Checked;
            Telescope.rightAscensionSideralRatePlus = Convert.ToDouble(this.rightAscensionPlusSideralRateTextBox.Text);
            Telescope.rightAscensionSideralRateMinus = Convert.ToDouble(this.rightAscensionMinusSideralRateTextBox.Text);
            Telescope.declinationSideralRatePlus = Convert.ToDouble(this.declinationPlusSideralRateTextBox.Text);
            Telescope.declinationSideralRateMinus = Convert.ToDouble(this.declinationMinusSideralRateTextBox.Text);
            Telescope.mountCompensatesEarthRotationInSlew = this.mountCompensatesEarthRotationInSlewCheckBox.Checked;
            Telescope.meridianFlip = this.meridianFlipCheckBox.Checked;
        }

        private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }
    }
}
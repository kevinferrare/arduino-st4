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
            Configuration configuration = Configuration.Instance;
            PopulateDeviceComboBox(configuration.Device);
            PopulateSerialComboBox(configuration.ComPort);
            // Initialise current values of user settings from the ASCOM Profile 
            this.traceStateCheckBox.Checked = configuration.TraceState;
            this.rightAscensionPlusSideralRateTextBox.Text = configuration.RightAscensionSideralRatePlus.ToString();
            this.rightAscensionMinusSideralRateTextBox.Text = configuration.RightAscensionSideralRateMinus.ToString();
            this.declinationPlusSideralRateTextBox.Text = configuration.DeclinationSideralRatePlus.ToString();
            this.declinationMinusSideralRateTextBox.Text = configuration.DeclinationSideralRateMinus.ToString();
            this.mountCompensatesEarthRotationInSlewCheckBox.Checked = configuration.MountCompensatesEarthRotationInSlew;
            this.meridianFlipCheckBox.Checked = configuration.MeridianFlip;
        }

        private void PopulateDeviceComboBox(string selected)
        {
            string[] devices = { DeviceType.ARDUINO.ToString(), DeviceType.DUMMY.ToString() };
            FillComboBoxFromArray(this.deviceComboBox, devices, selected);
        }
        /// <summary>
        /// Reads the available COM ports on the computer and adds them to the COM Port combobox
        /// </summary>
        private void PopulateSerialComboBox(string selected)
        {
            string[] serialPorts = System.IO.Ports.SerialPort.GetPortNames();
            Array.Sort(serialPorts);
            FillComboBoxFromArray(this.comPortComboBox, serialPorts, selected);
        }
  
        /// <summary>
        /// Fills a combobox with the given string values
        /// </summary>
        private void FillComboBoxFromArray(ComboBox combobox, string[] values, string selected)
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

        private void CmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Update the state variables with results from the dialogue
            Configuration configuration = Configuration.Instance;
            configuration.ComPort = comPortComboBox.Text;
            configuration.Device = deviceComboBox.Text;
            configuration.TraceState = traceStateCheckBox.Checked;
            configuration.RightAscensionSideralRatePlus = Convert.ToDouble(this.rightAscensionPlusSideralRateTextBox.Text);
            configuration.RightAscensionSideralRateMinus = Convert.ToDouble(this.rightAscensionMinusSideralRateTextBox.Text);
            configuration.DeclinationSideralRatePlus = Convert.ToDouble(this.declinationPlusSideralRateTextBox.Text);
            configuration.DeclinationSideralRateMinus = Convert.ToDouble(this.declinationMinusSideralRateTextBox.Text);
            configuration.MountCompensatesEarthRotationInSlew = this.mountCompensatesEarthRotationInSlewCheckBox.Checked;
            configuration.MeridianFlip = this.meridianFlipCheckBox.Checked;
        }

        private void CmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
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
// This file is part of Arduino ST4.
//
// Arduino ST4 is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Arduino ST4 is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Arduino ST4.  If not, see <http://www.gnu.org/licenses/>.

// ASCOM Telescope driver for ArduinoST4
//
// Description:	Driver for an USB-ST4 interface implemented with an arduino.
//              Pulse guiding and slew is supported, allowing for very slow goto :)
//
// Implements:	ASCOM Telescope interface version: 3
// Author:		(Kevin FERRARE) kevinferrare@gmail.com

using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Runtime.InteropServices;

namespace ASCOM.ArduinoST4
{
    /// <summary>
    /// ASCOM Telescope Driver for ArduinoST4.
    /// A big part of the code in this class is autogenerated boilerplate for ASCOM API.
    /// Real logic is under AxisPositionController.
    /// 
    /// GUID is for COM identification of the component.
    /// ClassInterface is to make it invisible to other COM clients.
    /// </summary>
    /// Author:  Kevin Ferrare
    [Guid("045b2ced-6f70-4a3a-8483-1891f235deb1")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Telescope : TelescopeUnimplementedAPI, ITelescopeV3
    {
        /// <summary>
        /// Logger
        /// </summary>
        private TraceLogger traceLogger;

        /// <summary>
        /// Handles the communication with the arduino
        /// </summary>
        private IDeviceController deviceController;

        /// <summary>
        /// Target position
        /// </summary>
        private double? targetRightAscension;
        private double? targetDeclination;

        /// <summary>
        /// Controller for each axis, talks with the device controller to start / stop movement, keeps track of the position while doing so
        /// </summary>
        private AxisController[] axisControllers;

        public Telescope()
        {
            Init();
        }

        protected sealed override void Init()
        {
            base.Init();
            // Read device configuration from the ASCOM Profile store
            Configuration configuration = Configuration.Instance;
            configuration.ReadProfile();
            traceLogger = configuration.CreateTraceLogger("", "ArduinoST4 Telescope driver");
            traceLogger.LogMessage("Telescope", "Starting initialisation");
            DeviceControllerFactory deviceControllerFactory = new DeviceControllerFactory();
            deviceController = deviceControllerFactory.BuildChoosenDeviceController();

            double compensatedRightAscensionSideralRateMinus = configuration.RightAscensionSideralRateMinus;
            double compensatedRightAscensionSideralRatePlus = configuration.RightAscensionSideralRatePlus;
            if (!configuration.MountCompensatesEarthRotationInSlew)
            {
                // Compensate in software for the earth rotation while slewing
                compensatedRightAscensionSideralRateMinus -= 1;//Sideral Rate -1 for earth rotation
                compensatedRightAscensionSideralRatePlus += 1;//Sideral Rate +1 for earth rotation
            }
            //Setup the axes
            axisControllers = new AxisController[2];
            axisControllers[(int)Axis.RA] = new AxisController(Axis.RA, this.deviceController, Constants.RA_PER_SECOND * compensatedRightAscensionSideralRateMinus, Constants.RA_PER_SECOND * compensatedRightAscensionSideralRatePlus, false);
            axisControllers[(int)Axis.DEC] = new AxisController(Axis.DEC, this.deviceController, Constants.DEGREES_PER_SECOND * configuration.DeclinationSideralRateMinus, Constants.DEGREES_PER_SECOND * configuration.DeclinationSideralRatePlus, configuration.MeridianFlip);
            traceLogger.LogMessage("Telescope", "Initialisation done");
            
        }

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // Only show the setup dialog if not connected
            if (this.Connected)
            {
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");
                return;
            }

            using (SetupDialogForm setupDialogForm = new SetupDialogForm())
            {
                //Read profile before showing UI
                Configuration.Instance.ReadProfile();
                System.Windows.Forms.DialogResult result = setupDialogForm.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // Persist device configuration values to the ASCOM Profile store
                    Configuration.Instance.WriteProfile();
                    // Take in account new values
                    Init();
                }
            }
        }

        public bool CanMoveAxis(TelescopeAxes axis)
        {
            traceLogger.LogMessage("CanMoveAxis", "Get - " + axis.ToString());
            switch (axis)
            {
                case TelescopeAxes.axisPrimary: return true;
                case TelescopeAxes.axisSecondary: return true;
                case TelescopeAxes.axisTertiary: return false;
                default: throw new InvalidValueException("CanMoveAxis", axis.ToString(), "0 to 2");
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                traceLogger.LogMessage("CanPulseGuide", "Get - " + true.ToString());
                return true;
            }
        }



        public bool CanSlew
        {
            get
            {
                traceLogger.LogMessage("CanSlew", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSlewAsync
        {
            get
            {
                traceLogger.LogMessage("CanSlewAsync", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSync
        {
            get
            {
                traceLogger.LogMessage("CanSync", "Get - " + true.ToString());
                return true;
            }
        }
        public AlignmentModes AlignmentMode
        {
            get
            {
                return AlignmentModes.algGermanPolar;
            }
        }

        public EquatorialCoordinateType EquatorialSystem
        {
            get
            {
                EquatorialCoordinateType equatorialSystem = EquatorialCoordinateType.equLocalTopocentric;
                traceLogger.LogMessage("DeclinationRate", "Get - " + equatorialSystem.ToString());
                return equatorialSystem;
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                return this.Moving;
            }
        }

        public void SlewToTarget()
        {
            SlewToCoordinates(TargetRightAscension, TargetDeclination);
        }

        public void SlewToTargetAsync()
        {
            SlewToCoordinatesAsync(TargetRightAscension, TargetDeclination);
        }

        public bool Slewing
        {
            get
            {
                return this.Moving;
            }
        }

        public void SyncToTarget()
        {
            SyncToCoordinates(TargetRightAscension, TargetDeclination);
        }

        public DriveRates TrackingRate
        {
            get
            {
                return DriveRates.driveSidereal;
            }
            set
            {
            }
        }

        public ITrackingRates TrackingRates
        {
            get
            {
                traceLogger.LogMessage("TrackingRates", "Get - ");
                return new TrackingRates();
            }
        }

        public IAxisRates AxisRates(TelescopeAxes axis)
        {
            return new AxisRates(axis);
        }

        public bool Connected
        {
            get
            {
                traceLogger.LogMessage("Connected Get", this.deviceController.Connected.ToString());
                return this.deviceController.Connected;
            }
            set
            {
                traceLogger.LogMessage("Connected Set", value.ToString());
                if (value == this.deviceController.Connected)
                {
                    return;
                }
                if (value)
                {
                    this.deviceController.Connect(Configuration.Instance.ComPort);
                }
                else
                {
                    this.deviceController.Disconnect();
                }
            }
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // Call CommandString and return as soon as it finishes
            this.CommandString(command, raw);
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            return deviceController.CommandBool(command);
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            return deviceController.CommandString(command);
        }

        /// <summary>
        /// Throws an exception if not connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!this.deviceController.Connected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        public void AbortSlew()
        {
            foreach(AxisController axisController in axisControllers)
            {
                axisController.Stop();
            }
        }

        public double Declination
        {
            get
            {
                return axisControllers[(int)Axis.DEC].Position;
            }
        }

        public double GuideRateDeclination
        {
            get
            {
                return axisControllers[(int)Axis.DEC].SlewRate;
            }
            set
            {
                traceLogger.LogMessage("GuideRateDeclination Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", true);
            }
        }

        public double GuideRateRightAscension
        {
            get
            {
                return axisControllers[(int)Axis.RA].SlewRate;
            }
            set
            {
                traceLogger.LogMessage("GuideRateRightAscension Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", true);
            }
        }

        public void MoveAxis(TelescopeAxes telescopeAxis, double rate)
        {
            traceLogger.LogMessage("MoveAxis", "TelescopeAxis - " + telescopeAxis.ToString() + "Rate - " + rate.ToString());
            //Some checks on given values for API conformity
            if (telescopeAxis == TelescopeAxes.axisTertiary)
            {
                throw new ASCOM.InvalidValueException("TelescopeAxis", telescopeAxis.ToString(), "No ternary axis on ST-4");
            }
            IRate axisRate = this.AxisRates(telescopeAxis)[1];
            if (Math.Abs(rate) > axisRate.Maximum || Math.Abs(rate) < axisRate.Minimum)
            {
                throw new ASCOM.InvalidValueException("AxisRate", rate.ToString(), axisRate.Minimum + ".." + axisRate.Maximum);
            }

            Orientation orientation = rate > 0 ? Orientation.PLUS : Orientation.MINUS;
            Axis axis = telescopeAxis == TelescopeAxes.axisPrimary ? Axis.RA : Axis.DEC;
            AxisController axisPositionController = axisControllers[(int)axis];
            if (rate == 0)
            {
                axisPositionController.Stop();
            }
            else
            {
                axisPositionController.Move(orientation);
            }
        }

        public void PulseGuide(GuideDirections direction, int duration)
        {
            traceLogger.LogMessage("PulseGuide", "Direction - " + direction.ToString() + "Duration - " + duration.ToString());
            //Duration in milliseconds
            TimeSpan moveDuration = TimeSpan.FromMilliseconds(duration);
            switch (direction)
            {
                case GuideDirections.guideEast:
                    axisControllers[(int)Axis.RA].Move(moveDuration, Orientation.PLUS);
                    break;
                case GuideDirections.guideWest:
                    axisControllers[(int)Axis.RA].Move(moveDuration, Orientation.MINUS);
                    break;
                case GuideDirections.guideNorth:
                    axisControllers[(int)Axis.DEC].Move(moveDuration, Orientation.PLUS);
                    break;
                case GuideDirections.guideSouth:
                    axisControllers[(int)Axis.DEC].Move(moveDuration, Orientation.MINUS);
                    break;
            }
        }

        public double RightAscension
        {
            get
            {
                double rightAscension = axisControllers[(int)Axis.RA].Position;
                //Ensure that RA is always positive and between 0 and 24
                rightAscension = (rightAscension % 24 + 24) % 24;
                traceLogger.LogMessage("RightAscension", "GET value " + rightAscension);
                return rightAscension;
            }
        }

        public void SlewToCoordinates(double rightAscension, double declination)
        {
            traceLogger.LogMessage("SlewToCoordinates", "RightAscension=" + rightAscension.ToString() + ", Declination=" + declination.ToString());
            SlewToCoordinatesAsync(rightAscension, declination);
            //Wait for all axes to finish
            foreach (AxisController axisController in axisControllers)
            {
                axisController.WaitAsyncMoveEnd();
            }
            traceLogger.LogMessage("SlewToCoordinates", "Slew complete");
        }

        public void SlewToCoordinatesAsync(double rightAscension, double declination)
        {
            traceLogger.LogMessage("SlewToCoordinatesAsync", "RightAscension=" + rightAscension.ToString() + ", Declination=" + declination.ToString() + " (current position ra=" + this.RightAscension + ", dec=" + this.Declination);
            CheckRightAscension(rightAscension);
            CheckDeclination(declination);
            double rightAscensionDelta = rightAscension - this.RightAscension;
            if (rightAscensionDelta < -12)
            {
                //Shortest path from 24 to 0
                rightAscensionDelta += 24;
            }
            else if (rightAscensionDelta > 12)
            {
                //Shortest path from 0 to 24
                rightAscensionDelta -= 24;
            }
            double declinationDelta = declination - this.Declination;
            traceLogger.LogMessage("SlewToCoordinatesAsync", "RightAscension delta =" + rightAscensionDelta.ToString() + ", Declination delta =" + declinationDelta.ToString());
            this.axisControllers[(int)Axis.RA].Move(rightAscensionDelta);
            this.axisControllers[(int)Axis.DEC].Move(declinationDelta);
            traceLogger.LogMessage("SlewToCoordinatesAsync", "Slew commands sent");
        }

        public void SyncToCoordinates(double rightAscension, double declination)
        {
            traceLogger.LogMessage("SyncToCoordinates", "Sync to RA = " + rightAscension + " DEC = " + declination);
            CheckRightAscension(rightAscension);
            CheckDeclination(declination);
            axisControllers[(int)Axis.RA].Position = rightAscension;
            axisControllers[(int)Axis.DEC].Position = declination;
            traceLogger.LogMessage("SyncToCoordinates", "Synced");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public double TargetDeclination
        {
            get
            {
                traceLogger.LogMessage("TargetDeclination GET", "Value = " + this.targetDeclination);
                if (this.targetDeclination == null)
                {
                    traceLogger.LogMessage("TargetDeclination GET", "Value is null, throwing exception");
                    throw new ASCOM.ValueNotSetException("TargetDeclination");
                }
                return (double)this.targetDeclination;
            }
            set
            {
                traceLogger.LogMessage("TargetDeclination SET", "Value = " + value);
                CheckDeclination(value);
                this.targetDeclination = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public double TargetRightAscension
        {
            get
            {
                traceLogger.LogMessage("TargetRightAscension GET", "Value = " + this.targetRightAscension);
                if (this.targetRightAscension == null)
                {
                    traceLogger.LogMessage("TargetRightAscension GET", "Value is null, throwing exception");
                    throw new ASCOM.ValueNotSetException("TargetRightAscension");
                }
                return (double)this.targetRightAscension;
            }
            set
            {
                traceLogger.LogMessage("TargetRightAscension SET", "Value = " + value);
                CheckRightAscension(value);
                this.targetRightAscension = value;
            }
        }
        
        private void CheckDeclination(double value)
        {
            if (value < -90 || value > 90)
            {
                traceLogger.LogMessage("TargetDeclination SET", "Value is not in range -90..90, throwing exception");
                throw new ASCOM.InvalidValueException("TargetDeclination", value.ToString(), "-90..90");
            }
        }

        private void CheckRightAscension(double value)
        {
            if (value < 0 || value >= 24)
            {
                traceLogger.LogMessage("TargetRightAscension SET", "Value is not in range 0..24, throwing exception");
                throw new InvalidValueException("TargetRightAscension", value.ToString(), "0..24");
            }
        }

        public double DeclinationRate
        {
            get
            {
                double declination = axisControllers[(int)Axis.DEC].SlewRate;
                traceLogger.LogMessage("DeclinationRate", "Get - " + declination.ToString());
                return declination;
            }
            set
            {
                traceLogger.LogMessage("DeclinationRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DeclinationRate", true);
            }
        }

        public double RightAscensionRate
        {
            get
            {
                double rightAscensionRate = axisControllers[(int)Axis.RA].SlewRate;
                traceLogger.LogMessage("RightAscensionRate", "Get - " + rightAscensionRate.ToString());
                return rightAscensionRate;
            }
            set
            {
                traceLogger.LogMessage("RightAscensionRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("RightAscensionRate", true);
            }
        }

        public bool Tracking
        {
            get
            {
                traceLogger.LogMessage("Tracking GET", "Value = true");
                //EQ5 is always tracking when motors are on
                return true;
            }
            set
            {
                traceLogger.LogMessage("Tracking Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Tracking", false);
            }
        }

        protected Boolean Moving
        {
            get
            {
                bool moving = axisControllers[(int)Axis.RA].Moving || axisControllers[(int)Axis.DEC].Moving;
                traceLogger.LogMessage("Moving GET", "Value = " + moving);
                return moving;
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.Connected = false;
            foreach (AxisController axisController in this.axisControllers)
            {
                axisController.Dispose();
            }
            traceLogger.Dispose();
            base.Dispose(disposing);
        }
    }
}

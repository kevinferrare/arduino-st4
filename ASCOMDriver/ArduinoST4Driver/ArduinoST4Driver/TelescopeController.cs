using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASCOM.ArduinoST4
{
    class TelescopeController
    {
        /// <summary>
        /// Logger
        /// </summary>
        private TraceLogger traceLogger;

        /// <summary>
        /// Handles the communication with the arduino
        /// </summary>
        private DeviceController deviceController;

        /// <summary>
        /// Target position
        /// </summary>
        private double? targetRightAscension;
        private double? targetDeclination;

        /// <summary>
        /// Controller for each axis, talks with the device controller to start / stop movement, keeps track of the position while doing so
        /// </summary>
        private AxisController[] axisControllers;

        private Configuration configuration;

        public TelescopeController(Configuration configuration)
        {
            this.configuration = configuration;
            traceLogger = new TraceLogger("", "ArduinoST4 TelescopeController");
            traceLogger.Enabled = configuration.TraceState;
            traceLogger.LogMessage("Telescope", "Starting initialisation");
            deviceController = new DeviceController();

            double compensatedRightAscensionSideralRateMinus = configuration.RightAscensionSideralRateMinus;
            double compensatedRightAscensionSideralRatePlus = configuration.RightAscensionSideralRatePlus;
            if (!configuration.MountCompensatesEarthRotationInSlew)
            {
                //Compensates in software for the earth rotation while slewing
                compensatedRightAscensionSideralRateMinus -= 1;//Sideral Rate -1 for earth rotation
                compensatedRightAscensionSideralRatePlus += 1;//Sideral Rate +1 for earth rotation
            }
            //Setup the axes
            axisControllers = new AxisController[2];
            axisControllers[(int)Axis.RA] = new AxisController(Axis.RA, this.deviceController, Constants.RA_PER_SECOND * compensatedRightAscensionSideralRateMinus, Constants.RA_PER_SECOND * compensatedRightAscensionSideralRatePlus, false);
            axisControllers[(int)Axis.DEC] = new AxisController(Axis.DEC, this.deviceController, Constants.DEGREES_PER_SECOND * configuration.DeclinationSideralRateMinus, Constants.DEGREES_PER_SECOND * configuration.DeclinationSideralRatePlus, configuration.MeridianFlip);
            traceLogger.LogMessage("Telescope", "Initialisation done");
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
                    this.deviceController.Connect(configuration.ComPort);
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
            axisControllers[(int)Axis.RA].Stop();
            axisControllers[(int)Axis.DEC].Stop();
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
        }

        public double GuideRateRightAscension
        {
            get
            {
                return axisControllers[(int)Axis.RA].SlewRate;
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
                rightAscension = (rightAscension % 24 + 24) % 24;//Ensure that RA is always positive and between 0 and 24
                return rightAscension;
            }
        }

        public void SlewToCoordinates(double rightAscension, double declination)
        {
            traceLogger.LogMessage("SlewToCoordinatesAsync", "RightAscension=" + rightAscension.ToString() + ", Declination=" + declination.ToString());
            SlewToCoordinatesAsync(rightAscension, declination);
            //Wait for both axes to finish
            this.axisControllers[(int)Axis.RA].WaitAsyncMoveEnd();
            this.axisControllers[(int)Axis.DEC].WaitAsyncMoveEnd();
        }

        public void SlewToCoordinatesAsync(double rightAscension, double declination)
        {
            traceLogger.LogMessage("SlewToCoordinatesAsync", "RightAscension=" + rightAscension.ToString() + ", Declination=" + declination.ToString() + " (current position ra=" + this.RightAscension + ", dec=" + this.Declination);
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
            traceLogger.LogMessage("SlewToCoordinatesAsync", "RightAscension delta =" + rightAscensionDelta.ToString());
            double declinationDelta = declination - this.Declination;
            this.axisControllers[(int)Axis.RA].Move(rightAscensionDelta);
            this.axisControllers[(int)Axis.DEC].Move(declinationDelta);
        }

        public void SyncToCoordinates(double rightAscension, double declination)
        {
            axisControllers[(int)Axis.RA].Position = rightAscension;
            axisControllers[(int)Axis.DEC].Position = declination;
        }

        public double TargetDeclination
        {
            get
            {
                if (this.targetDeclination == null)
                {
                    throw new ASCOM.ValueNotSetException("TargetDeclination");
                }
                return (double)this.targetDeclination;
            }
            set
            {
                if (value < -90 || value > 90)
                {
                    throw new ASCOM.InvalidValueException("TargetDeclination", value.ToString(), "-90..90");
                }
                this.targetDeclination = value;
            }
        }

        public double TargetRightAscension
        {
            get
            {
                if (this.targetRightAscension == null)
                {
                    throw new ASCOM.ValueNotSetException("TargetRightAscension");
                }
                return (double)this.targetRightAscension;
            }
            set
            {
                if (value < 0 || value > 24)
                {
                    throw new InvalidValueException("TargetRightAscension", value.ToString(), "0..24");
                }
                this.targetRightAscension = value;
            }
        }

        public bool Tracking
        {
            get
            {
                //EQ5 is always tracking when motors are on
                return !this.Moving;
            }
        }

        public Boolean Moving
        {
            get
            {
                return axisControllers[(int)Axis.RA].Moving || axisControllers[(int)Axis.DEC].Moving;
            }
        }

        public void Dispose()
        {
            this.Connected = false;
            foreach (AxisController axisController in this.axisControllers)
            {
                axisController.Dispose();
            }
            this.traceLogger.Enabled = false;
            this.traceLogger.Dispose();
            this.traceLogger = null;
        }
    }
}

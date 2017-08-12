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

using ASCOM.Utilities;
using System;

namespace ASCOM.ArduinoST4
{
    public class Configuration : IDisposable
    {
        /// <summary>
        /// Global instance of the configuration, accessible by all
        /// </summary>
        public static Configuration Instance{ get;set; }
        /// <summary>
        /// Right ascension and declination speed of the device in sideral multiple (earth rotation multiple)
        /// </summary>
        public double RightAscensionSideralRatePlus { get; set; }
        public double RightAscensionSideralRateMinus { get; set; }
        public double DeclinationSideralRatePlus { get; set; }
        public double DeclinationSideralRateMinus { get; set; }
        public bool MountCompensatesEarthRotationInSlew { get; set; }
        public bool MeridianFlip { get; set; }
        /// <summary>
        /// COM port for arduino access
        /// </summary>
        public string ComPort { get; set; }

        /// <summary>
        /// Enable logging
        /// </summary>
        public bool TraceState { get; set; }

        /// <summary>
        /// Device type
        /// </summary>
        public string Device { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        private TraceLogger traceLogger;

        private String driverId;

        public Configuration(String driverId, Boolean traceState)
        {
            this.RightAscensionSideralRatePlus = 8;
            this.RightAscensionSideralRateMinus = 8;
            this.DeclinationSideralRatePlus = 8;
            this.DeclinationSideralRateMinus = 8;
            this.MountCompensatesEarthRotationInSlew = false;
            this.MeridianFlip = false;
            this.ComPort = "";
            this.TraceState = traceState;
            this.driverId = driverId;
            this.Device = DeviceType.ARDUINO.ToString();
            this.traceLogger = CreateTraceLogger("", "ArduinoST4 Configuration");
        }

        public TraceLogger CreateTraceLogger(String logFileName, String logFileType)
        {
            TraceLogger res = new TraceLogger(logFileName, logFileType);
            res.Enabled = TraceState;
            return res;
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        public void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                driverProfile.WriteValue(driverId, "traceState", TraceState.ToString());
                driverProfile.WriteValue(driverId, "comPort", ComPort.ToString());
                driverProfile.WriteValue(driverId, "deviceType", Device.ToString());
                driverProfile.WriteValue(driverId, "rightAscensionSideralRatePlus2", RightAscensionSideralRatePlus.ToString());
                driverProfile.WriteValue(driverId, "rightAscensionSideralRateMinus2", RightAscensionSideralRateMinus.ToString());
                driverProfile.WriteValue(driverId, "declinationSideralRatePlus", DeclinationSideralRatePlus.ToString());
                driverProfile.WriteValue(driverId, "declinationSideralRateMinus", DeclinationSideralRateMinus.ToString());
                driverProfile.WriteValue(driverId, "mountCompensatesEarthRotationInSlew", MountCompensatesEarthRotationInSlew.ToString());
                driverProfile.WriteValue(driverId, "meridianFlip", MeridianFlip.ToString());
            }
            traceLogger.LogMessage("WriteProfile", "Wrote profile");
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        public void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                ReadFieldsFromProfile(driverProfile);
                // Setup logger accordingly
                traceLogger.Enabled = TraceState;
            }
            traceLogger.LogMessage("ReadProfile", "Read profile");
        }

        private void ReadFieldsFromProfile(Profile driverProfile)
        {
            TraceState = ReadBoolFromProfile(driverProfile, "traceState", TraceState);
            ComPort = ReadStringFromProfile(driverProfile, "comPort", ComPort);
            Device = ReadStringFromProfile(driverProfile, "deviceType", Device);
            RightAscensionSideralRatePlus = ReadDoubleFromProfile(driverProfile, "rightAscensionSideralRatePlus2", RightAscensionSideralRatePlus);
            RightAscensionSideralRateMinus = ReadDoubleFromProfile(driverProfile, "rightAscensionSideralRateMinus2", RightAscensionSideralRateMinus);
            DeclinationSideralRatePlus = ReadDoubleFromProfile(driverProfile, "declinationSideralRatePlus", DeclinationSideralRatePlus);
            DeclinationSideralRateMinus = ReadDoubleFromProfile(driverProfile, "declinationSideralRateMinus", DeclinationSideralRateMinus);
            MountCompensatesEarthRotationInSlew = ReadBoolFromProfile(driverProfile, "mountCompensatesEarthRotationInSlew", MountCompensatesEarthRotationInSlew);
            MeridianFlip = ReadBoolFromProfile(driverProfile, "meridianFlip", MeridianFlip);
        }

        private bool ReadBoolFromProfile(Profile driverProfile, String profileName, bool defaultValue)
        {
            return Convert.ToBoolean(ReadStringFromProfile(driverProfile, profileName, Convert.ToString(defaultValue)));
        }

        private double ReadDoubleFromProfile(Profile driverProfile, String profileName, double defaultValue)
        {
            return Convert.ToDouble(ReadStringFromProfile(driverProfile, profileName, Convert.ToString(defaultValue)));
        }

        private String ReadStringFromProfile(Profile driverProfile, String profileName, String defaultValue)
        {
            return driverProfile.GetValue(driverId, profileName, string.Empty, defaultValue);
        }

        protected virtual void Dispose(bool disposing)
        {
            traceLogger.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
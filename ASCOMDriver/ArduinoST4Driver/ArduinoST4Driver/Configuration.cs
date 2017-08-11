using ASCOM.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASCOM.ArduinoST4
{
    public class Configuration : IDisposable
    {
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
        /// Logger
        /// </summary>
        private TraceLogger traceLogger;

        private String driverId;

        public Configuration(String driverId, Boolean traceState)
        {
            RightAscensionSideralRatePlus = 8;
            RightAscensionSideralRateMinus = 8;
            DeclinationSideralRatePlus = 8;
            DeclinationSideralRateMinus = 8;
            MountCompensatesEarthRotationInSlew = false;
            MeridianFlip = false;
            ComPort = "";
            TraceState = traceState;
            this.driverId = driverId;
            traceLogger = new TraceLogger("", "ArduinoST4 Configuration");
            traceLogger.Enabled = traceState;
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
            }
            traceLogger.LogMessage("ReadProfile", "Read profile");
        }

        private void ReadFieldsFromProfile(Profile driverProfile)
        {
            TraceState = ReadBoolFromProfile(driverProfile, "traceState", TraceState);
            ComPort = ReadStringFromProfile(driverProfile, "comPort", ComPort);
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

        public void Dispose()
        {
            traceLogger.Dispose();
        }
    }
}
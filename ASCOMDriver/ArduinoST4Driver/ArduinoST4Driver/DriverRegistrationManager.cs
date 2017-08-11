using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASCOM.ArduinoST4
{
    class DriverRegistrationManager
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        public string DriverId { get; }

        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        public string DriverDescription { get;}

        public DriverRegistrationManager()
        {
            DriverId = "ASCOM.ArduinoST4.Telescope";
            DriverDescription = "ArduinoST4 telescope driver";
        }

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="register">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        public void RegUnregASCOM(bool register)
        {
            using (var profile = new ASCOM.Utilities.Profile())
            {
                profile.DeviceType = "Telescope";
                if (register)
                {
                    profile.Register(DriverId, DriverDescription);
                }
                else
                {
                    profile.Unregister(DriverId);
                }
            }
        }
    }
}

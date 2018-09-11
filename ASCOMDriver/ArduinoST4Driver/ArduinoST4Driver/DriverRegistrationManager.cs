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

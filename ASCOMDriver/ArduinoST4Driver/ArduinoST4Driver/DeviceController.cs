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
    /// <summary>
    /// Hardware communication is implemented here.
    /// </summary>
    /// Author:  Kevin Ferrare
    interface DeviceController
    {

        /// <summary>
        /// Connect to the arduino with the given com port.
        /// </summary>
        /// <param name="comPort"></param>
        void Connect(String comPort);

        /// <summary>
        /// Disconnect from the arduino
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Return true when connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Send the given command to the device.
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <returns>true if it has been understood correctly</returns>
        bool CommandBool(string command);

        /// <summary>
        /// Send the given command to the device.
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <returns>Response returned by the device</returns>
        string CommandString(string command);

        /// <summary>
        /// Tell the hardware to start / stop moving on the given axis.
        /// If the given orientation is null, the movement will stop.
        /// </summary>
        /// <param name="axis">Axis to move</param>
        /// <param name="orientation">Orientation along the axis</param>
        void Move(Axis axis, Orientation? orientation);
    }
}

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
    /// Dummy device controller for testing without the hardware connected
    /// </summary>
    /// Author:  Kevin Ferrare
    class DeviceControllerDummy : IDeviceController
    {
        private TraceLogger traceLogger;

        private bool connected;

        public DeviceControllerDummy()
        {
            traceLogger = Configuration.Instance.CreateTraceLogger("", "Dummy DeviceController");
            connected = false;
        }
        public void Connect(String comPort) {
            traceLogger.LogMessage("Connected Set", "Connecting to port " + comPort);
            connected = true;
        }

        public void Disconnect() {
            connected = false;
            traceLogger.LogMessage("Connected Set", "Disconnecting");
        }

        public bool Connected {
            get {
                traceLogger.LogMessage("Connected Get", connected.ToString());
                return connected;
            }
        }

        public bool CommandBool(string command) {
            traceLogger.LogMessage("CommandBool", "Sending command " + command);
            return true;
        }

        public string CommandString(string command) {
            traceLogger.LogMessage("CommandString", "Sending command " + command);
            return "";
        }

        public void Move(Axis axis, Orientation? orientation) {
            traceLogger.LogMessage("Move", "Axis " + axis + " Orientation " + orientation);
        }
    }
}

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
    class DeviceControllerArduino : IDeviceController, IDisposable
    {
        private TraceLogger traceLogger;

        /// <summary>
        /// Serial connection to the device
        /// </summary>
        private Serial serialConnection;

        /// <summary>
        /// Whether the device is connected or not
        /// </summary>
        private Boolean connected;

        public DeviceControllerArduino()
        {
            traceLogger = Configuration.Instance.CreateTraceLogger("", "Arduino DeviceController");
            connected = false;
        }

        /// <summary>
        /// Connect to the arduino with the given com port.
        /// </summary>
        /// <param name="comPort"></param>
        public void Connect(String comPort)
        {
            traceLogger.LogMessage("Connected Set", "Connecting to port " + comPort);
            serialConnection = new Serial();
            serialConnection.PortName = comPort;
            //57.6k
            serialConnection.Speed = SerialSpeed.ps57600;
            //No parity
            serialConnection.Parity = SerialParity.None;
            //Stop bit one
            serialConnection.StopBits = SerialStopBits.One;
            serialConnection.DataBits = 8;
            //Timeout, initial message can take up to 5 seconds while the arduino initializes
            serialConnection.ReceiveTimeout = 5;
            serialConnection.Connected = true;

            //The arduino will send "INITIALIZED" by itself once it is ready (can take several seconds)
            String initialMessage = ReadResponse();
            //Reset device and light up the LED
            this.connected = CommandBool("CONNECT");
            if (!this.connected)
            {
                //close serial connection when it failed
                serialConnection.Connected = false;
                serialConnection = null;
            }
        }

        /// <summary>
        /// Disconnect from the arduino
        /// </summary>
        public void Disconnect()
        {
            if (serialConnection == null)
            {
                return;
            }
            //Tell bye-bye to the device
            CommandBool("DISCONNECT");
            this.connected = false;
            traceLogger.LogMessage("Connected Set", "Disconnecting");
            serialConnection.Connected = false;
        }

        /// <summary>
        /// Return true when connected
        /// </summary>
        public bool Connected
        {
            get
            {
                traceLogger.LogMessage("Connected Get", connected.ToString());
                return this.connected;
            }
        }

        /// <summary>
        /// Send the given command to the device.
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <returns>true if it has been understood correctly</returns>
        public bool CommandBool(string command)
        {
            string ret = CommandString(command);
            //Successful commands should return OK
            return "OK".Equals(ret);
        }

        /// <summary>
        /// Send the given command to the device.
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <returns>Response returned by the device</returns>
        public string CommandString(string command)
        {
            if (!this.Connected)
            {
                return null;
            }
            traceLogger.LogMessage("CommandString", "Sending command " + command);
            //All commands from and to the arduino ends with #
            serialConnection.Transmit(command + "#");
            return ReadResponse();
        }

        /// <summary>
        /// Read a response from the arduino and returns it
        /// </summary>
        private String ReadResponse()
        {
            traceLogger.LogMessage("ReadResponse", "Reading response");
            String response = serialConnection.ReceiveTerminated("#");
            response = response.Replace("#", "").Replace("\r", "").Replace("\n", "");
            traceLogger.LogMessage("ReadResponse", "Received response " + response);
            return response;
        }

        /// <summary>
        /// Tell the hardware to start / stop moving on the given axis.
        /// If the given orientation is null, the movement will stop.
        /// </summary>
        /// <param name="axis">Axis to move</param>
        /// <param name="orientation">Orientation along the axis</param>
        public void Move(Axis axis, Orientation? orientation)
        {
            //Do nothing if not connected
            if (!this.Connected)
            {
                return;
            }
            traceLogger.LogMessage("Move", "Axis " + axis + " Orientation " + orientation);
            String axisName = axis.ToString();
            if (orientation == null)
            {
                //Stops the movement for the axis
                this.CommandBool(axisName + "0");
            }
            else
            {
                String sign = (Orientation)orientation == Orientation.PLUS ? "+" : "-";
                this.CommandBool(axisName + sign);
            }
        }

        public void Dispose()
        {
            traceLogger.Enabled = false;
            traceLogger.Dispose();
            traceLogger = null;
            serialConnection.Dispose();
        }
    }
}

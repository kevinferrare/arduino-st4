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
    enum DeviceType
    {
        DUMMY,
        ARDUINO
    }
    class DeviceControllerFactory
    {
        private TraceLogger traceLogger;

        public DeviceControllerFactory()
        {
            traceLogger = Configuration.Instance.CreateTraceLogger("", "DeviceControllerFactory");
        }

        public IDeviceController BuildChoosenDeviceController()
        {
            DeviceType deviceType = ParseDeviceType(Configuration.Instance.Device);
            traceLogger.LogMessage("BuildChoosenDeviceController", "Device type is  " + deviceType);
            switch (deviceType)
            {
                case DeviceType.DUMMY:
                    return new DeviceControllerDummy();
                case DeviceType.ARDUINO:
                    return new DeviceControllerArduino();
            }
            return null;
        }

        private DeviceType ParseDeviceType(string deviceType)
        {
            try
            {
                return (DeviceType)Enum.Parse(typeof(DeviceType), deviceType, true);
            }
            catch (Exception)
            {
                traceLogger.LogMessage("ParseDeviceType", "Could not read device type " + deviceType + "Defaulting to " + DeviceType.ARDUINO);
                return DeviceType.ARDUINO;
            }
        }
    }
}

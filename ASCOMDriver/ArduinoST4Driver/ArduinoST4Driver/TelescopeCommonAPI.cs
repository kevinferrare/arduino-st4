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
using System.Globalization;
using System.Runtime.InteropServices;

namespace ASCOM.ArduinoST4
{
    public abstract class TelescopeCommonAPI : IDisposable
    {
        private static DriverRegistrationManager driverRegistrationManager = new DriverRegistrationManager();

        static TelescopeCommonAPI()
        {
            Configuration.Instance = new Configuration(driverRegistrationManager.DriverId, false);
        }

        /// <summary>
        /// Logger
        /// </summary>
        private TraceLogger traceLogger;

        /// <summary>
        /// ASCOM Utilities object, used only to calculate dates
        /// </summary>
        private Util utilities = new Util();

        protected virtual void Init()
        {
            // Make an initial read
            Configuration.Instance.ReadProfile();
            traceLogger = Configuration.Instance.CreateTraceLogger("", "TelescopeCommonAPI");
        }

        public string Description
        {
            get
            {
                traceLogger.LogMessage("Description Get", driverRegistrationManager.DriverDescription);
                return driverRegistrationManager.DriverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                string driverInfo = "Information about the driver itself. Version: " + DriverVersion;
                traceLogger.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                traceLogger.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                traceLogger.LogMessage("InterfaceVersion Get", "3");
                return 3;
            }
        }

        public string Name
        {
            get
            {
                string name = "ArduinoST4";
                traceLogger.LogMessage("Name Get", name);
                return name;
            }
        }
        public double SiderealTime
        {
            get
            {
                double siderealTime = (18.697374558 + 24.065709824419081 * (utilities.DateLocalToJulian(DateTime.Now) - 2451545.0)) % 24.0;
                traceLogger.LogMessage("SiderealTime", "Get - " + siderealTime.ToString());
                return siderealTime;
            }
        }
        public DateTime UTCDate
        {
            get
            {
                return DateTime.UtcNow;
            }
            set
            {
            }
        }
        #region ASCOM Registration

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            driverRegistrationManager.RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            driverRegistrationManager.RegUnregASCOM(false);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            traceLogger.Dispose();
            utilities.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

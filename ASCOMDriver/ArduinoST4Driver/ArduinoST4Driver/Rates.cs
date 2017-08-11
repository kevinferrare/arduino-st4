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

using ASCOM.DeviceInterface;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace ASCOM.ArduinoST4
{
    #region Rate class
    //
    // The Rate class implements IRate, and is used to hold values
    // for AxisRates. You do not need to change this class.
    //
    // The Guid attribute sets the CLSID for ASCOM.ArduinoST4.Rate
    // The ClassInterface/None addribute prevents an empty interface called
    // _Rate from being created and used as the [default] interface
    //
    [Guid("a60ffd60-3725-4457-812e-49ace8613a81")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class Rate : ASCOM.DeviceInterface.IRate
    {
        private double maximum = 0;
        private double minimum = 0;

        internal Rate(double minimum, double maximum)
        {
            this.maximum = maximum;
            this.minimum = minimum;
        }

        #region Implementation of IRate

        public void Dispose()
        {
            throw new ASCOM.MethodNotImplementedException("Dispose");
        }

        public double Maximum
        {
            get { return this.maximum; }
            set { this.maximum = value; }
        }

        public double Minimum
        {
            get { return this.minimum; }
            set { this.minimum = value; }
        }

        #endregion
    }
    #endregion

    #region AxisRates
    //
    // AxisRates is a strongly-typed collection that must be enumerable by
    // both COM and .NET. The IAxisRates and IEnumerable interfaces provide
    // this polymorphism. 
    //
    // The Guid attribute sets the CLSID for ASCOM.ArduinoST4.AxisRates
    // The ClassInterface/None addribute prevents an empty interface called
    // _AxisRates from being created and used as the [default] interface
    //
    [Guid("1f302d16-dd2d-49a4-af1a-992cf9599bc4")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class AxisRates : IAxisRates, IEnumerable
    {
        private readonly Rate[] rates;

        //
        // Constructor - Internal prevents public creation
        // of instances. Returned by Telescope.AxisRates.
        //
        internal AxisRates(TelescopeAxes axis)
        {
            switch (axis)
            {
                case TelescopeAxes.axisPrimary:
                    this.rates = new Rate[] { new Rate(0, Constants.DEGREES_PER_SECOND * 8) };//8X Sideral Rate
                    break;
                case TelescopeAxes.axisSecondary:
                    this.rates = new Rate[] { new Rate(0, Constants.DEGREES_PER_SECOND * 8) };//8X Sideral Rate
                    break;
                case TelescopeAxes.axisTertiary:
                    this.rates = new Rate[] { new Rate(0, Constants.DEGREES_PER_SECOND * 8) };//8X Sideral Rate
                    break;
            }
        }

        #region IAxisRates Members

        public int Count
        {
            get { return this.rates.Length; }
        }

        public void Dispose()
        {
            throw new ASCOM.MethodNotImplementedException("Dispose");
        }

        public IEnumerator GetEnumerator()
        {
            return rates.GetEnumerator();
        }

        public IRate this[int index]
        {
            get { return this.rates[index - 1]; }	// 1-based
        }

        #endregion
    }
    #endregion

    #region TrackingRates
    //
    // TrackingRates is a strongly-typed collection that must be enumerable by
    // both COM and .NET. The ITrackingRates and IEnumerable interfaces provide
    // this polymorphism. 
    //
    // The Guid attribute sets the CLSID for ASCOM.ArduinoST4.TrackingRates
    // The ClassInterface/None addribute prevents an empty interface called
    // _TrackingRates from being created and used as the [default] interface
    //
    [Guid("a544289a-d48d-44fc-8492-d911fcd6d3ef")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class TrackingRates : ITrackingRates, IEnumerable, IEnumerator
    {
        private readonly DriveRates[] trackingRates;
        private static int pos = -1;

        //
        // Default constructor - Internal prevents public creation
        // of instances. Returned by Telescope.AxisRates.
        //
        internal TrackingRates()
        {
            //
            // This array must hold ONE or more DriveRates values, indicating
            // the tracking rates supported by your telescope. The one value
            // (tracking rate) that MUST be supported is driveSidereal!
            //
            this.trackingRates = new[] { DriveRates.driveSidereal };
            // TODO Initialize this array with any additional tracking rates that your driver may provide
        }

        #region ITrackingRates Members

        public int Count
        {
            get { return this.trackingRates.Length; }
        }

        public IEnumerator GetEnumerator()
        {
            pos = -1;
            return this as IEnumerator;
        }

        public void Dispose()
        {
            throw new ASCOM.MethodNotImplementedException("Dispose");
        }

        public DriveRates this[int index]
        {
            get { return this.trackingRates[index - 1]; }	// 1-based
        }

        #endregion

        #region IEnumerable members

        public object Current
        {
            get
            {
                if (pos < 0 || pos >= trackingRates.Length)
                {
                    throw new System.InvalidOperationException();
                }
                return trackingRates[pos];
            }
        }

        public bool MoveNext()
        {
            if (++pos >= trackingRates.Length)
            {
                return false;
            }
            return true;
        }

        public void Reset()
        {
            pos = -1;
        }
        #endregion
    }
    #endregion
}

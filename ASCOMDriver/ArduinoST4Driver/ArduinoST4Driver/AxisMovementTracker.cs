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

using System;

namespace ASCOM.ArduinoST4
{
    /// <summary>
    /// Keeps track of the position of an axis.
    /// 
    /// Updates the position from the slew rate and the movement start / stop time
    /// </summary>
    /// Author:  Kevin Ferrare
    class AxisMovementTracker
    {

        /// <summary>
        /// Start position before slewing started. Is updated when calling Stop().
        /// </summary>
        private double position = 0;

        /// <summary>
        /// Slew rate for the axis
        /// </summary>
        private double slewRate;

        /// <summary>
        /// Time at which the slewing started, used to calculate the current position
        /// </summary>
        private DateTime slewStartTime;

        public AxisMovementTracker()
        {
        }

        /// <summary>
        /// Calculates the position delta from time represented by the slewStartTime property until now.
        /// </summary>
        /// <returns></returns>
        private double CalculateCurrentPositionDelta()
        {
            TimeSpan slewTime = DateTime.Now - this.slewStartTime;
            return slewRate * slewTime.TotalSeconds;
        }

        /// <summary>
        /// Position of the axis. The value returned is calculated when the axis is moving.
        /// </summary>
        public double Position
        {
            get
            {
                //If moving, returns calculated position
                if (this.SlewRate != 0)
                {
                    return this.position + this.CalculateCurrentPositionDelta();
                }
                else
                {
                    return this.position;
                }

            }
            set
            {
                this.position = value;
            }
        }

        /// <summary>
        /// Rate at which the axis is moving
        /// </summary>
        public double SlewRate
        {
            get
            {
                return this.slewRate;
            }
        }

        /// <summary>
        /// Starts keeping track of the movement with the given slew rate
        /// </summary>
        /// <param name="slewRate">Rate at which the axis is moving</param>
        public void Start(double slewRate)
        {
            this.slewRate = slewRate;
            this.slewStartTime = DateTime.Now;
        }

        /// <summary>
        /// Stops keeping track of the movement
        /// </summary>
        public void Stop()
        {
            this.position += CalculateCurrentPositionDelta();
            this.slewRate = 0;
        }
    }
}

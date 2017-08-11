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
using System.Threading;

namespace ASCOM.ArduinoST4
{
    /// <summary>
    /// Controls an axis.
    /// 
    /// Responsible for telling the hardware to start and stop slewing.
    /// Keeps trace of the position while doing so.
    /// 
    /// Can move the axis for a given amount of time or until a position is reached.
    /// </summary>
    /// Author:  Kevin Ferrare
    class AxisController : IDisposable
    {
        private TraceLogger traceLogger;

        /// <summary>
        /// Axis controlled by this object
        /// </summary>
        private readonly Axis axis;

        /// <summary>
        /// Helper to keep track of the position along the axis depending on the slew time
        /// </summary>
        private readonly AxisMovementTracker axisMovementTracker;

        /// <summary>
        /// Actual hardware controller
        /// </summary>
        private readonly DeviceController deviceController;

        /// <summary>
        /// Thread that will wait for axis to finish slewing asynchronously
        /// </summary>
        private Thread waitAndStopAsyncThread;

        /// <summary>
        /// Slew rate to use when moving in the MINUS orientation
        /// </summary>
        private readonly double minusSlewRate;

        /// <summary>
        /// Slew rate to use when moving in the PLUS orientation
        /// </summary>
        private readonly double plusSlewRate;

        /// <summary>
        /// Whether to invert the orientation of commands to give to the mount
        /// </summary>
        private readonly bool invertedHardware;

        /// <summary>
        /// Build an AxisPositionController object.
        /// 
        /// </summary>
        /// <param name="axis">Axis controlled by this object</param>
        /// <param name="deviceController">Actual hardware controller</param>
        /// <param name="minusSlewRate">Slew rate to use when moving in the MINUS orientation</param>
        /// <param name="plusSlewRate">Slew rate to use when moving in the PLUS orientation</param>
        /// <param name="inverted">Whether to invert the orientation of commands to give to the mount</param>
        public AxisController(Axis axis, DeviceController deviceController, double minusSlewRate, double plusSlewRate, bool invertedHardware)
        {
            this.traceLogger = new TraceLogger("", "ArduinoST4 AxisPositionController " + axis.ToString());
            this.traceLogger.Enabled = true;
            this.axis = axis;
            this.deviceController = deviceController;
            this.minusSlewRate = minusSlewRate;
            this.plusSlewRate = plusSlewRate;
            this.invertedHardware = invertedHardware;
            this.axisMovementTracker = new AxisMovementTracker();
        }

        /// <summary>
        /// Return the slew rate corresponding to the given orientation
        /// </summary>
        /// <param name="slewOrientation">Orientation for which the slew rate is requested</param>
        /// <returns></returns>
        private double CalculateSlewRate(Orientation slewOrientation)
        {
            return slewOrientation == Orientation.PLUS ? this.plusSlewRate : this.minusSlewRate;
        }

        /// <summary>
        /// Calculates the number (-1 / +1) by which to multiply the slew rate in order to get a slew speed
        /// <param name="orientation">Orientation to move the axis (+ or -)</param>
        /// </summary>
        private double CalculateSlewSign(Orientation slewOrientation)
        {
            return slewOrientation == Orientation.PLUS ? 1 : -1;
        }

        /// <summary>
        /// Return an orientation from the given positive or negative slew rate.
        /// </summary>
        /// <param name="slewRate">Slew rate, eithe positive or negative</param>
        /// <returns></returns>
        private Orientation CalculateOrientation(double slewRate)
        {
            return slewRate > 0 ? Orientation.PLUS : Orientation.MINUS;
        }

        /// <summary>
        /// Calculates the mount actual orientation taking in account potential axis inversion
        /// <param name="orientation">Orientation to move the axis (+ or -)</param>
        /// </summary>
        private Orientation CalculateMountOrientation(Orientation orientation)
        {
            return orientation.Invert(this.invertedHardware);
        }

        /// <summary>
        /// Whether the axis is currently moving or not
        /// </summary>
        public Boolean Moving
        {
            get
            {
                return SlewRate != 0;
            }
        }

        /// <summary>
        /// Current position of the axis
        /// </summary>
        public double Position
        {
            get
            {
                return this.axisMovementTracker.Position;
            }
            set
            {
                this.axisMovementTracker.Position = value;
                this.Log("Position Set", "New position is  " + value);
            }
        }

        /// <summary>
        /// Current rate at which the axis is slewing, can be 0 if not moving
        /// </summary>
        public double SlewRate
        {
            get
            {
                return this.axisMovementTracker.SlewRate;
            }
        }

        /// <summary>
        /// Move the axis for the given time with the given orientation.
        /// If provided time is null or async is true, will not wait for the move to complete.
        /// 
        /// <param name="moveDuration">Duration of the move, moves indefinitely if null</param>
        /// <param name="orientation">Orientation to move the axis (+ or -)</param>
        /// <param name="asynchronous">If true, will return without waiting for the move to complete </param>
        /// </summary>
        protected void Move(TimeSpan? moveDuration, Orientation orientation, Boolean asynchronous)
        {
            this.Log("Move", "duration=" + (moveDuration == null ? "null" : moveDuration.ToString()) + ", orientation=" + orientation + ", asynchronous=" + asynchronous);
            //Stop any async move
            this.Log("Move", "Stopping any move");
            this.Stop();
            if (moveDuration != null && ((TimeSpan)moveDuration).TotalMilliseconds == 0)
            {
                this.Log("Move", "Nothing to do :)");
                //Nothing to do :)
                return;
            }
            double hardwareSlewRate = MoveHardwareAndCalculateSlewRate(orientation);
            // Start tracking move position
            // Slew rate has to be the actual hardware slew rate, but orientation is the logical one (differs from hardware one if the axis is inverted)
            this.axisMovementTracker.Start(this.CalculateSlewSign(orientation) * hardwareSlewRate);
            // Respect the duration if it has been specified
            if (moveDuration != null)
            {
                if (asynchronous)
                {
                    this.WaitAndStopAsync((TimeSpan)moveDuration);
                }
                else
                {
                    this.WaitAndStop((TimeSpan)moveDuration);
                }
            }
        }

        /// <summary>
        /// Move the hardware with the given Orientation
        /// </summary>
        /// <param name="orientation">Orientation to move the axis (+ or -)</param>
        /// <returns>The slew rate of the hardware for this command</returns>
        private double MoveHardwareAndCalculateSlewRate(Orientation orientation)
        {
            // Orientation the mount has to move changes if the axis is inverted
            Orientation mountOrientation = this.CalculateMountOrientation(orientation);
            // Tell the device to move
            this.deviceController.Move(this.axis, mountOrientation);
            // Actual Slew rate of the hardware for the physical orientation
            return this.CalculateSlewRate(mountOrientation);
        }

        /// <summary>
        /// Move the axis with the given orientation.
        /// Return immediatly leaving the axis moving for as long as Stop or another move is not called.
        /// <param name="orientation">Orientation of the move</param>
        /// </summary>
        public void Move(Orientation orientation)
        {
            this.Move(null, orientation);
        }

        /// <summary>
        /// Move the axis for the given duration with the given orientation.
        /// Return immediatly without waiting for the move to complete.
        /// <param name="moveDuration">Duration of the move</param>
        /// <param name="orientation">Orientation of the move</param>
        /// </summary>
        public void Move(TimeSpan? moveDuration, Orientation orientation)
        {
            this.Log("Move", "orientation=" + orientation);
            this.Move(moveDuration, orientation, true);
        }

        /// <summary>
        /// Move the axis until the position changes by the given positionDifference.
        /// Return immediatly without waiting for the move to complete.
        /// <param name="positionDifference">Value to add or remove to the current position</param>
        /// </summary>
        public void Move(double positionDifference)
        {
            this.Log("Move", "positionDifference=" + positionDifference + ", currentPosition=" + this.Position);
            Orientation orientation = this.CalculateOrientation(positionDifference);
            // Slew rate is dependent of the mount hardware orientation
            double slewRate = this.CalculateSlewRate(this.CalculateMountOrientation(orientation));
            double remainingTimeInSeconds = Math.Abs(positionDifference / slewRate);
            // Calculates the time difference to reach the given position
            TimeSpan time = TimeSpan.FromSeconds(remainingTimeInSeconds);
            this.Move(time, orientation, true);
        }

        /// <summary>
        /// Stop the hardware slew and aborts the waiting thread
        /// </summary>
        public void Stop()
        {
            if (this.waitAndStopAsyncThread != null && this.waitAndStopAsyncThread.IsAlive)
            {
                //Stops the waiting thread
                this.waitAndStopAsyncThread.Abort();
                this.waitAndStopAsyncThread = null;
            }
            this.StopMovement();
        }

        /// <summary>
        /// Wait for the axis to stop moving.
        /// Will return immediately if no asynchronous move is currently taking place.
        /// </summary>
        public void WaitAsyncMoveEnd()
        {
            if (this.waitAndStopAsyncThread != null && this.waitAndStopAsyncThread.IsAlive)
            {
                this.waitAndStopAsyncThread.Join();
            }
        }
        /// <summary>
        /// Wait the given amount of time, then stop the movement
        /// </summary>
        private void WaitAndStop(TimeSpan moveDuration)
        {
            Thread.Sleep(moveDuration);
            this.StopMovement();
            this.Log("WaitAndStop", "Waited for move to complete for " + moveDuration.ToString() + " sec, stopped. Current position is " + this.Position);
        }

        /// <summary>
        /// Stop the movement and the position tracking
        /// </summary>
        private void StopMovement()
        {
            //Stop the position tracking
            this.axisMovementTracker.Stop();

            //Stop the hardware movement
            this.deviceController.Move(this.axis, null);
        }

        /// <summary>
        /// Start a thread that will execute WaitAndStop so that the waiting can be done in background.
        /// Ensure that no other waiting thread for the axis is active before calling.
        /// </summary>
        private void WaitAndStopAsync(TimeSpan moveDuration)
        {
            this.Log("StartAsyncMove", "Starting async thread");
            this.waitAndStopAsyncThread = new Thread(
                 delegate(object data)
                 {
                     WaitAndStop(moveDuration);
                 }
             );
            this.waitAndStopAsyncThread.Start();
        }

        public void Dispose()
        {
            Stop();
            // Clean up the tracelogger and util objects
            traceLogger.Enabled = false;
            traceLogger.Dispose();
            traceLogger = null;
        }

        private void Log(String id, String message)
        {
            this.traceLogger.LogMessage(id + " " + this.axis.ToString(), message);
        }
    }
}

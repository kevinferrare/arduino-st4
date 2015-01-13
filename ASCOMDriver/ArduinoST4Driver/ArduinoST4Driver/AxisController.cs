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
        private Axis axis;

        /// <summary>
        /// Helper to keep track of the position along the axis depending on the slew time
        /// </summary>
        private AxisMovementTracker axisMovementTracker;

        /// <summary>
        /// Actual hardware controller
        /// </summary>
        private DeviceController deviceController;

        /// <summary>
        /// Thread that will wait for axis to finish slewing asynchronously
        /// </summary>
        private Thread waitAndStopAsyncThread;

        /// <summary>
        /// Slew rate to use when moving in the MINUS orientation
        /// </summary>
        private double minusSlewRate;

        /// <summary>
        /// Slew rate to use when moving in the PLUS orientation
        /// </summary>
        private double plusSlewRate;

        /// <summary>
        /// Builds an AxisPositionController object.
        /// 
        /// </summary>
        /// <param name="axis">Axis controlled by this object</param>
        /// <param name="deviceController">Actual hardware controller</param>
        /// <param name="minusSlewRate">Slew rate to use when moving in the MINUS orientation</param>
        /// <param name="plusSlewRate">Slew rate to use when moving in the PLUS orientation</param>
        public AxisController(Axis axis, DeviceController deviceController, double minusSlewRate, double plusSlewRate)
        {
            this.traceLogger = new TraceLogger("", "ArduinoST4 AxisPositionController " + axis.ToString());
            this.traceLogger.Enabled = true;
            this.axis = axis;
            this.deviceController = deviceController;
            this.minusSlewRate = minusSlewRate;
            this.plusSlewRate = plusSlewRate;
            this.axisMovementTracker = new AxisMovementTracker();
        }

        /// <summary>
        /// Returns the slew rate corresponding to the given orientation
        /// </summary>
        /// <param name="slewOrientation">Orientation for which the slew rate is requested</param>
        /// <returns></returns>
        private double CalculateSlewRate(Orientation slewOrientation)
        {
            return slewOrientation == Orientation.PLUS ? this.plusSlewRate : this.minusSlewRate;
        }

        /// <summary>
        /// Returns an orientation from the given positive or negative slew rate.
        /// </summary>
        /// <param name="slewRate">Slew rate, eithe positive or negative</param>
        /// <returns></returns>
        private Orientation CalculateOrientation(double slewRate)
        {
            return slewRate > 0 ? Orientation.PLUS : Orientation.MINUS;
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
        /// Moves the axis for the given time with the given orientation.
        /// If provided time is null or async is true, will not wait for the move to complete.
        /// 
        /// <param name="moveDuration">Duration of the move, moves indefinitely if null</param>
        /// <param name="orientation">Orientation to move the axis (+ or -)</param>
        /// <param name="async">If true, will return without waiting for the move to complete </param>
        /// </summary>
        protected void Move(TimeSpan? moveDuration, Orientation orientation, Boolean async)
        {
            this.Log("Move", "duration=" + (moveDuration == null ? "null" : moveDuration.ToString()) + ", orientation=" + orientation + ", async=" + async);
            //Stop any async move
            this.Log("Move", "Stopping any move");
            this.Stop();
            if (moveDuration != null && ((TimeSpan)moveDuration).TotalMilliseconds == 0)
            {
                this.Log("Move", "Nothing to do :)");
                //Nothing to do :)
                return;
            }
            //Tell the device to move
            this.deviceController.Move(this.axis, orientation);
            //Start tracking move position
            this.axisMovementTracker.Start(this.CalculateSlewRate(orientation));
            //Respect the duration if it has been specified
            if (moveDuration != null)
            {
                if (async)
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
        /// Moves the axis with the given orientation.
        /// Returns immediatly leaving the axis moving for as long as Stop or another move is not called.
        /// <param name="orientation">Orientation of the move</param>
        /// </summary>
        public void Move(Orientation orientation)
        {
            this.Move(null, orientation);
        }

        /// <summary>
        /// Moves the axis for the given duration with the given orientation.
        /// Returns immediatly without waiting for the move to complete.
        /// <param name="moveDuration">Duration of the move</param>
        /// <param name="orientation">Orientation of the move</param>
        /// </summary>
        public void Move(TimeSpan? moveDuration, Orientation orientation)
        {
            this.Log("Move", "orientation=" + orientation);
            this.Move(moveDuration, orientation, true);
        }

        /// <summary>
        /// Moves the axis until the position changes by the given positionDifference.
        /// Returns immediatly without waiting for the move to complete.
        /// <param name="positionDifference">Value to add or remove to the current position</param>
        /// </summary>
        public void Move(double positionDifference)
        {
            this.Log("Move", "positionDifference=" + positionDifference + ", currentPosition=" + this.Position);
            Orientation orientation = this.CalculateOrientation(positionDifference);
            double remainingTimeInSeconds = positionDifference / this.CalculateSlewRate(orientation);
            //Calculates the time difference to reach the given position
            TimeSpan time = TimeSpan.FromSeconds(remainingTimeInSeconds);
            this.Move(time, orientation, true);
        }

        /// <summary>
        /// Stops the hardware slew and aborts the waiting thread
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
        /// Waits for the axis to stop moving.
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
        /// Waits the given amount of time, then stop the movement
        /// </summary>
        private void WaitAndStop(TimeSpan moveDuration)
        {
            Thread.Sleep(moveDuration);
            this.StopMovement();
            this.Log("WaitAndStop", "Waited for move to complete for " + moveDuration.ToString() + " sec, stopped. Current position is " + this.Position);
        }

        /// <summary>
        /// Stops the movement and the position tracking
        /// </summary>
        private void StopMovement()
        {
            //Stop the position tracking
            this.axisMovementTracker.Stop();
            //Check for connection for the case when this method is called from Dispose
            if (deviceController.Connected)
            {
                //Stop the hardware movement
                this.deviceController.Move(this.axis, null);
            }
        }

        /// <summary>
        /// Starts a thread that will execute WaitAndStop so that the waiting can be done in background.
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

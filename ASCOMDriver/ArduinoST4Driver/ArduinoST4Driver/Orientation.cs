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
    /// <summary>
    /// Orientation of the movement along an axis. Can be forward (PLUS) or backward (MINUS)
    /// </summary>
    public enum Orientation
    {
        PLUS, MINUS
    }

    public static class OrientationExtensions
    {
        /// <summary>
        /// Inverts the given orientation
        /// <param name="orientation">Orientation invert, + will give - and - will give +</param>
        /// </summary>
        public static Orientation Invert(this Orientation orientation)
        {
            if (orientation == Orientation.PLUS)
            {
                return Orientation.MINUS;
            }
            return Orientation.PLUS;
        }

        /// <summary>
        /// Inverts the given orientation if the given invert parameter is true
        /// <param name="orientation">Orientation invert, + will give - and - will give +</param>
        /// <param name="invert">Whether to invert or not</param>
        /// </summary>
        public static Orientation Invert(this Orientation orientation, bool invert)
        {
            if (invert)
            {
                return orientation.Invert();
            }
            return orientation;
        }
    }
}

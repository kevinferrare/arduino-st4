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
    /// Constants used by the driver
    /// </summary>
    /// Author:  Kevin Ferrare
    class Constants
    {
        public const double DEGREES_PER_SECOND = 360d / (24d * 3600d);
        public const double RA_PER_SECOND = 1d / 3600d;
    }
}

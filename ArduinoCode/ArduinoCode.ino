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
//
// Copyright Kevin Ferrare 2014

/*
 * Led indicates whether the software requested connection, it is shut off when the software disconnects
 */
const int pinLED = 13;

/**
 * An axis has a pin per direction.
 * Both pins cannot be up at the same time.
 */
class Axis {
private:
  int plusPin;
  int minusPin;
public:
  Axis(int plusPin, int minusPin) : 
  plusPin(plusPin), minusPin(minusPin) {
  }
  void setupPins(){
    pinMode(this->plusPin, OUTPUT);
    pinMode(this->minusPin, OUTPUT);
  }
  void plus(){
    digitalWrite(this->minusPin, LOW);
    digitalWrite(this->plusPin, HIGH);
  }
  void minus(){
    digitalWrite(this->plusPin, LOW);
    digitalWrite(this->minusPin, HIGH);
  }
  void reset(){
    digitalWrite(this->minusPin, LOW);
    digitalWrite(this->plusPin, LOW);
  }
};

class Axis rightAscension(
2,//RA+ pin
5);//RA- pin
class Axis declination(
3,//DEC+ pin
4);//DEC- pin

void setup()
{
  rightAscension.setupPins();
  declination.setupPins();
  pinMode(pinLED, OUTPUT);
  //57.6k, 8 data bits, no parity, one stop bit.
  Serial.begin(57600, SERIAL_8N1);
  //Wait for serial port to connect. Needed for Leonardo only
  while (!Serial);
  Serial.println("INITIALIZED#");
}

void resetPins(){
  rightAscension.reset();
  declination.reset();
}

void loop()
{
  if (Serial.available() > 0) {
    //Received something
    String opcode = Serial.readStringUntil('#');
    boolean validOpcode=true;
    //Parse opcode
    if(opcode=="CONNECT"){
      digitalWrite(pinLED, HIGH);
      resetPins();
    }
    else if (opcode=="DISCONNECT"){
      digitalWrite(pinLED, LOW);
      resetPins();
    }
    else if(opcode=="RA0"){
      rightAscension.reset();
    }
    else if(opcode=="RA+"){
      rightAscension.plus();
    }
    else if(opcode=="RA-"){
      rightAscension.minus();
    }
    else if(opcode=="DEC0"){
      declination.reset();
    }
    else if(opcode=="DEC+"){
      declination.plus();
    }
    else if(opcode=="DEC-"){
      declination.minus();
    }
    else{
      validOpcode=false;
    }
    if(validOpcode){
      //Acknowledge valid command
      Serial.println("OK#");
    }
  } 
}

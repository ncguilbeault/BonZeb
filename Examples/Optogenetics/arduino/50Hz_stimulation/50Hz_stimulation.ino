// Define variables 
bool state = false; // defines initial state. Do not change
int pinNum = 8; // defines the pin number used as the digital output pin. Change to match your configuration
int lightOnDelay = 10; // defines the duration of the on signal in ms. Stimulation frequency = 1000 / (lightOnDelay + lightOffDelay) 
int lightOffDelay = 10; // defines the duration of the off signal in ms
char inputChar; // defines the variable received from the Bonsai script. Do not change

// define function to initialize the arduino
void setup() {
  pinMode(pinNum, OUTPUT); // define pinNum as output
  Serial.begin(9600); // open channel to read serial input
  while (!Serial) { 
    ; // wait until channel is open
  }
}

// define function to check status of arduino in a loop
void loop() {
  if (!state && digitalRead(pinNum)) { // check if state is 0
    digitalWrite(pinNum, LOW); // set pin to low voltage
  } else if (state) { // check if state is 1
    digitalWrite(pinNum, HIGH); // set pin to high voltage
    delay(lightOnDelay); // wait for the duration of the light on period
    digitalWrite(pinNum, LOW); // set pin to low voltage
    delay(lightOffDelay); // wait for the duration of the light off period
  }
}

//define function to monitor serial communication channel
void serialEvent() {
  while (Serial.available() > 0) { // check if data are available
    inputChar = Serial.read(); // read data
    if (inputChar == 'T') { // check if data = T
      state = 1; // set state to 1
    } else if (inputChar == 'F') { // check if data = F
      state = 0; // set state to 0
    }
  }
}

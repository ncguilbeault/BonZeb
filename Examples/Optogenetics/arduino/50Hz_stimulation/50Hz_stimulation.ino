bool state = false;
int pinNum = 8;
int lightOnDelay = 10;
int lightOffDelay = 10;
char inputChar;
String inputString;

void setup() {
  pinMode(pinNum, OUTPUT);
  Serial.begin(9600);
  while (!Serial) {
    ;
  }
}

void loop() {
  if (!state && digitalRead(pinNum)) {
    digitalWrite(pinNum, LOW);
  } else if (state) {
    digitalWrite(pinNum, HIGH);
    delay(lightOnDelay);
    digitalWrite(pinNum, LOW);
    delay(lightOffDelay);
  }
}

void serialEvent() {
  while (Serial.available() > 0) {
    inputChar = Serial.read();
    if (inputChar == 'T') {
      state = 1;
    } else if (inputChar == 'F') {
      state = 0;
    }
  }
}

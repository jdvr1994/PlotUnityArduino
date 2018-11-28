const int analogInPin = A0;  // Analog input pin that the potentiometer is attached to
int sensorValue = 0;        // value read from the pot

void setup() {
  // put your setup code here, to run once:
  Serial.begin(500000);
}

void loop() {
  // put your main code here, to run repeatedly:
  if(Serial.available()>0){
    Serial.read();
    sensorValue = analogRead(analogInPin);
    uint8_t highByte = (sensorValue>>8)&0x00FF;
    uint8_t lowByte = sensorValue&0x00FF;
    Serial.write(highByte);  
    Serial.write(lowByte);    
    Serial.println();
  }
}

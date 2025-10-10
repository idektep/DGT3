//-------------------------------------------------Declare variable-------------------------------------------------------//
#define Shoot_Pin 5
#define L1_IR 23  // Left IR sensor
#define R1_IR 34  // Right IR sensor
#define L_LED 16  // Left LED
#define R_LED 17  // Right LED
//-----------------------------------------------------------------------------------------------------------------------//

//-------------------------------------------------edit Parameter-------------------------------------------------------//

void setup() {
  Serial.begin(115200);
  motorsetup();
  //--------- IR Pin ---------------//
  pinMode(L1_IR, INPUT);
  pinMode(R1_IR, INPUT);
  //---------- LED Pin -----------------//
  pinMode(L_LED, OUTPUT);
  pinMode(R_LED, OUTPUT);

//----------------LED ON--------------//
  digitalWrite(L_LED, HIGH);
  digitalWrite(R_LED, HIGH);
//----------------Start Mqtt--------------//
  MQTT_setup();
}
void loop() {
  MQTT_run();
}



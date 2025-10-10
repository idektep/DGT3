#define MR_IN1 12 //motor1
#define MR_IN2 13 //motor1
#define MR_IN3 33 //motor2
#define MR_IN4 32 //motor2
#define ML_IN3 25 //motor3
#define ML_IN4 26 //motor3
#define ML_IN1 14 //motor4
#define ML_IN2 27 //motor4

#define L_ENA 15  //Pin speed motor1
#define L_ENB 2   //Pin speed motor2
#define R_ENA 19  //Pin speed motor4
#define R_ENB 4   //Pin speed motor3

uint8_t SpeedM1 = 170;  //Adjust speed  //motor1
uint8_t SpeedM2 = 150;  //Adjust speed //motor2
uint8_t SpeedM3 = 150;  //Adjust speed //motor3
uint8_t SpeedM4 = 150;  //Adjust speed //motor4

void motorsetup() {

  pinMode(MR_IN1, OUTPUT);
  pinMode(MR_IN2, OUTPUT);
  pinMode(MR_IN3, OUTPUT);
  pinMode(MR_IN4, OUTPUT);
  pinMode(ML_IN1, OUTPUT);
  pinMode(ML_IN2, OUTPUT);
  pinMode(ML_IN3, OUTPUT);
  pinMode(ML_IN4, OUTPUT);
  pinMode(L_ENA, OUTPUT);
  pinMode(L_ENB, OUTPUT);
  pinMode(R_ENA, OUTPUT);
  pinMode(R_ENB, OUTPUT);
  stop();

}
void forward() {
  analogWrite(R_ENA, SpeedM1);
  analogWrite(R_ENB, SpeedM2);
  analogWrite(L_ENB, SpeedM3);
  analogWrite(L_ENA, SpeedM4);

  digitalWrite(MR_IN1, HIGH);  //motor1
  digitalWrite(MR_IN2, LOW);   //motor1
  digitalWrite(MR_IN3, HIGH);  //motor2
  digitalWrite(MR_IN4, LOW);   //motor2
  digitalWrite(ML_IN1, HIGH);  //motor4
  digitalWrite(ML_IN2, LOW);   //motor4
  digitalWrite(ML_IN3, HIGH);  //motor3
  digitalWrite(ML_IN4, LOW);   //motor3
}

void backward() {
  analogWrite(R_ENA, SpeedM1);
  analogWrite(R_ENB, SpeedM2);
  analogWrite(L_ENB, SpeedM3);
  analogWrite(L_ENA, SpeedM4);

  digitalWrite(MR_IN1, LOW);   //motor1
  digitalWrite(MR_IN2, HIGH);  //motor1
  digitalWrite(MR_IN3, LOW);   //motor2
  digitalWrite(MR_IN4, HIGH);  //motor2
  digitalWrite(ML_IN1, LOW);   //motor4
  digitalWrite(ML_IN2, HIGH);  //motor4
  digitalWrite(ML_IN3, LOW);   //motor3
  digitalWrite(ML_IN4, HIGH);  //motor3
}

void turn_right() {
  analogWrite(R_ENA, SpeedM1);
  analogWrite(R_ENB, SpeedM2);
  analogWrite(L_ENB, SpeedM3);
  analogWrite(L_ENA, SpeedM4);

  digitalWrite(MR_IN1, LOW);   //motor1
  digitalWrite(MR_IN2, HIGH);  //motor1
  digitalWrite(MR_IN3, LOW);   //motor2
  digitalWrite(MR_IN4, HIGH);  //motor2
  digitalWrite(ML_IN1, HIGH);  //motor4
  digitalWrite(ML_IN2, LOW);   //motor4
  digitalWrite(ML_IN3, HIGH);  //motor3
  digitalWrite(ML_IN4, LOW);   //motor3
}

void turn_left() {
  analogWrite(R_ENA, SpeedM1);
  analogWrite(R_ENB, SpeedM2);
  analogWrite(L_ENB, SpeedM3);
  analogWrite(L_ENA, SpeedM4);

  digitalWrite(MR_IN1, HIGH);  //motor1
  digitalWrite(MR_IN2, LOW);   //motor1
  digitalWrite(MR_IN3, HIGH);  //motor2
  digitalWrite(MR_IN4, LOW);   //motor2
  digitalWrite(ML_IN1, LOW);   //motor4
  digitalWrite(ML_IN2, HIGH);  //motor4
  digitalWrite(ML_IN3, LOW);   //motor3
  digitalWrite(ML_IN4, HIGH);  //motor3
}

void slide_left() {
  analogWrite(R_ENA, SpeedM1);
  analogWrite(R_ENB, SpeedM2);
  analogWrite(L_ENB, SpeedM3);
  analogWrite(L_ENA, SpeedM4);

  digitalWrite(MR_IN1, LOW);   //motor1
  digitalWrite(MR_IN2, HIGH);  //motor1
  digitalWrite(MR_IN3, HIGH);  //motor2
  digitalWrite(MR_IN4, LOW);   //motor2
  digitalWrite(ML_IN1, LOW);   //motor4
  digitalWrite(ML_IN2, HIGH);  //motor4
  digitalWrite(ML_IN3, HIGH);  //motor3
  digitalWrite(ML_IN4, LOW);   //motor3
}

void slide_right() {
  analogWrite(R_ENA, SpeedM1);
  analogWrite(R_ENB, SpeedM2);
  analogWrite(L_ENB, SpeedM3);
  analogWrite(L_ENA, SpeedM4);

  digitalWrite(MR_IN1, HIGH);  //motor1
  digitalWrite(MR_IN2, LOW);   //motor1
  digitalWrite(MR_IN3, LOW);   //motor2
  digitalWrite(MR_IN4, HIGH);  //motor2
  digitalWrite(ML_IN1, HIGH);  //motor4
  digitalWrite(ML_IN2, LOW);   //motor4
  digitalWrite(ML_IN3, LOW);   //motor3
  digitalWrite(ML_IN4, HIGH);  //motor3
}

void slide_left_forward() {
  analogWrite(R_ENA, SpeedM1);
  analogWrite(R_ENB, SpeedM2);
  analogWrite(L_ENB, SpeedM3);
  analogWrite(L_ENA, SpeedM4);

  digitalWrite(MR_IN1, LOW);   //motor1
  digitalWrite(MR_IN2, LOW);   //motor1
  digitalWrite(MR_IN3, HIGH);  //motor2
  digitalWrite(MR_IN4, LOW);   //motor2
  digitalWrite(ML_IN1, LOW);   //motor4
  digitalWrite(ML_IN2, LOW);   //motor4
  digitalWrite(ML_IN3, HIGH);  //motor3
  digitalWrite(ML_IN4, LOW);   //motor3
}

void slide_right_forward() {
  analogWrite(R_ENA, SpeedM1);
  analogWrite(R_ENB, SpeedM2);
  analogWrite(L_ENB, SpeedM3);
  analogWrite(L_ENA, SpeedM4);

  digitalWrite(MR_IN1, HIGH);  //motor1
  digitalWrite(MR_IN2, LOW);   //motor1
  digitalWrite(MR_IN3, LOW);   //motor2
  digitalWrite(MR_IN4, LOW);   //motor2
  digitalWrite(ML_IN1, HIGH);  //motor4
  digitalWrite(ML_IN2, LOW);   //motor4
  digitalWrite(ML_IN3, LOW);   //motor3
  digitalWrite(ML_IN4, LOW);   //motor3
}

void slide_left_backward() {
  analogWrite(R_ENA, SpeedM1);
  analogWrite(R_ENB, SpeedM2);
  analogWrite(L_ENB, SpeedM3);
  analogWrite(L_ENA, SpeedM4);

  digitalWrite(MR_IN1, LOW);   //motor1
  digitalWrite(MR_IN2, HIGH);  //motor1
  digitalWrite(MR_IN3, LOW);   //motor2
  digitalWrite(MR_IN4, LOW);   //motor2
  digitalWrite(ML_IN1, LOW);   //motor4
  digitalWrite(ML_IN2, HIGH);  //motor4
  digitalWrite(ML_IN3, LOW);   //motor3
  digitalWrite(ML_IN4, LOW);   //motor3
}

void slide_right_backward() {
  analogWrite(R_ENA, SpeedM1);
  analogWrite(R_ENB, SpeedM2);
  analogWrite(L_ENB, SpeedM3);
  analogWrite(L_ENA, SpeedM4);

  digitalWrite(MR_IN1, LOW);   //motor1
  digitalWrite(MR_IN2, LOW);   //motor1
  digitalWrite(MR_IN3, LOW);   //motor2
  digitalWrite(MR_IN4, HIGH);  //motor2
  digitalWrite(ML_IN1, LOW);   //motor4
  digitalWrite(ML_IN2, LOW);   //motor4
  digitalWrite(ML_IN3, LOW);   //motor3
  digitalWrite(ML_IN4, HIGH);  //motor3
}

void stop() {
  digitalWrite(MR_IN1, HIGH);  //motor1
  digitalWrite(MR_IN2, HIGH);  //motor1
  digitalWrite(MR_IN3, HIGH);  //motor2
  digitalWrite(MR_IN4, HIGH);  //motor2
  digitalWrite(ML_IN1, HIGH);  //motor4
  digitalWrite(ML_IN2, HIGH);  //motor4
  digitalWrite(ML_IN3, HIGH);  //motor3
  digitalWrite(ML_IN4, HIGH);  //motor3
}

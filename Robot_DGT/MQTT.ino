#include <WiFi.h>
#include <PubSubClient.h>

WiFiClient wificlient;
PubSubClient mqttclient(wificlient);

const char *ssid = "***********";
const char *password = "***********";

const char *mqtt_broker = "broker.emqx.io";
const char *topic = "***********";
const int mqtt_port = 1883;
const char *mqtt_client_id = "***********";

#define BUZZER_PIN 18
#define LED_PIN 2
String msg;
int speedValue = 0;
void callback(char *topic, byte *payload, unsigned int length) {
  payload[length] = '\0';
  String payload_str = (char *)payload;

  payload_str.toLowerCase();

  msg = payload_str;
  Serial.print("Received [");
  Serial.print(topic);
  Serial.print("] : ");
  Serial.println(payload_str);

  // ---------------- แยกคำสั่ง ----------------
  if (payload_str.startsWith("move:")) {
    if (payload_str == "move:forward") {
      Serial.println("Command: MOVE FORWARD");
      forward();
    } else if (payload_str == "move:left") {
      Serial.println("Command: MOVE LEFT");
      slide_left();
    }else if (payload_str == "move:right") {
      Serial.println("Command: MOVE RIGHT");
      slide_right();
    }else if (payload_str == "move:backward") {
      Serial.println("Command: MOVE RIGHT");
      backward();
    }else if (payload_str == "move:stop") {
      Serial.println("Command: STOP");
      stop();
    }
  } else if (payload_str.startsWith("speed:")) {
    String valueStr = payload_str.substring(6);
    speedValue = valueStr.toInt();
    Serial.print("Set Speed = ");
    Serial.println(speedValue);
    SpeedM1 = speedValue;
    SpeedM2 = speedValue;
    SpeedM3 = speedValue;
    SpeedM4 = speedValue;
  }
}

void MQTT_setup() {
  Serial.begin(115200);
  pinMode(LED_PIN, OUTPUT);
  pinMode(BUZZER_PIN, OUTPUT);
  Serial.println("Connecting WiFi");
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\nWiFi connected");

  Serial.println("Connecting MQTT");
  mqttclient.setServer(mqtt_broker, mqtt_port);
  mqttclient.setCallback(callback);
  while (!mqttclient.connected()) {
    Serial.printf("The client %s connects to the public MQTT broker\n", mqtt_client_id);
    if (mqttclient.connect(mqtt_client_id)) {
      Serial.println("MQTT connected");
    } else {
      Serial.print("failed with state ");
      Serial.println(mqttclient.state());
      delay(2000);
    }
  }
  digitalWrite(BUZZER_PIN,HIGH);
  delay(1000);
  digitalWrite(BUZZER_PIN,LOW);
  mqttclient.subscribe(topic);
}

void MQTT_run() {
  mqttclient.loop();
}


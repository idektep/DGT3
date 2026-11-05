#include <WiFi.h>
#include <PubSubClient.h>
#include <DHT.h>

// ======================
// USER CONFIG
// ======================

// WiFi
const char* WIFI_SSID = "wifi";
const char* WIFI_PASS = "password";

// MQTT
const char* MQTT_BROKER = "broker.emqx.io";
const uint16_t MQTT_PORT = 1883;

// เปลี่ยน prefix ได้ตรงนี้
// เช่น "idt", "dgt", "lab1"
const char* TOPIC_PREFIX = "<prefix>";

// ส่ง Sensor ทุกกี่ ms
const uint32_t PUBLISH_INTERVAL_MS = 1000;


// ======================
// PIN DEFINITIONS
// ======================

#define DHTPIN 23
#define DHTTYPE DHT22

#define LDR_PIN 34
#define RESET_PIN 16
#define BUZZER_PIN 18

const int ledPins[6] = {
  12,
  14,
  27,
  26,
  25,
  33
};


// ======================
// OBJECTS
// ======================

DHT dht(
  DHTPIN,
  DHTTYPE);

WiFiClient wifiClient;

PubSubClient mqttClient(
  wifiClient);


// ======================
// TOPICS
// ======================

// Sensor
String topic_temp;
String topic_hum;
String topic_ldr;

// Command
String topic_buzzer;
String topic_led_base;

// Status
String topic_status_buzzer;
String topic_status_led_base;


// ======================
// MQTT CLIENT ID
// ======================

char mqtt_client_id[64];


// ======================
// BUILD TOPICS
// ======================

void buildTopics() {
  // Sensor
  topic_temp =
    String(TOPIC_PREFIX)
    + "/temperature";

  topic_hum =
    String(TOPIC_PREFIX)
    + "/humidity";

  topic_ldr =
    String(TOPIC_PREFIX)
    + "/ldr";


  // Command
  topic_buzzer =
    String(TOPIC_PREFIX)
    + "/buzzer";

  topic_led_base =
    String(TOPIC_PREFIX)
    + "/led/led";


  // Status
  topic_status_buzzer =
    String(TOPIC_PREFIX)
    + "/status/buzzer";

  topic_status_led_base =
    String(TOPIC_PREFIX)
    + "/status/led";
}


// ======================
// RANDOM CLIENT ID
// ======================

void buildClientId() {
  uint32_t r =
    (uint32_t)esp_random();

  String id =
    String(TOPIC_PREFIX)
    + "_LDR_DHT_"
    + WiFi.macAddress()
    + "_"
    + String(r, HEX);

  id.replace(
    ":",
    "");

  id.toUpperCase();

  strlcpy(
    mqtt_client_id,
    id.c_str(),
    sizeof(mqtt_client_id));
}


// ======================
// WIFI CONNECT
// ======================

void connectWiFi() {
  WiFi.mode(
    WIFI_STA);

  WiFi.begin(
    WIFI_SSID,
    WIFI_PASS);

  Serial.print(
    "Connecting WiFi");

  uint32_t t0 =
    millis();

  while (
    WiFi.status()
    != WL_CONNECTED) {
    delay(300);

    Serial.print(".");

    if (
      millis() - t0
      > 20000) {
      Serial.println(
        "\nWiFi connect timeout -> restart");

      ESP.restart();
    }
  }

  Serial.println(
    "\nWiFi connected");

  Serial.print(
    "IP: ");

  Serial.println(
    WiFi.localIP());
}


// ======================
// MQTT CALLBACK
// ======================

void callback(
  char* topic,
  byte* payload,
  unsigned int length) {
  // ======================
  // PAYLOAD -> STRING
  // ======================

  String message;

  message.reserve(
    length + 1);

  for (
    unsigned int i = 0;
    i < length;
    i++) {
    message +=
      (char)payload[i];
  }

  message.trim();

  message.toLowerCase();

  String topicStr =
    String(topic);


  Serial.printf(
    "Message from [%s]: %s\n",
    topicStr.c_str(),
    message.c_str());


  // ======================
  // BUZZER CONTROL
  // ======================

  if (
    topicStr
    == topic_buzzer) {
    if (
      message == "on") {
      digitalWrite(
        BUZZER_PIN,
        HIGH);

      mqttClient.publish(
        topic_status_buzzer.c_str(),
        "on");
    } else if (
      message == "off") {
      digitalWrite(
        BUZZER_PIN,
        LOW);

      mqttClient.publish(
        topic_status_buzzer.c_str(),
        "off");
    }

    Serial.printf(
      "Buzzer %s\n",
      message.c_str());

    return;
  }


  // ======================
  // LED CONTROL
  // ======================

  for (
    int i = 1;
    i <= 6;
    i++) {
    String commandTopic =
      topic_led_base
      + String(i);

    if (
      topicStr
      == commandTopic) {
      String statusTopic =
        topic_status_led_base
        + String(i);


      if (
        message == "on") {
        digitalWrite(
          ledPins[i - 1],
          HIGH);

        mqttClient.publish(
          statusTopic.c_str(),
          "on");
      } else if (
        message == "off") {
        digitalWrite(
          ledPins[i - 1],
          LOW);

        mqttClient.publish(
          statusTopic.c_str(),
          "off");
      }


      Serial.printf(
        "LED%d %s\n",
        i,
        message.c_str());

      return;
    }
  }
}


// ======================
// MQTT CONNECT
// ======================

void connectMQTT() {
  mqttClient.setServer(
    MQTT_BROKER,
    MQTT_PORT);

  while (
    !mqttClient.connected()) {
    Serial.println(
      "Connecting to MQTT...");


    if (
      mqttClient.connect(
        mqtt_client_id)) {
      Serial.println(
        "MQTT connected");

      Serial.print(
        "ClientID: ");

      Serial.println(
        mqtt_client_id);


      // ======================
      // SUBSCRIBE BUZZER
      // ======================

      mqttClient.subscribe(
        topic_buzzer.c_str());


      // ======================
      // SUBSCRIBE LED 1 - 6
      // ======================

      for (
        int i = 1;
        i <= 6;
        i++) {
        String t =
          topic_led_base
          + String(i);

        mqttClient.subscribe(
          t.c_str());
      }
    } else {
      Serial.print(
        "failed, rc=");

      Serial.print(
        mqttClient.state());

      Serial.println(
        " try again in 2 seconds");

      delay(2000);
    }
  }
}


// ======================
// SETUP
// ======================

void setup() {
  Serial.begin(
    115200);


  // RESET
  pinMode(
    RESET_PIN,
    INPUT_PULLUP);


  // BUZZER
  pinMode(
    BUZZER_PIN,
    OUTPUT);

  digitalWrite(
    BUZZER_PIN,
    LOW);


  // LED 1 - 6
  for (
    int i = 0;
    i < 6;
    i++) {
    pinMode(
      ledPins[i],
      OUTPUT);

    digitalWrite(
      ledPins[i],
      LOW);
  }


  // DHT
  dht.begin();


  // ======================
  // RESET BUTTON
  // ======================

  if (
    digitalRead(
      RESET_PIN)
    == LOW) {
    Serial.println(
      "Reset pressed -> restart");

    digitalWrite(
      BUZZER_PIN,
      HIGH);

    delay(300);

    digitalWrite(
      BUZZER_PIN,
      LOW);

    delay(300);

    ESP.restart();
  }


  // ======================
  // START SYSTEM
  // ======================

  buildTopics();

  connectWiFi();

  buildClientId();

  mqttClient.setCallback(
    callback);

  connectMQTT();


  // ======================
  // STARTUP BEEP
  // ======================

  for (int i = 0;i < 2;i++) {
    digitalWrite(
      BUZZER_PIN,
      HIGH);

    delay(200);

    digitalWrite(
      BUZZER_PIN,
      LOW);

    delay(200);
  }
}


// ======================
// LOOP
// ======================

uint32_t lastPub = 0;


void loop() {
  // ======================
  // WIFI RECONNECT
  // ======================

  if (
    WiFi.status()
    != WL_CONNECTED) {
    connectWiFi();

    buildClientId();
  }


  // ======================
  // MQTT RECONNECT
  // ======================

  if (
    !mqttClient.connected()) {
    connectMQTT();
  }


  mqttClient.loop();


  // ======================
  // SENSOR PUBLISH
  // ======================

  uint32_t now =
    millis();

  if (
    now - lastPub
    >= PUBLISH_INTERVAL_MS) {
    lastPub =
      now;


    float temp =
      dht.readTemperature();

    float hum =
      dht.readHumidity();

    int ldr =
      analogRead(
        LDR_PIN);


    // Temperature
    if (
      !isnan(temp)) {
      mqttClient.publish(
        topic_temp.c_str(),
        String(
          temp,
          2)
          .c_str());
    }


    // Humidity
    if (
      !isnan(hum)) {
      mqttClient.publish(
        topic_hum.c_str(),
        String(
          hum,
          2)
          .c_str());
    }


    // LDR
    mqttClient.publish(
      topic_ldr.c_str(),
      String(
        ldr)
        .c_str());
  }
}
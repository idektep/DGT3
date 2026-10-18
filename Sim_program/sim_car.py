"""
============================================================
CAR MQTT SIMULATOR
============================================================
------------------------------------------------------------
MQTT TOPICS
------------------------------------------------------------

{TOPIC_PREFIX}/position/x
    Type    : Float
    Example : 0.00, 5.20, 12.80
    Meaning : ตำแหน่งรถแกน X
    Unity   : ใช้ควบคุม Position X

{TOPIC_PREFIX}/position/z
    Type    : Float
    Example : 0.00, 3.50, 9.20
    Meaning : ตำแหน่งรถแกน Z
    Unity   : ใช้ควบคุม Position Z

{TOPIC_PREFIX}/heading
    Type    : Float
    Range   : 0 - 360 degree
    Meaning : ทิศทางหัวรถ
    Unity   : Rotation Y

{TOPIC_PREFIX}/speed
    Type    : Float
    Unit    : Simulation speed
    Meaning : ความเร็วรถ
    Unity   : UI / Wheel speed

{TOPIC_PREFIX}/rpm
    Type    : Float
    Meaning : Engine RPM
    Unity   : Motor / Fan / Wheel Rotation

{TOPIC_PREFIX}/vibration
    Type    : Float
    Range   : ประมาณ 0.0 - 1.0
    Meaning : ระดับการสั่น
    Unity   : Vibration / Shake

{TOPIC_PREFIX}/temperature
    Type    : Float
    Unit    : °C
    Meaning : อุณหภูมิเครื่องยนต์
    Unity   : UI / Color / Gradient / Fade

{TOPIC_PREFIX}/fuel
    Type    : Float
    Range   : 0 - 100 %
    Meaning : ปริมาณเชื้อเพลิง
    Unity   : UI / Scale / Gauge

{TOPIC_PREFIX}/status
    Type    : String
    Values  : RUN, STOP
    Meaning : สถานะรถ
    Unity   : UI / Lamp / Material

------------------------------------------------------------
SUGGESTED UNITY MAPPING
------------------------------------------------------------

position/x + position/z
    -> Vehicle Position

heading
    -> Rotation Y

speed
    -> Wheel Rotation Speed

rpm
    -> Engine / Fan Rotation

vibration
    -> Shake

temperature
    -> Gradient Color

fuel
    -> UI / Scale

status
    -> Status Text / Lamp
============================================================
"""
import paho.mqtt.client as mqtt
import time
import math
import random
import uuid

# ==========================================
# SETTING
# ==========================================

BROKER = "broker.emqx.io"
PORT = 1883

# ให้น้องแก้ Prefix ตรงนี้
TOPIC_PREFIX = "idt/sim/car"

CLIENT_ID = "car_" + uuid.uuid4().hex[:8]

INTERVAL = 0.2


# ==========================================
# MQTT
# ==========================================

client = mqtt.Client(
    client_id=CLIENT_ID,
    protocol=mqtt.MQTTv311
)

client.connect(BROKER, PORT, 60)
client.loop_start()

print("CAR SIMULATOR")
print("Client ID :", CLIENT_ID)
print("Prefix    :", TOPIC_PREFIX)


def publish(name, value):

    topic = f"{TOPIC_PREFIX}/{name}"

    client.publish(
        topic,
        str(value)
    )


# ==========================================
# SIMULATION DATA
# ==========================================

t = 0.0

x = 0.0
z = 0.0

heading = 0.0

fuel = 100.0


try:

    while True:

        # ----------------------------------
        # SPEED
        # ----------------------------------

        speed = 30 + 25 * math.sin(t * 0.25)

        speed = max(0, speed)


        # ----------------------------------
        # HEADING
        # ----------------------------------

        heading += 8 * math.sin(t * 0.15) * INTERVAL

        heading %= 360


        # ----------------------------------
        # POSITION
        # ----------------------------------

        rad = math.radians(heading)

        distance = speed * INTERVAL * 0.02

        x += math.sin(rad) * distance
        z += math.cos(rad) * distance


        # ----------------------------------
        # RPM
        # ----------------------------------

        rpm = 700 + speed * 45

        rpm += random.uniform(-50, 50)


        # ----------------------------------
        # VIBRATION
        # ----------------------------------

        vibration = 0.05 + speed / 150

        vibration += random.uniform(-0.02, 0.02)

        vibration = max(0, vibration)


        # ----------------------------------
        # TEMPERATURE
        # ----------------------------------

        temperature = 30 + rpm / 250

        temperature += random.uniform(-0.3, 0.3)


        # ----------------------------------
        # FUEL
        # ----------------------------------

        fuel -= speed * 0.00003

        fuel = max(0, fuel)


        # ----------------------------------
        # STATUS
        # ----------------------------------

        status = "RUN" if speed > 1 else "STOP"


        # ==================================
        # MQTT PUBLISH
        # ==================================

        publish("position/x", round(x, 2))
        publish("position/z", round(z, 2))

        publish("heading", round(heading, 1))

        publish("speed", round(speed, 1))
        publish("rpm", round(rpm))

        publish(
            "vibration",
            round(vibration, 3)
        )

        publish(
            "temperature",
            round(temperature, 1)
        )

        publish("fuel", round(fuel, 1))

        publish("status", status)


        print(
            f"Speed:{speed:5.1f} "
            f"RPM:{rpm:5.0f} "
            f"Temp:{temperature:4.1f} "
            f"Pos:({x:.1f},{z:.1f})"
        )

        t += INTERVAL

        time.sleep(INTERVAL)


except KeyboardInterrupt:

    print("\nStop Car Simulator")


finally:

    client.loop_stop()
    client.disconnect()
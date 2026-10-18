"""
============================================================
BOAT MQTT SIMULATOR
============================================================
------------------------------------------------------------
MQTT TOPICS
------------------------------------------------------------

{TOPIC_PREFIX}/position/x
    Type    : Float
    Meaning : ตำแหน่งเรือแกน X
    Unity   : Position X

{TOPIC_PREFIX}/position/z
    Type    : Float
    Meaning : ตำแหน่งเรือแกน Z
    Unity   : Position Z

{TOPIC_PREFIX}/heading
    Type    : Float
    Range   : 0 - 360 degree
    Meaning : ทิศทางหัวเรือ
    Unity   : Rotation Y / Yaw

{TOPIC_PREFIX}/roll
    Type    : Float
    Unit    : degree
    Meaning : การเอียงซ้าย-ขวา
    Unity   : Rotation Z

{TOPIC_PREFIX}/pitch
    Type    : Float
    Unit    : degree
    Meaning : การก้มหรือเงยของหัวเรือ
    Unity   : Rotation X

{TOPIC_PREFIX}/wave_height
    Type    : Float
    Meaning : ระดับคลื่นจำลอง
    Unity   : ใช้เพิ่มความแรงของการโยก

{TOPIC_PREFIX}/speed
    Type    : Float
    Meaning : ความเร็วเรือ
    Unity   : UI / Motion

{TOPIC_PREFIX}/engine_rpm
    Type    : Float
    Meaning : รอบเครื่องยนต์
    Unity   : Propeller Rotation

{TOPIC_PREFIX}/temperature
    Type    : Float
    Unit    : °C
    Meaning : อุณหภูมิเครื่องยนต์
    Unity   : UI / Gradient Color

{TOPIC_PREFIX}/fuel
    Type    : Float
    Range   : 0 - 100 %
    Meaning : เชื้อเพลิง
    Unity   : UI / Scale

{TOPIC_PREFIX}/status
    Type    : String
    Example : SAILING
    Meaning : สถานะเรือ
    Unity   : UI / Status Indicator

------------------------------------------------------------
SUGGESTED UNITY MAPPING
------------------------------------------------------------

position/x + position/z
    -> Boat Position

heading
    -> Rotation Y

pitch
    -> Rotation X

roll
    -> Rotation Z

engine_rpm
    -> Propeller Rotation

wave_height
    -> Rocking Effect

temperature
    -> Color / Gradient

fuel
    -> UI / Gauge
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

TOPIC_PREFIX = "idt/sim/boat"

CLIENT_ID = "boat_" + uuid.uuid4().hex[:8]

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

print("BOAT SIMULATOR")
print("Client ID :", CLIENT_ID)
print("Prefix    :", TOPIC_PREFIX)


def publish(name, value):

    client.publish(
        f"{TOPIC_PREFIX}/{name}",
        str(value)
    )


# ==========================================
# DATA
# ==========================================

t = 0.0

x = 0.0
z = 0.0

heading = 20.0

fuel = 100.0


try:

    while True:

        # ----------------------------------
        # SPEED
        # ----------------------------------

        speed = (
            12
            + 6 * math.sin(t * 0.15)
        )

        speed = max(0, speed)


        # ----------------------------------
        # WAVE
        # ----------------------------------

        wave_height = (
            0.4
            + 0.3 * math.sin(t * 0.4)
        )


        # ----------------------------------
        # ROLL / PITCH
        # ----------------------------------

        roll = (
            math.sin(t * 1.8)
            * wave_height
            * 12
        )

        pitch = (
            math.sin(t * 1.2)
            * wave_height
            * 7
        )


        # ----------------------------------
        # HEADING
        # ----------------------------------

        heading += (
            math.sin(t * 0.1)
            * 3
            * INTERVAL
        )

        heading %= 360


        # ----------------------------------
        # POSITION
        # ----------------------------------

        rad = math.radians(heading)

        distance = speed * INTERVAL * 0.03

        x += math.sin(rad) * distance
        z += math.cos(rad) * distance


        # ----------------------------------
        # ENGINE RPM
        # ----------------------------------

        engine_rpm = (
            500
            + speed * 110
            + random.uniform(-30, 30)
        )


        # ----------------------------------
        # TEMPERATURE
        # ----------------------------------

        temperature = (
            32
            + engine_rpm / 180
        )

        temperature += random.uniform(
            -0.3,
            0.3
        )


        # ----------------------------------
        # FUEL
        # ----------------------------------

        fuel -= speed * 0.000025
        fuel = max(0, fuel)


        status = "SAILING"


        # ==================================
        # MQTT
        # ==================================

        publish("position/x", round(x, 2))
        publish("position/z", round(z, 2))

        publish(
            "heading",
            round(heading, 1)
        )

        publish("roll", round(roll, 2))
        publish("pitch", round(pitch, 2))

        publish(
            "wave_height",
            round(wave_height, 2)
        )

        publish("speed", round(speed, 1))

        publish(
            "engine_rpm",
            round(engine_rpm)
        )

        publish(
            "temperature",
            round(temperature, 1)
        )

        publish(
            "fuel",
            round(fuel, 1)
        )

        publish("status", status)


        print(
            f"Speed:{speed:4.1f} "
            f"Roll:{roll:5.1f} "
            f"Pitch:{pitch:5.1f} "
            f"Heading:{heading:5.1f}"
        )


        t += INTERVAL

        time.sleep(INTERVAL)


except KeyboardInterrupt:

    print("\nStop Boat Simulator")


finally:

    client.loop_stop()
    client.disconnect()
"""
============================================================
AGV MQTT SIMULATOR
============================================================
AGV จะวิ่งตามเส้นทางจำลองแบบสี่เหลี่ยม:
    Point 0 -> Point 1 -> Point 2 -> Point 3 -> Point 0

------------------------------------------------------------
MQTT TOPICS
------------------------------------------------------------

{TOPIC_PREFIX}/position/x
    Type    : Float
    Meaning : ตำแหน่ง AGV แกน X
    Unity   : Position X

{TOPIC_PREFIX}/position/z
    Type    : Float
    Meaning : ตำแหน่ง AGV แกน Z
    Unity   : Position Z

{TOPIC_PREFIX}/heading
    Type    : Float
    Range   : 0 - 360 degree
    Meaning : ทิศทางของ AGV
    Unity   : Rotation Y

{TOPIC_PREFIX}/speed
    Type    : Float
    Meaning : ความเร็ว AGV
    Unity   : Wheel Rotation / UI

{TOPIC_PREFIX}/battery
    Type    : Float
    Range   : 0 - 100 %
    Meaning : Battery Level
    Unity   : UI / Color / Gauge

{TOPIC_PREFIX}/load
    Type    : Float
    Range   : ประมาณ 0 - 100 %
    Meaning : Load ที่ AGV กำลังขน
    Unity   : Scale / UI / Weight Indicator

{TOPIC_PREFIX}/distance
    Type    : Float
    Meaning : ระยะทางสะสม
    Unity   : UI

{TOPIC_PREFIX}/vibration
    Type    : Float
    Meaning : ระดับการสั่น
    Unity   : Shake Effect

{TOPIC_PREFIX}/target
    Type    : Int
    Values  : 0, 1, 2, 3
    Meaning : หมายเลข Target Point ปัจจุบัน
    Unity   : UI / Target Marker

{TOPIC_PREFIX}/status
    Type    : String
    Values  : MOVING, LOW_BATTERY
    Meaning : สถานะ AGV
    Unity   : UI / Warning Lamp / Material

------------------------------------------------------------
SUGGESTED UNITY MAPPING
------------------------------------------------------------

position/x + position/z
    -> AGV Position

heading
    -> Rotation Y

speed
    -> Wheel Rotation

battery
    -> UI + Gradient

load
    -> Scale / UI

vibration
    -> Shake

target
    -> Target UI

status
    -> Warning Indicator
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

TOPIC_PREFIX = "idt/sim/agv"

CLIENT_ID = "agv_" + uuid.uuid4().hex[:8]

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

print("AGV SIMULATOR")
print("Client ID :", CLIENT_ID)
print("Prefix    :", TOPIC_PREFIX)


def publish(name, value):

    client.publish(
        f"{TOPIC_PREFIX}/{name}",
        str(value)
    )


# ==========================================
# PATH
# ==========================================

path = [
    (0, 0),
    (8, 0),
    (8, 6),
    (0, 6)
]

target_index = 1

x = 0.0
z = 0.0

speed = 1.5

battery = 100.0

load = 20.0

total_distance = 0.0

t = 0.0


try:

    while True:

        target_x, target_z = (
            path[target_index]
        )

        dx = target_x - x
        dz = target_z - z

        distance_to_target = math.sqrt(
            dx * dx + dz * dz
        )


        # ----------------------------------
        # TARGET REACHED
        # ----------------------------------

        if distance_to_target < 0.15:

            target_index += 1

            if target_index >= len(path):
                target_index = 0

            target_x, target_z = (
                path[target_index]
            )

            dx = target_x - x
            dz = target_z - z

            distance_to_target = math.sqrt(
                dx * dx + dz * dz
            )


        # ----------------------------------
        # HEADING
        # ----------------------------------

        heading = math.degrees(
            math.atan2(dx, dz)
        )

        if heading < 0:
            heading += 360


        # ----------------------------------
        # MOVE
        # ----------------------------------

        if distance_to_target > 0:

            nx = dx / distance_to_target
            nz = dz / distance_to_target

            move_distance = (
                speed * INTERVAL
            )

            move_distance = min(
                move_distance,
                distance_to_target
            )

            x += nx * move_distance
            z += nz * move_distance

            total_distance += move_distance


        # ----------------------------------
        # BATTERY
        # ----------------------------------

        battery -= (
            speed *
            INTERVAL *
            0.003
        )

        battery = max(0, battery)


        # ----------------------------------
        # VIBRATION
        # ----------------------------------

        vibration = (
            0.05
            + speed * 0.04
            + random.uniform(
                -0.01,
                0.01
            )
        )


        # ----------------------------------
        # LOAD
        # ----------------------------------

        load = (
            50
            + 30 *
            math.sin(t * 0.08)
        )

        load = max(0, load)


        # ----------------------------------
        # STATUS
        # ----------------------------------

        if battery <= 5:

            status = "LOW_BATTERY"

        else:

            status = "MOVING"


        # ==================================
        # MQTT
        # ==================================

        publish(
            "position/x",
            round(x, 2)
        )

        publish(
            "position/z",
            round(z, 2)
        )

        publish(
            "heading",
            round(heading, 1)
        )

        publish(
            "speed",
            round(speed, 2)
        )

        publish(
            "battery",
            round(battery, 1)
        )

        publish(
            "load",
            round(load, 1)
        )

        publish(
            "distance",
            round(
                total_distance,
                2
            )
        )

        publish(
            "vibration",
            round(
                vibration,
                3
            )
        )

        publish(
            "target",
            target_index
        )

        publish(
            "status",
            status
        )


        print(
            f"Pos:({x:.2f},{z:.2f}) "
            f"Heading:{heading:5.1f} "
            f"Battery:{battery:5.1f}% "
            f"Target:{target_index}"
        )


        t += INTERVAL

        time.sleep(INTERVAL)


except KeyboardInterrupt:

    print("\nStop AGV Simulator")


finally:

    client.loop_stop()
    client.disconnect()
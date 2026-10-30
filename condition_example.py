import paho.mqtt.client as mqtt

broker = "broker.emqx.io"
port = 1883

topic_input1 = "topic_in"
topic_output1 = "topic_out"

client = mqtt.Client()


def on_connect(client, userdata, flags, rc):
    print("Connected")
    client.subscribe(topic_input1)


def on_message(client, userdata, msg):

    value = float(msg.payload.decode())

    print("Value =", value)

    if value < 0:
        client.publish(topic_output1, "LOW")




client.on_connect = on_connect
client.on_message = on_message

client.connect(
    broker,
    port,
    60
)

client.loop_forever()

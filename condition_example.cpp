float temperature = dht.readTemperature();

if (!isnan(temperature))
{
    Serial.print("Temperature : ");
    Serial.println(temperature);

    String state;

    if (temperature < 25)
    {
        state = "LOW";
    }

    String topicCondition =
        String(TOPIC_PREFIX) + "/condition";

    mqttClient.publish(
        topicCondition.c_str(),
        state.c_str()
    );

    Serial.print("Condition : ");
    Serial.println(state);
}

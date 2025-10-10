# ------------------------------------------------------------
# Environment Simulator → MQTT (paho-mqtt)
# Base topic: idt/*
#
# Topics & Units & Normal Ranges (approx):
# - idt/temp   : °C        (Normal ~ 24–34)
# - idt/humi   : %RH       (Normal ~ 45–85)
# - idt/rain   : mm/hr     (Normal ~ 0–5; Shower/Storm 10–60)
# - idt/water  : m         (River/Canal level) (Normal ~ 0.2–1.2; Flood > 1.5)
# - idt/storm  : scale 0-10 (Composite storm intensity) (Normal 0–2; Storm > 5)
# - idt/wave   : m         (Wave height) (Normal ~ 0.2–1.0; Storm 1.5–4.0)
# - idt/quake  : Mw        (Richter moment magnitude) (Normal ~ 0; Event 3.0–6.0)
# - idt/dust   : µg/m³     (PM2.5) (Normal ~ 10–50; Haze 80–200)
#
# Time scale:
#   TIME_SCALE_DAYS_PER_SEC = 1/5  → 5 seconds = 1 day (default)
#   Adjust by changing REAL_SECONDS_PER_SIM_DAY below.
#
# Anomaly model:
#   - Each variable has a Poisson-like chance to start an anomaly per sim-day
#   - Anomaly has random duration (in sim-hours/sim-days) and magnitude
#   - Natural variation = daily sinusoid + random walk
# ------------------------------------------------------------

import time, json, math, random
from dataclasses import dataclass, field
from typing import Optional, Tuple
import paho.mqtt.client as mqtt

# ----------------- CONFIG -----------------
MQTT_BROKER = "broker.emqx.io"
MQTT_PORT   = 1883
MQTT_CLIENT_ID = "idt_env_sim_001"
TOPIC_BASE  = "idt_env"

# Timing
REAL_SECONDS_PER_SIM_DAY = 5.0   # ← 5 real seconds = 1 simulated day (adjust as you like)
PUB_HZ = 2.0                      # publish rate (times per real second)
DT_REAL = 1.0 / PUB_HZ            # real seconds per step

# Random seed (fix for reproducibility if needed)
# random.seed(42)
# -----------------------------------------


@dataclass
class AnomalyState:
    active: bool = False
    time_left_sim_hours: float = 0.0
    magnitude: float = 0.0
    kind: str = ""


@dataclass
class VarSim:
    name: str
    unit: str
    topic: str
    base_mean: float
    daily_amp: float
    noise_sigma: float
    normal_range: Tuple[float, float]
    # anomaly params
    start_rate_per_sim_day: float   # avg starts/day (e.g., 0.3 => about once every ~3.3 days)
    dur_hours_range: Tuple[float, float]
    mag_range: Tuple[float, float]
    signed: bool = True             # if True, anomaly can be positive or negative
    clamp: Optional[Tuple[float, float]] = None
    # internal state
    baseline_walk: float = 0.0
    anomaly: AnomalyState = field(default_factory=AnomalyState)

    def maybe_start_anomaly(self, sim_dt_days: float):
        """Poisson chance per timestep to start anomaly."""
        if self.anomaly.active:
            return
        # Probability of at least one start in this small window:
        p = self.start_rate_per_sim_day * sim_dt_days
        if random.random() < p:
            self.anomaly.active = True
            self.anomaly.time_left_sim_hours = random.uniform(*self.dur_hours_range)
            mag = random.uniform(*self.mag_range)
            if self.signed and random.random() < 0.5:
                mag *= -1.0
            self.anomaly.magnitude = mag
            # simple kind tag
            self.anomaly.kind = "spike" if abs(mag) > 0 else "event"

    def step(self, sim_time_days: float, sim_dt_days: float):
        """Update variable value for current timestep."""
        # Natural daily cycle: sin(2π * day_fraction)
        day_fraction = sim_time_days % 1.0
        diurnal = self.daily_amp * math.sin(2*math.pi*day_fraction - math.pi/2)
        # Random walk (very gentle)
        self.baseline_walk += random.gauss(0, self.noise_sigma * 0.05)

        value = self.base_mean + diurnal + self.baseline_walk
        # Small short-term noise
        value += random.gauss(0, self.noise_sigma)

        # Anomaly handling
        status = "normal"
        if self.anomaly.active:
            # apply anomaly magnitude
            value += self.anomaly.magnitude
            self.anomaly.time_left_sim_hours -= sim_dt_days * 24.0
            status = "anomaly"
            if self.anomaly.time_left_sim_hours <= 0:
                self.anomaly = AnomalyState()  # reset

        # Clamp if needed
        if self.clamp:
            low, high = self.clamp
            value = max(low, min(high, value))

        # For dashboarding, also tell if inside normal range
        inside = (self.normal_range[0] <= value <= self.normal_range[1])
        if status == "normal" and not inside:
            # drifted outside normal due to random walk — mark as mild deviation
            status = "deviation"

        return value, status


def make_variables():
    vars = []

    # Temperature (°C)
    vars.append(VarSim(
        name="temp", unit="°C", topic=f"{TOPIC_BASE}/temp",
        base_mean=30.0, daily_amp=4.0, noise_sigma=0.3,
        normal_range=(24.0, 34.0),
        start_rate_per_sim_day=0.35, dur_hours_range=(6, 48), mag_range=(3, 10),
        signed=True, clamp=(10.0, 50.0)
    ))

    # Humidity (%RH)
    vars.append(VarSim(
        name="humi", unit="%RH", topic=f"{TOPIC_BASE}/humi",
        base_mean=65.0, daily_amp=15.0, noise_sigma=1.5,
        normal_range=(45.0, 85.0),
        start_rate_per_sim_day=0.25, dur_hours_range=(6, 36), mag_range=(8, 20),
        signed=True, clamp=(10.0, 100.0)
    ))

    # Rain (mm/hr) — mostly near 0; anomaly gives showers/storms
    vars.append(VarSim(
        name="rain", unit="mm/hr", topic=f"{TOPIC_BASE}/rain",
        base_mean=0.2, daily_amp=0.3, noise_sigma=0.2,
        normal_range=(0.0, 5.0),
        start_rate_per_sim_day=0.4, dur_hours_range=(1, 6), mag_range=(5, 40),
        signed=False, clamp=(0.0, 80.0)
    ))

    # Water level (m)
    vars.append(VarSim(
        name="water", unit="m", topic=f"{TOPIC_BASE}/water",
        base_mean=0.7, daily_amp=0.2, noise_sigma=0.03,
        normal_range=(0.2, 1.2),
        start_rate_per_sim_day=0.2, dur_hours_range=(12, 72), mag_range=(0.3, 1.2),
        signed=True, clamp=(0.0, 3.0)
    ))

    # Storm intensity (0–10)
    vars.append(VarSim(
        name="storm", unit="scale", topic=f"{TOPIC_BASE}/storm",
        base_mean=0.5, daily_amp=0.6, noise_sigma=0.2,
        normal_range=(0.0, 2.0),
        start_rate_per_sim_day=0.25, dur_hours_range=(3, 18), mag_range=(2.5, 7.0),
        signed=False, clamp=(0.0, 10.0)
    ))

    # Wave height (m)
    vars.append(VarSim(
        name="wave", unit="m", topic=f"{TOPIC_BASE}/wave",
        base_mean=0.6, daily_amp=0.4, noise_sigma=0.08,
        normal_range=(0.2, 1.0),
        start_rate_per_sim_day=0.25, dur_hours_range=(3, 18), mag_range=(1.0, 3.0),
        signed=False, clamp=(0.0, 6.0)
    ))

    # Earthquake (Mw) — rare spikes
    vars.append(VarSim(
        name="quake", unit="Mw", topic=f"{TOPIC_BASE}/quake",
        base_mean=0.0, daily_amp=0.0, noise_sigma=0.0,
        normal_range=(0.0, 0.1),
        start_rate_per_sim_day=0.05, dur_hours_range=(0.1, 1.0), mag_range=(3.0, 6.0),
        signed=False, clamp=(0.0, 9.9)
    ))

    # Dust PM2.5 (µg/m³)
    vars.append(VarSim(
        name="dust", unit="µg/m³", topic=f"{TOPIC_BASE}/dust",
        base_mean=30.0, daily_amp=10.0, noise_sigma=2.0,
        normal_range=(10.0, 50.0),
        start_rate_per_sim_day=0.3, dur_hours_range=(6, 48), mag_range=(40.0, 120.0),
        signed=False, clamp=(0.0, 500.0)
    ))

    return vars


def main():
    client = mqtt.Client(client_id=MQTT_CLIENT_ID, clean_session=True)
    client.connect(MQTT_BROKER, MQTT_PORT, keepalive=30)
    client.loop_start()

    vars = make_variables()

    # Publish a meta time tick (optional)
    meta_topic = f"{TOPIC_BASE}/meta/time"

    sim_time_days = 0.0
    last_pub = time.time()

    print(f"Running… broker={MQTT_BROKER}:{MQTT_PORT} base={TOPIC_BASE}")
    print(f"Time scale: {REAL_SECONDS_PER_SIM_DAY:.3f} real-sec per sim-day")

    try:
        while True:
            now = time.time()
            # Map real dt -> sim dt
            real_dt = now - last_pub
            if real_dt < DT_REAL:
                time.sleep(DT_REAL - real_dt)
                now = time.time()
                real_dt = now - last_pub
            last_pub = now

            sim_dt_days = real_dt / REAL_SECONDS_PER_SIM_DAY
            sim_time_days += sim_dt_days

            # Publish meta time (simulated clock)
            sim_days_total = sim_time_days
            sim_day_idx = int(sim_days_total)
            sim_day_fraction = sim_days_total % 1.0
            sim_hour = int(sim_day_fraction * 24.0)
            sim_minute = int((sim_day_fraction * 24.0 - sim_hour) * 60.0)

            meta_payload = {
                "sim_day": sim_day_idx,
                "sim_time_hhmm": f"{sim_hour:02d}:{sim_minute:02d}",
                "sim_time_days": round(sim_days_total, 5),
                "ts": int(time.time()*1000)
            }
            client.publish(meta_topic, json.dumps(meta_payload), qos=0, retain=False)

            # Step & publish each variable
            for v in vars:
                v.maybe_start_anomaly(sim_dt_days)
                val, status = v.step(sim_time_days, sim_dt_days)

                payload = {
                    "name": v.name,
                    "value": round(val, 3),
                    "unit": v.unit,
                    "status": status,   # "normal" | "deviation" | "anomaly"
                    "normal_low": v.normal_range[0],
                    "normal_high": v.normal_range[1],
                    "sim_day": sim_day_idx,
                    "sim_time_hhmm": f"{sim_hour:02d}:{sim_minute:02d}",
                    "ts": int(time.time()*1000)
                }
                client.publish(v.topic, json.dumps(payload), qos=0, retain=False)

            # loop pacing
            # (we already slept to respect PUB_HZ above)

    except KeyboardInterrupt:
        print("Stopping simulator...")
    finally:
        client.loop_stop()
        client.disconnect()


if __name__ == "__main__":
    main()

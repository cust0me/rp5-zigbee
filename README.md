# Raspberry PI 5

Devices used for this project:<br>
[Raspberry PI 5 ](https://raspberrypi.dk/produkt/raspberry-pi-5-4-gb/)<br>
*Sonoff ZigBee USB Dongle CC2531* is now deprecated, but it's successor is [here](https://sonoff.tech/product/gateway-and-sensors/sonoff-zigbee-3-0-usb-dongle-plus-p/)<br>
[Aqara Temperature And Humidity Sensor T1](https://www.aqara.com/eu/product/temperature-humidity-sensor/)

This project shows how you can get a Raspberry PI 5 4GB ARM64 to host a ZigBee network., which integrates with a .NET API which stores data in a influxdb database, and then a UI to display the collected telemetric data.

**Prerequisites**:<br>
The Raspberry PI 5 is installed using Raspberry PI Imager, the chosen operation system is **Pi OS Lite 64bit**

1. After first initial start, run:

    ```
    sudo apt-get update
    ```

2. After all repositories are updated, run:

    ```
    sudo apt-get upgrade
    ```

## Docker

1. Run the following command to download the docker install script from the official docker website: 

    ```bash
    curl -fsSL https://get.docker.com -o get-docker.sh  
    ```
2. Make the script executable by running the following: 

    ```bash
    chmod +x get-docker.sh
    ```

3. Run the script:

    ```bash
    ./get-docker.sh
    ```

4. Start docker after installation:

    ```bash
    sudo systemctl start docker
    ```

5. Give your logged in user access to the docker usergroup:

    ```bash
    sudo usermod -a -G docker <username>
    ```

6. Reboot your Raspberry PI:

    ```bash
    sudo reboot
    ```

## Pulling the repository down to your PI

You need to clone the repository, and that can be done by running:

```bash
git clone https://github.com/cust0me/rp5-zigbee.git
```

Now, navigate into your repository:

```bash
cd rp5-zigbee
```

## Detecting USB

1. Run: ``lsusb`` to see active USB Devices.
    1. Example output:
    ```bash
    andrias@andrias:~ $ lsusb
    Bus 004 Device 001: ID 1d6b:0003 Linux Foundation 3.0 root hub
    Bus 003 Device 002: ID 0451:16a8 Texas Instruments, Inc. CC2531 ZigBee
    Bus 003 Device 001: ID 1d6b:0002 Linux Foundation 2.0 root hub
    Bus 002 Device 001: ID 1d6b:0003 Linux Foundation 3.0 root hub
    Bus 001 Device 001: ID 1d6b:0002 Linux Foundation 2.0 root hub
    ```

    2. We can see that our ZigBee dongle is on `Bus 0003` with the name: `Texas Instruments, Inc. CC2531 ZigBee`
    3. Search for it in the /dev/serial/by-id/ directory: `ls /dev/serial/by-id/`
        I get the following output: 
        ```bash
        andrias@andrias:/ $ ls /dev/serial/by-id/
        usb-Texas_Instruments_TI_CC2531_USB_CDC___0X00124B001DF40392-if00
        ```

    4. No combine the `/dev/serial/by-id/` and `usb-Texas_Instruments_TI_CC2531_USB_CDC___0X00124B001DF40392-if00` to:

    ```bash
    /dev/serial/by-id/usb-Texas_Instruments_TI_CC2531_USB_CDC___0X00124B001DF40392-if00
    ```

    This is now the ful path to the zigbee usb dongle.

## Configuring the setup

The configuration is across multiple files: `.env` and `./zigbee2mqtt/configuration.yml` file.

The most important configuration you have to do is change the `ZIGBEE_ADAPTER` in the `.env` file to the full usb path detailed in `Detecting USB 1.4`

| KEY                     | Default Value         | Description                                                                                                                           |
|-------------------------|-----------------------|---------------------------------------------------------------------------------------------------------------------------------------|
| RABBITMQ_USER           | zigbee2mqtt           | The default Username for RabbitMQ <br> **N.B.** Changing this value requires additional changes in `./zigbee2mqtt/configuration.yml`  |
| RABBITMQ_PASSWORD       | zigbee2mqtt           | The default Passowrd for RabbitMQ <br> **N.B.**  Changing this value requires additional changes in `./zigbee2mqtt/configuration.yml` |
| ZIGBEE_ADAPTER          |                       | The full USB path for the ZigBee dongle                                                                                               |
| INFLUXDB_ADMIN_USER     | admin                 | The default Username for InfluxDB                                                                                                     |
| INFLUXDB_ADMIN_PASSWORD | supersecret           | The default Password for InfluxDB                                                                                                     |
| INFLUXDB_ADMIN_TOKEN    | supersecretadmintoken | The default Token for InfluxDB (Keep this safe and secure)                                                                            |
| INFLUXDB_ORG            | rp5org                | The default Organization for InfluxDB                                                                                                 |
| INFLUXDB_BUCKET         | Telemetry             | The default Bucket for InfluxDB                                                                                                       |
| API_PORT                | 5000                  | The port the API is exposed on                                                                                                        |

**N.B.** It is important that `RABBITMQ_USER` and `RABBITMQ_PASSWORD` in the `.env` file match `user` and `password` in the `./zigbee2mqtt/configuration.yml` file.

## Running the setup on the Raspberry PI

After the configuration is done, all you have to do on your raspberry pi is to run: `docker compose up -d`

The containers will the be spun up, and can be reached at:

| Service             | Launch url                  |
|---------------------|-----------------------------|
| Zigbee2MQTT         | http://<hostaddress\>:8080  |
| RabbitMQ Management | http://<hostaddress\>:15672 |
| InfluxDB            | http://<hostaddress\>:8086  |
| API                 | http://<hostaddress\>:5000  |
| UI                  | https://localhost:5002      |

**N.B.** `hostaddress` is the one you set when you installed your raspberry pi, or you could replace it with the pi's ip-address.

## Adopting the Aqara Telemetry and Humidity sensor

1. Open Zigbee2MQTT at the url in the table above
2. Click on **Permit Join (All)** at the top center, this puts the ZigBee usb coordinator into adoption mode
3. On your Aqara Temperature and Humidity Sensor T1, hold on the button on top for about 5 seconds until it blinks.
4. You should see the device being added in the UI, and once it's recognized as a Aqara click on the Friendly Name e.g. `0x54ef44100091a459`
5. Go to Reporting
    1. You need to configure reporting for Temperature, Humidity and Pressure, as it does not do it automatically.
    2. Temperature:
        1. Select `1` in the Endpoint drop down
        2. Select `Temperature` in the cluster drop down
        3. Select `measuredValue` in the attribute drop down
        4. Click Apply, after a few seconds there should appear a new blank row, repeat the steps 5.2.1-4 but for Humidity and pressure aswell.
6. Verify everything is working by going to the dashboard, and after up to a minute, you should start seeing telemetric data.

<br>

You can now try and execute a api request: `http://<hostaddress>:5000/telemetry/latest`

You should see something similar to this:

```json
{
    "temperature": 25.33,
    "humidity": 48.39,
    "pressure": 1008,
    "_time": "2025-06-02T07:32:03.6626492Z",
    "device": "0x54ef44100091a459"
}
```

## Running the UI

On your own machine, clone the repository: 

```bash
git clone https://github.com/cust0me/rp5-zigbee.git
```

Now, navigate into your repository:

```bash
cd rp5-zigbee
```

Now, navigate into your RP5.Web:

```bash
cd RP5.Web
```

Run the UI:
```bash
dotnet run -c release
```

The UI is now hosted here: [https://localhost:5002](https://localhost:5002)
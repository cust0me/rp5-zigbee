# Raspberry PI 5

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

## RabbitMQ

1. Starting an instance of RabbitMQ using Docker:

    ```bash
    docker run -it -d --name rabbitmq -p 5672:5672 -p 15672:15672 -p 1883:1883 rabbitmq:4.1-management
    ```

2. Enabling MQTT plugin on RabbitMQ
    1. Entering the container
        ```bash
        docker exec -it rabbitmq bash
        ```
    2. Enabling MQTT plugin
        ```bash
        rabbitmq-plugins enable rabbitmq_mqtt
        ```
    3. Exiting the container
        ```bash
        exit
        ```

3. RabbitMQ ManagementUI should now be available at the pi's ``hostname/ipaddress:15672``
    1. The default login is: guest:guest

## InfluxDB

1. Starting an instance of InfluxDB using Docker:

    ```
    docker run -d -p 8086:8086 -v influxdb:/var/lib/influxdb -v influxdb2:/var/lib/influxdb2 influxdb:2.0
    ```

2. You can access the InfluxDB management portal by going to: ``hostname/ipaddress:8086``
    1. Here you will create a default user: administrator
    2. Set a secure password
    3. Set an initial organization name: e.g. AndriasOrg
    4. Set the bucket name to e.g. Telemetry

3. Establishing a connection:
    1. Getting the address: that address is the same as you connect to in the browser: ``hostname/ipaddress:8086``
    2. Getting the token: 
        1. Go to the data tab on the left side
        2. Select token section 
        3. Generate a new full access token and copy the value
    3. Getting the Organization ID:
        1. Click on your Avatar
        2. Click on About
        3. Copy the Organization ID

    4. Getting the bucket name:
        1. Go to the data tab on the left side
        2. Click on buckets
        3. Create a new one, or use the already created ``Telemetry`` bucket.


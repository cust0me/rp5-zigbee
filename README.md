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

2. See last connected USB Devices: ``dmesg | grep -i tty``
    1. Example output:
    ```bash
    andrias@andrias:~ $ dmesg | grep -i tty
    [    0.000000] Kernel command line: reboot=w coherent_pool=1M 8250.nr_uarts=1 pci=pcie_bus_safe cgroup_disable=memory numa_policy=interleave  numa=fake=8 system_heap.max_order=0 smsc95xx.macaddr=2C:CF:67:76:DF:80 vc_mem.mem_base=0x3fc00000 vc_mem.mem_size=0x40000000  console=ttyAMA10,115200 console=tty1 root=PARTUUID=a46c75d5-02 rootfstype=ext4 fsck.repair=yes rootwait cfg80211.ieee80211_regdom=DK
    [    0.000095] printk: legacy console [tty1] enabled
    [    0.056213] 107d001000.serial: ttyAMA10 at MMIO 0x107d001000 (irq = 16, base_baud = 0) is a PL011 rev3
    [    0.056223] printk: legacy console [ttyAMA10] enabled
    [    1.632335] 107d50c000.serial: ttyS0 at MMIO 0x107d50c000 (irq = 33, base_baud = 6000000) is a Broadcom BCM7271 UART
    [    1.642944] serial serial0: tty port ttyS0 registered
    [    4.005888] systemd[1]: Created slice system-getty.slice - Slice /system/getty.
    [    4.021771] systemd[1]: Created slice system-serial\x2dgetty.slice - Slice /system/serial-getty.
    [    4.092730] systemd[1]: Expecting device dev-ttyAMA10.device - /dev/ttyAMA10...
    [15788.075443] cdc_acm 3-1:1.0: ttyACM0: USB ACM device
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
import logging
import time
import uuid
import socket

import Adafruit_BluefruitLE


# Enable debug output.
logging.basicConfig(level=logging.DEBUG)

# Define service and characteristic UUIDs used by the IMU service.
IMU_SERVICE_UUID = uuid.UUID('0000A000-0000-1000-8000-00805F9B34FB')
W_CHAR_UUID      = uuid.UUID('0000A001-0000-1000-8000-00805F9B34FB')
X_CHAR_UUID      = uuid.UUID('0000A002-0000-1000-8000-00805F9B34FB')
Y_CHAR_UUID      = uuid.UUID('0000A003-0000-1000-8000-00805F9B34FB')
Z_CHAR_UUID      = uuid.UUID('0000A004-0000-1000-8000-00805F9B34FB')

# Define the host and port
HOST = '' # localhost
PORT = 33000

# Get the BLE provider for the current platform.
ble = Adafruit_BluefruitLE.get_provider()

# Initialize the socket
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((HOST, PORT))
s.listen(1)

# Define our quaternion
quat = [0, 0, 0, 0] # (w, x, y, z)

# Main function implements the program logic so it can run in a background
# thread.  Most platforms require the main thread to handle GUI events and other
# asyncronous events like BLE actions.  All of the threading logic is taken care
# of automatically though and you just need to provide a main function that uses
# the BLE provider.
def main():
    #connection, address = s.accept()
    # Clear any cached data because both bluez and CoreBluetooth have issues with
    # caching data and it going stale.
    ble.clear_cached_data()

    # Get the first available BLE network adapter and make sure it's powered on.
    adapter = ble.get_default_adapter()
    adapter.power_on()
    print('Using adapter: {0}'.format(adapter.name))

    # Disconnect any currently connected IMU devices.  Good for cleaning up and
    # starting from a fresh state.
    print('Disconnecting any connected IMU devices...')
    ble.disconnect_devices([IMU_SERVICE_UUID])

    # Scan for IMU devices.
    print('Searching for IMU device...')
    try:
        adapter.start_scan()
        # Search for the first IMU device found (will time out after 60 seconds
        # but you can specify an optional timeout_sec parameter to change it).
        device = ble.find_device(service_uuids=[IMU_SERVICE_UUID])
        if device is None:
            raise RuntimeError('Failed to find IMU device!')
    finally:
        # Make sure scanning is stopped before exiting.
        adapter.stop_scan()

    print('Connecting to device...')
    device.connect()  # Will time out after 60 seconds, specify timeout_sec parameter
                      # to change the timeout.

    # Once connected do everything else in a try/finally to make sure the device
    # is disconnected when done.
    try:
        # Wait for service discovery to complete for at least the specified
        # service and characteristic UUID lists.  Will time out after 60 seconds
        # (specify timeout_sec parameter to override).
        print('Discovering services...')
        device.discover([IMU_SERVICE_UUID], [W_CHAR_UUID, X_CHAR_UUID, Y_CHAR_UUID, Z_CHAR_UUID])

        # Find the UART service and its characteristics.
        imu = device.find_service(IMU_SERVICE_UUID)
        w = imu.find_characteristic(W_CHAR_UUID)
        x = imu.find_characteristic(X_CHAR_UUID)
        y = imu.find_characteristic(Y_CHAR_UUID)
        z = imu.find_characteristic(Z_CHAR_UUID)

        # Functions to receive characteristics changes.  Note that this will
        # be called on a different thread so be careful to make sure state that
        # the function changes is thread safe.  Use Queue or other thread-safe
        # primitives to send data to other threads.
        def w_receiver(data):
            print('W : {0}'.format(data))
            quat[0]= data
            #connection.sendall(quat)

        def x_receiver(data):
            print('X : {0}'.format(data))
            quat[1] = data
            #connection.sendall(quat)

        def y_receiver(data):
            print('Y : {0}'.format(data))
            #connection.sendall(quat)

        def z_receiver(data):
            print('Z : {0}'.format(data))
            #connection.sendall(quat)


        # Turn on notification of characteristics using the callback above.
        print('Subscribing to characteristics changes...')
        w.start_notify(w_receiver)
        x.start_notify(x_receiver)
        y.start_notify(y_receiver)
        z.start_notify(z_receiver)

        # Now just wait for 30 seconds to receive data.
        print('Waiting 60 seconds to receive data from the device...')
        time.sleep(60)
    finally:
        # Make sure device is disconnected on exit.
        device.disconnect()


# Initialize the BLE system.  MUST be called before other BLE calls!
print("Initializing BLE")
ble.initialize()

# Start the mainloop to process BLE events, and run the provided function in
# a background thread.  When the provided main function stops running, returns
# an integer status code, or throws an error the program will exit.
print("Running main loop")
ble.run_mainloop_with(main)

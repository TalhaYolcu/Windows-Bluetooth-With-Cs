using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace QuickBlueToothLE
{
    class Program
    {
        static DeviceInformation device = null;

        static async Task Main(string[] args)
        {
            // Query for extra properties you want returned
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            DeviceWatcher deviceWatcher =
                        DeviceInformation.CreateWatcher(
                                BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                                requestedProperties,
                                DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;

            // EnumerationCompleted and Stopped are optional to implement.
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start the watcher.
            deviceWatcher.Start();
            GattSession session = null;
            while (true)
            {
                if (device == null)
                {
                    Thread.Sleep(200);
                }
                else
                {

                    Console.WriteLine("Press Any to pair with BILGEM");
                    Console.ReadKey();
                    BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(device.Id);
                    //BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(115100448267000);

                    //115100448267000
                    Console.WriteLine("Attempting to pair with device");
                    GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync();

                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        Console.WriteLine("Pairing succeeded");
                        Console.WriteLine(bluetoothLeDevice.BluetoothAddress.ToString());


                        try
                        {
                            BluetoothDeviceId bleId = null;
                            bleId = BluetoothDeviceId.FromId(device.Id);

                            Console.WriteLine("Waiting for you to accept pairing on the module");
                            Console.ReadKey();

                            session = await GattSession.FromDeviceIdAsync(bleId);
                            session.MaxPduSizeChanged += GattSession_MaxPduSizeChanged;

                            if (session != null)
                            {
                                Console.WriteLine("Max pdu size : " + session.MaxPduSize);

                            }
                            //115100448267000
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to create Gatt Session");
                        }

                        break;
                    }
                    else
                    {
                        Console.WriteLine("Error on pairing");
                    }

                }
            }
            Thread.Sleep(10000);

            deviceWatcher.Stop();

            for(int i =0;i<100;i++)
            {
                Console.WriteLine("Max pdu size : " + session.MaxPduSize);
                Thread.Sleep(200);

            }

        }

        private static void GattSession_MaxPduSizeChanged(GattSession sender, object args)
        {
            Console.WriteLine(args.ToString());
        }

        private static void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            var flags = reader.ReadByte();
            var value = reader.ReadByte();
            Console.WriteLine($"{flags} - {value}");
        }

        private static void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            //throw new NotImplementedException();
        }

        private static void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            //throw new NotImplementedException();
        }

        private static void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //throw new NotImplementedException();
        }

        private static void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //throw new NotImplementedException();
        }

        private static void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            Console.WriteLine(args.Name);
            if (args.Name == "BILGEM-ELCI-ELCI-0014")
                device = args;
            //throw new NotImplementedException();
        }


    }
}
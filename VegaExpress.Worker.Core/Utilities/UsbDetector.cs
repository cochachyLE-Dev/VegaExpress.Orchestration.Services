using System.Management;
using System.Timers;

using Timer = System.Timers.Timer;

namespace VegaExpress.Worker.Core.Utilities
{
    public class UsbDetector
    {
        private static Timer USBAttachedDebounceTimer, USBDetachedDebounceTimer;
        private static bool USBAttachedEventProcessed = false, USBDetachedEventProcessed = false;
        private IEnumerable<Disk> diskDrives { get; set; }
#if WINDOWS
        private ManagementEventWatcher watcherAttach;
        private ManagementEventWatcher watcherDetach;

        public UsbDetector()
        {
            watcherAttach = new ManagementEventWatcher();
            watcherAttach.EventArrived += new EventArrivedEventHandler(OnUSBAttached);
            watcherAttach.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");

            watcherDetach = new ManagementEventWatcher();
            watcherDetach.EventArrived += new EventArrivedEventHandler(OnUSBDetached);
            watcherDetach.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
        }

        public void Start()
        {
            diskDrives = GetDiskDrives().ToList();
            //var partitions = diskDrives.First().Partitions.ToList();
            //var logicalDrives = partitions[0].LogicalDisks.ToList();

            watcherAttach.Start();
            watcherDetach.Start();
        }

        public void Stop()
        {
            watcherAttach.Stop();
            watcherDetach.Stop();
        }

        private IEnumerable<Disk> GetDiskDrives()
        {
            string query = $"SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'"; //WHERE InterfaceType='USB'
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (var drive in searcher.Get())
            {
                yield return new Disk
                {
                    DeviceID = drive["DeviceId"]?.ToString(),
                    SerialNumber = drive["SerialNumber"]?.ToString(),
                    Manufacturer = drive["Manufacturer"]?.ToString(),
                    Model = drive["Model"]?.ToString(),
                    InterfaceType = drive["InterfaceType"]?.ToString(),
                    Size = Convert.ToUInt64(drive["Size"]),
                    Partitions = PartitionSearcher(drive["DeviceID"])
                };
            }
            /// Obtener las particiones asociadas al disco actual
            IEnumerable<Partition> PartitionSearcher(object deviceID)
            {
                var partitionQuery = $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{deviceID}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition";
                ManagementObjectSearcher partitionSearcher = new ManagementObjectSearcher(partitionQuery);

                foreach (ManagementObject partition in partitionSearcher.Get())
                {
                    yield return new Partition
                    {
                        DeviceID = partition["DeviceID"]?.ToString(),
                        Size = Convert.ToUInt64(partition["size"]),
                        Type = partition["Type"]?.ToString(),
                        NumberOfBlock = partition["NumberOfBlocks"]?.ToString(),
                        StartingOffset = Convert.ToUInt64(partition["StartingOffset"]),
                        LogicalDisks = LogicalDiskSearcher(partition["DeviceID"])
                    };
                }
            }
            /// Obtener las letras de unidad asociadas a las particiones
            IEnumerable<LogicalDisk> LogicalDiskSearcher(object deviceID)
            {
                var logicalDiskQuery = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{deviceID}'}} WHERE AssocClass=Win32_LogicalDiskToPartition";
                ManagementObjectSearcher logicalDiskSearcher = new ManagementObjectSearcher(logicalDiskQuery);

                foreach (ManagementObject logicalDisk in logicalDiskSearcher.Get())
                {
                    yield return new LogicalDisk
                    {
                        DeviceID = logicalDisk["DeviceID"]?.ToString(),
                        FileSystem = logicalDisk["FileSystem"]?.ToString(),
                        FreeSpace = Convert.ToUInt64(logicalDisk["FreeSpace"]),
                        Size = Convert.ToUInt64(logicalDisk["Size"])
                    };
                }
            }
        }

        private void OnUSBAttached(object sender, EventArrivedEventArgs e)
        {
            if (USBAttachedEventProcessed)
            {
                return;
            }

            USBAttachedEventProcessed = true;

            var attachedDiskDrives = GetDiskDrives().Except(diskDrives);

            foreach (var item in attachedDiskDrives)
            {
                Console.WriteLine("USB device {0} attached", item.DeviceID);
            }

            // Reiniciar el temporizador
            USBAttachedDebounceTimer = new Timer(2000); // 2 segundos
            USBAttachedDebounceTimer.Elapsed += USBAttachedResetEventProcessed;
            USBAttachedDebounceTimer.AutoReset = false;
            USBAttachedDebounceTimer.Start();
        }

        private void USBAttachedResetEventProcessed(object sender, ElapsedEventArgs e)
        {
            USBAttachedEventProcessed = false;
            USBAttachedDebounceTimer.Stop();
        }

        private void OnUSBDetached(object sender, EventArrivedEventArgs e)
        {
            if (USBDetachedEventProcessed)
            {
                return;
            }

            USBDetachedEventProcessed = true;

            var detachedDiskDrives = GetDiskDrives().Except(diskDrives);

            foreach (var item in detachedDiskDrives)
            {
                Console.WriteLine("USB device {0} detached", item.DeviceID);
            }

            // Reiniciar el temporizador
            USBDetachedDebounceTimer = new Timer(2000); // 2 segundos
            USBDetachedDebounceTimer.Elapsed += USBDetachedResetEventProcessed;
            USBDetachedDebounceTimer.AutoReset = false;
            USBDetachedDebounceTimer.Start();
        }

        private void USBDetachedResetEventProcessed(object sender, ElapsedEventArgs e)
        {
            USBDetachedEventProcessed = false;
            USBDetachedDebounceTimer.Stop();
        }
#endif
        public class Disk : IEqualityComparer<Disk>
        {
            public string DeviceID { get; set; }
            public string SerialNumber { get; set; }
            public string Manufacturer { get; set; }
            public string Model { get; set; }
            public string InterfaceType { get; set; }
            public ulong Size { get; set; }
            public IEnumerable<Partition> Partitions { get; set; }

            public bool Equals(Disk x, Disk y)
            {
                if (x == null || y == null)
                    return false;

                // Comparar discos por SerialNumber
                return x.SerialNumber == y.SerialNumber;
            }

            public int GetHashCode(Disk obj)
            {
                if (obj == null)
                    throw new ArgumentNullException(nameof(obj));

                // SerialNumber como base para el hash code
                return obj.SerialNumber?.GetHashCode() ?? 0;
            }
        }
        public class Partition
        {
            public string DeviceID { get; set; }
            public ulong Size { get; set; }
            public string Type { get; set; }
            public string NumberOfBlock { get; set; }
            public ulong StartingOffset { get; set; }

            public IEnumerable<LogicalDisk> LogicalDisks { get; set; }
        }
        public class LogicalDisk
        {
            public string DeviceID { get; set; }
            public string FileSystem { get; set; }
            public ulong FreeSpace { get; set; }
            public ulong Size { get; set; }
        }
    }
}

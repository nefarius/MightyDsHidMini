using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JsonConfig;
using Mighty.HID;

namespace MightyHIDTest
{
    internal class ReplayEntry
    {
        public ReplayEntry(string report, int delay)
        {
            Report = StringToByteArray(report);
            WaitPeriod = TimeSpan.FromMilliseconds(delay);
        }

        public byte[] Report { get; set; }

        public TimeSpan WaitPeriod { get; set; }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }

    internal class Program
    {
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        private static void Main(string[] args)
        {
            /* hello, world! */
            Console.WriteLine("Looking for DsHidMini devices");
            /* browse for hid devices */
            var devs = HIDBrowse.Browse().Where(d => d.Vid == 0x054C && d.Pid == 0x0268);

            if (!devs.Any())
            {
                Console.WriteLine("None found, nothing to do");
                return;
            }

            Console.WriteLine("Found one");

            var instructions = ((IList<dynamic>) Config.Global.Instructions)
                .Select(i => new ReplayEntry((string) i.OutputReport, (int) i.WaitPeriodMs)).ToList();

            /* new device */
            var dev = new HIDDev();
            /* connect */
            dev.Open(devs.First());

            /* send report */
            foreach (var instruction in instructions)
            {
                Console.WriteLine("Sending report");
                dev.Write(instruction.Report);
                Console.WriteLine("Waiting");
                Thread.Sleep(instruction.WaitPeriod);
            }

            Console.WriteLine("Done!");
            Console.Read();
        }
    }
}
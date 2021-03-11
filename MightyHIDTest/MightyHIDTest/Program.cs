using System;
using System.Linq;
using Mighty.HID;

namespace MightyHIDTest
{
    internal class Program
    {
        public static byte[] StringToByteArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        private static void Main(string[] args)
        {
            var report = StringToByteArray((string) JsonConfig.Config.Global.OutputReport);

            if (report.Length < 49)
            {
                Console.WriteLine("Missing arguments, can't continue");
                return;
            }

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

            /* new device */
            var dev = new HIDDev();
            /* connect */
            dev.Open(devs.First());
            /* an example of hid report, report id is always located 
             * at the beginning of every report. Here it was set to 0x01.
             * adjust this report so it does meet your hid device reports */
            /* send report */
            Console.WriteLine("Sending report");
            dev.Write(report);

            Console.WriteLine("Done!");
            Console.Read();
        }
    }
}
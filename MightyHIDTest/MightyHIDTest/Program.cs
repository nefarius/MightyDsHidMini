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
        public ReplayEntry(string report, int delay, int timestosend, int repdelay)
        {
            Report = StringToByteArray(report);
            WaitPeriod = TimeSpan.FromMilliseconds(delay);
            TimesToSend = timestosend;
            RepDelay = TimeSpan.FromMilliseconds(repdelay);
        }

        public byte[] Report { get; set; }

        public TimeSpan WaitPeriod { get; set; }

        public int TimesToSend { get; set; }

        public TimeSpan RepDelay { get; set; }

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
                .Select(i => new ReplayEntry((string) i.OutputReport, (int) i.WaitPeriodMs, (int) i.SendXTimes, (int)i.RepeatDelay)).ToList();

            /* new device */
            var dev = new HIDDev();
            /* connect */
            dev.Open(devs.First());

            /* send report */

            String userin;
            Console.Write("Execute how many times? (0 = indefinitely): ");
            userin = Console.ReadLine();
            int FullRepeat = string.IsNullOrEmpty(userin) ? 0 : Convert.ToInt32(userin);
            Console.WriteLine();

            int FR = 1;
            while(FullRepeat == 0 || FR <= FullRepeat)
            {

                Console.Clear();

                if (FullRepeat >= 1)
                {
                    Console.WriteLine("===== EXECUTING (" + FR + "/" + FullRepeat + ") =====");
                    Console.WriteLine();
                }

                int CurrentI = 0;

                foreach (var instruction in instructions)
                {
                    Console.WriteLine("Executing instructions block number " + CurrentI);

                    int P = 1;

                    while (P <= instruction.TimesToSend)
                    {
                        dev.Write(instruction.Report);
                        Console.Write("."); // Print "." to inform that a command has been sent

                        if (P == instruction.TimesToSend) // Break; if on the last repeat to prevent an aditional delay on Thread.Sleep(instruction.RepDelay)
                        {
                            Console.WriteLine();
                            Console.WriteLine(P + " command(s) sent"); // informs the number of sent commands
                            break;
                        }
                        if (instruction.RepDelay > TimeSpan.Zero) // Print "-" if RepDelay > 0
                        {
                            Console.Write("-");
                            Thread.Sleep(instruction.RepDelay);
                        }
                        P++;
                    }

                    if (instruction.WaitPeriod > TimeSpan.Zero)
                    {
                        Console.WriteLine("Waiting for " + instruction.WaitPeriod + "ms");
                        Thread.Sleep(instruction.WaitPeriod);
                    }
                    else
                    {
                        Console.WriteLine("Not waiting");
                    }
                    CurrentI++;
                    Console.WriteLine();
                }

                if (FullRepeat >= 1) { FR++; }

            }


            Console.WriteLine("Done!");
            Console.Read();
        }
    }
}
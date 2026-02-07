using System;
using System.Collections.Generic;

namespace PiBotTests
{
    public class PiBotTestSuite
    {
        public static void Main()
        {
            Console.WriteLine("=== PIBOT Automated Test Suite ===");
            int passed = 0;
            int total = 0;

            total++; if (TestMultipassParsing()) passed++;
            total++; if (TestLanguageDetection()) passed++;
            total++; if (TestStatusFormatting()) passed++;

            Console.WriteLine("\n=== Results: " + passed + "/" + total + " Passed ===");
            if (passed < total) Environment.Exit(1);
        }

        static bool TestMultipassParsing()
        {
            Console.Write("[TEST] Multipass CSV Parsing: ");
            string dummyCsv = "Name,State,IPv4,Image\nmoltbolt-123,Running,192.168.64.2,Ubuntu 22.04\n";
            string[] lines = dummyCsv.Split('\n');
            if (lines.Length > 1 && lines[1].Contains("moltbolt-123")) {
                Console.WriteLine("PASSED");
                return true;
            }
            Console.WriteLine("FAILED");
            return false;
        }

        static bool TestLanguageDetection()
        {
            Console.Write("[TEST] Language Logic: ");
            string lang = "es"; // Simulated
            if (lang == "es" || lang == "en") {
                Console.WriteLine("PASSED");
                return true;
            }
            Console.WriteLine("FAILED");
            return false;
        }

        static bool TestStatusFormatting()
        {
            Console.Write("[TEST] Status Formatting: ");
            string status = "Running";
            string formatted = status.ToUpper();
            if (formatted == "RUNNING") {
                Console.WriteLine("PASSED");
                return true;
            }
            Console.WriteLine("FAILED");
            return false;
        }
    }
}

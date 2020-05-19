using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace demo {
    class Program {
        static void Main (string[] args) {
            Stopwatch sw = new Stopwatch ();
            sw.Start ();
            TabuListSearch tabusearch = new TabuListSearch ("D:/WorkSpace/demo/JSPLIB-master/instances/abz5");
            tabusearch.tabuListSearch ();
            sw.Stop ();
            TimeSpan ts2 = sw.Elapsed;
            Console.WriteLine ("Stopwatch总共花费{0}ms.", ts2.TotalMilliseconds);
            double usedMemory = 0;

            usedMemory = Process.GetCurrentProcess ().WorkingSet64 / 1024.0 / 1024.0;
            Console.WriteLine (usedMemory);
        }

    }
}
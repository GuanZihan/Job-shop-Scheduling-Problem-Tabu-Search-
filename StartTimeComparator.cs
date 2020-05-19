using System;
using System.Collections.Generic;
namespace demo {
    public class StartTimeComparator : IComparer<Operation> {
        public int Compare (Operation left, Operation right)

        {

            if (left.getStartTime () > right.getStartTime ())

                return 1;

            else if (left.getStartTime () == right.getStartTime ())

                return 0;

            else

                return -1;

        }
    }
}
using System;
using System.Diagnostics;

namespace demo
{
    /*
    Singleton Mode
    */
    public class Drawer
    {
        private static Drawer drawer;
        private Drawer() {
            
        }

        //Constructor for Drawer
        public static Drawer getDrawer() {
            if(drawer == null) {
                drawer = new Drawer();
            }
            return drawer;
        }

        //For Line Diagrams
        //@input: X
        //@input: y
        public void drawLineDiagram(int[] X, int[] y) {
            
        }
    }
}
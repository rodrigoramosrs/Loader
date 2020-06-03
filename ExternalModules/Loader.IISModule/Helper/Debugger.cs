using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Loader.Helper
{
    public static class Debugger
    {
        public static void Write(string message)
        {
            Debug.WriteLine("[LOADER]: " + message);
        }
    }
}

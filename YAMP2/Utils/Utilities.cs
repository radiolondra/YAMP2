
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace YAMP2.Utils
{
    public class Utilities
    {
        public static void SetWindowBounds(
            double pixelDensity, 
            double containerW, 
            double containerH, 
            double originalW,
            double originalH,            
            out double width, 
            out double height,
            bool addIncrement = true)
        {
            var incrementX = 0.0;
            var incrementY = 0.0;

            // Trick of the pig I
            if (pixelDensity > 1.0)
            {
                pixelDensity = pixelDensity - 1.25 + 1.0;
            }

            if (addIncrement)
            {
                // Trick of the pig II
                incrementX = originalW * pixelDensity * 0.1;
                incrementY = originalH * pixelDensity * 0.1;

                if (originalW + incrementX > containerW)
                {
                    incrementX = containerW - originalW - 100;
                }
                if (originalH + incrementY > containerH)
                {
                    incrementY = containerH - originalH - 100;
                }
            }           

            width = (originalW + incrementX) / pixelDensity;
            height = (originalH + incrementY) / pixelDensity;
        }

        /// <summary>
        /// Returns the folder where binary is running
        /// </summary>
        /// <returns></returns>
        public static string ApplicationFolder()
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string? assemblyPath = Path.GetDirectoryName(assembly.Location);
            return assemblyPath;
        }

        /// <summary>
        /// Open folder
        /// </summary>
        /// <param name="uri"></param>
        public static void OpenUri(string uri) => _ = Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });

        /// <summary>
        /// Check if the running process is a 86/64 process
        /// </summary>
        /// <returns></returns>
        public static bool IsWin64()
        {
            //if (Environment.Is64BitOperatingSystem)
            if (IntPtr.Size == 4)
            {
                return false;
            }
            return true;
        }


    }
}

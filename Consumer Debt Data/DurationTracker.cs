using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FluidTest
{

    /// <summary>
    /// simple quick and dirty class used to track durations for lengthy data generation operations
    /// </summary>
    internal class DurationTracker
    {

        private static string outputFile;
        private static string checkpointFile;

        private static Dictionary<string, double> durations = new Dictionary<string, double>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="outputFilePath"></param>
        /// <param name="checkpointFilePath"></param>
        internal DurationTracker(string outputFilePath, string checkpointFilePath)
        {
            outputFile = outputFilePath;
            
            checkpointFile = checkpointFilePath;

            // create a clean checkpoint file per run
            if (File.Exists(checkpointFile))
            {
                File.SetAttributes(checkpointFilePath, FileAttributes.Normal);
                File.Delete(checkpointFile);
            }

            using (StreamWriter sw = File.CreateText(checkpointFile))
            {
                // "Using" makes sure the file is not marked as in use..seems to fix a problem in XP
            }
        }



        /// <summary>
        /// write a timestamp out to the checkpoint file
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="message"></param>
        internal void MarkTimestamp(DateTime timestamp)
        {
            
            MarkTimestamp(timestamp, string.Empty);

        }

        /// <summary>
        /// write a timestamp out to the checkpoint file
        /// </summary>
        /// <param name="timestamp"></param>
        internal void MarkTimestamp(DateTime timestamp, string message)
        {

            string output;

            if (string.IsNullOrEmpty(message))
            {
                output = timestamp.ToString();
            }
            else
            {
                output = string.Format("{0}: {1}", timestamp.ToString(), message);
            }


            // Write a timestamp out to a 'checkpoint' file.  This is helpful when
            // running outside the debugger so you can see what the app did last
            // [skt] this should be replaced by the tracelogger 
            using (StreamWriter sw = File.AppendText(checkpointFile))
            {
                sw.WriteLine(output);
            }
        }



        /// <summary>
        /// Create an entry in the checkpoint file and in the running tally list
        /// </summary>
        /// <param name="category"></param>
        /// <param name="duration"></param>
        internal void MarkCheckpoint(string category, double duration)
        {
            // keep a running list of durations
            durations.Add(category, duration);

            // Write the duration out to a 'checkpoint' file.  This is helpful when
            // running outside the debugger so you can see what the app did last
            // [skt] this should be replaced by the tracelogger 
            using (StreamWriter sw = File.AppendText(checkpointFile)) 
            {
                sw.WriteLine("{2}: {1:F3} Min: {0}", category, duration, DateTime.Now.ToString());
            }
        }


        /// <summary>
        /// write out the duration list to disk
        /// </summary>
        internal void DumpDurations(StreamWriter sw)
        {
            double tally = 0;
            sw.WriteLine("");
            sw.WriteLine("Data Gen Durations");
            foreach (KeyValuePair<string, double> kvp in durations)
            {
                sw.WriteLine("{1:F3} Min: {0}", kvp.Key, kvp.Value);
                tally += kvp.Value;
            }
            sw.WriteLine("{0:F3} Min: Total", tally);
        }
    }
}

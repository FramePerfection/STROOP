using System.IO;

namespace STROOP.Utilities
{
    public static class ErrorUtilities
    {
        public static string SeeLogFileText = "See error.log for details.";
        static bool firstError = true;

        public static void WriteErrorLog(string text)
        {
            using (var wr = new StreamWriter("error.log", !firstError))
            {
                wr.WriteLine(System.DateTime.Now);
                wr.WriteLine(text);
                wr.WriteLine();
            }

            firstError = false;
        }
    }
}

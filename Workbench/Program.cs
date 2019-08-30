using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Workbench
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += Application_Error;
            var sw = Stopwatch.StartNew();

            // do stuff here
            DAL.Standard.IDatabase test = new DAL.Standard.DatabaseFake("connection string");
            test.ExecuteQuerySp("TestProc", new SqlParameter[0]);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Total run time: {sw.Elapsed}");
            Console.WriteLine("Press any key to exit...");
            Console.ResetColor();
            Console.ReadKey();
        }

        private static void Application_Error(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Fatal error encountered '{ex.Message}', cannot continue");
                Console.ResetColor();

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();

                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unhandled application error: {ex.Message}, stacktrace: {ex.StackTrace}");
            }
        }
    }
}

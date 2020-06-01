using System;
using System.Data.SqlClient;
using System.Diagnostics;

using DAL.Standard;

namespace Workbench
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += Application_Error;
            var sw = Stopwatch.StartNew();

            // do stuff here

            IDatabase test = new DatabaseFake();
            var parameters = new SqlParameter[0];

            //IDatabase test = new Database(LOCAL_SQL, true, true);

            //var parameters = new SqlParameter[]
            //{
            //    new SqlParameter() { Value = 3.4f, ParameterName = "@Fooo", DbType = DbType.Single },
            //    new SqlParameter() { Value = "blork", ParameterName = "@Snorrg", DbType = DbType.String, Size = 50 }
            //};

            test.ExecuteQuerySp("AccountData.Account_CheckEmail", parameters);

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
                Console.WriteLine($"Fatal error encountered, cannot continue: {ex}");
                Console.ResetColor();

                // test output format
                //string output = JsonConvert.SerializeObject(ex);

                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();

                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unhandled application error: {ex}");
            }
        }
    }
}

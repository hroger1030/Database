using DAL.DataBase;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Workbench
{
    public class Program
    {
        private const string SQL_CONN = "Data Source=.;Initial Catalog=master;Integrated Security=True;Pooling=true;";

        public static void Main(string[] Args)
        {
            AppDomain.CurrentDomain.UnhandledException += Application_Error;
            var sw = Stopwatch.StartNew();

            // do stuff here

            IDatabase test = new DatabaseFake();
            var parameters = Array.Empty<SqlParameter>();

            //IDatabase test = new Database(SQL_CONN, true, true);

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

                // push this to debug stream too
                Debug.WriteLine(JsonConvert.SerializeObject(ex));

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

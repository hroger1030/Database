using DAL.Net;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Workbench
{
    public class Program
    {
        /// <summary>
        /// We are assuming that we are working off a local SQL Server instance by default
        /// </summary>
        private const string SQL_CONN = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Pooling=true;";

        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += Application_Error;
            var sw = Stopwatch.StartNew();


            // run integration tests against the testDb
            IDatabase test = new Database(SQL_CONN, true, true);

            var parameters = new SqlParameter[]
            {
                new SqlParameter() { Value = 1, ParameterName = "@Id", DbType = DbType.Int32 },
            };

            //Func<SqlDataReader, Dictionary<int, string>> processor = delegate (SqlDataReader reader)
            //{
            //    var output = new Dictionary<int, string>();

            //    while (reader.Read())
            //    {
            //        int id = (int)reader["Id"];
            //        string name = (string)reader["Location"];

            //        output.Add(id, name);
            //    }

            //    return output;
            //};

            var result =  Task.Run(() => test.ExecuteQuerySpAsync<DbTestTable>("[toolsdb].[dbo].[SelectAllData]", parameters)).Result;

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

using DAL.DataBase;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Workbench
{
    public class Program
    {
        private const string SQL_CONN = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AdhocTests;Integrated Security=True;Pooling=true;";

        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += Application_Error;
            var sw = Stopwatch.StartNew();

            // do stuff here

            //IDatabase test = new DatabaseFake();
            //var parameters = Array.Empty<SqlParameter>();

            IDatabase test = new DAL.DataBase.Database(SQL_CONN, true, true);

            //var parameters = new SqlParameter[]
            //{
            //    new SqlParameter() { Value = 3.4f, ParameterName = "@Fooo", DbType = DbType.Single },
            //    new SqlParameter() { Value = "blork", ParameterName = "@Snorrg", DbType = DbType.String, Size = 50 }
            //};

            //test.ExecuteQuerySp("AccountData.Account_CheckEmail", parameters);

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

            var result = test.ExecuteQuery<GeoTest>("select * from test", null);


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

    public class GeoTest
    {
        public int Id { get; set; }
        public SqlGeometry Location { get; set; }
    }
}

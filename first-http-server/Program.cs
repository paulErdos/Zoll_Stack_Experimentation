using System;
using System.Data;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace HttpListenerExample
{
    class HttpServer
    {
        public static HttpListener listener;
        public static string url = "http://localhost:8000/";
        public static int pageViews = 0;
        public static int requestCount = 0;
        public static string pageData =
            "<!DOCTYPE>" +
            "<html>" +
            "   <head>" +
            "       <title>HttpListener Example</title>" +
            "       <script type=\"text/javascript\" src=\"./testscript.js\"></script> " +
            "   </head>" +
            "   <body>" +
            "       <p>Page Views: {0}</p>" +
            "       <form method=\"post\" action=\"shutdown\">" +
            "           <input type=\"submit\" value=\"Shutdown\" {1}>" +
            "       </form>" +
            "       <form method=\"post\" action=\"equitySelection\">" +
            "              <p>Compare Vanguard S&P 500 ETF VOO to:</p>" +
            "              <input type=\"radio\" id=\"qqq\" name=\"ticker\" value=\"qqq\">" +
            "              <label for=\"qqq\">Nasdaq ETF QQQ</label><br>" +
            "              <input type=\"radio\" id=\"intu\" name=\"age\" value=\"intu\">" +
            "              <label for=\"intu\">Intuit, Inc.</label><br>  " +
            "              <input type=\"submit\" value=\"Submit\">" +
            "            </form>" +
            "            {2}" +
            "   </body>" +
            "</html>";


        public static async Task HandleIncomingConnections(SqlConnection connection)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            string queryString = "select * from spy";
            List<Object[]> theData = getData(connection, queryString);
            string theJson = makeJson(theData);

            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    Console.WriteLine("Shutdown requested");
                    runServer = false;
                }

                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                // Write the response info
                string disableSubmit = !runServer ? "disabled" : "";
                byte[] data = Encoding.UTF8.GetBytes(String.Format(pageData, pageViews, disableSubmit, theJson));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }

        private static List<Object[]> getData(SqlConnection connection, string queryString)
        {
            List<Object[]> rows = new List<object[]>();
            SqlCommand command = new SqlCommand(queryString, connection);
            try
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Object[] row = new object[reader.FieldCount];
                        reader.GetValues(row);

                        rows.Add(row);
                    }

                    /*
                    foreach(Object[] thisRow in rows)
                    {
                        Console.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}",
                            thisRow[0], thisRow[1], thisRow[2], thisRow[3], thisRow[4], thisRow[5], thisRow[6]));
                    }
                    */
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Ya dun goofed the query you n00b" + e.Message);
            }

            return rows;
        }

        private static string makeJson(List<Object[]> data)
        {
            /*
             * Manually for learning's sake
             */

            string the_json = "{\n";

            // Key <-- Date <-- First field
            // And let's just use the open price for now
            for(int i = 0; i < data.Count - 1; i++)
            {
                string the_date = (string)data[i][0]; // This is a string already
                double the_open = (double)data[i][1]; // This is a Double

                the_json += "\t\"" + the_date + "\": " +
                    "{\"open\": \"" + Convert.ToString(the_open) + "\"" + "},\n";

            }

            // Last row without trailing comma
            string final_date = (string)data[data.Count - 1][0];
            double final_open = (double)data[data.Count - 1][1];
            the_json += "\t\"" + final_date + "\": " +
                "{\"open\": \"" + Convert.ToString(final_open) + "\"" + "}\n";

            the_json += "}";

            return the_json;
        }

        private static SqlConnection attemptToConnect()
        {
            string connectionString = "Data Source=127.0.0.1,1433;Initial Catalog=Equities;User ID=SA;Password=Aa345678";
            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                Console.WriteLine("Attempting to open connection...");
                connection.Open();
                Console.WriteLine("Successfully opened connection!");
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to open connection to database: " + e.Message);
            }

            return connection;
        }

        public static void Main(string[] args)
        {
            // Get a connection to the db
            SqlConnection connection = attemptToConnect();

            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests, passing in db connection
            Task listenTask = HandleIncomingConnections(connection);
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}
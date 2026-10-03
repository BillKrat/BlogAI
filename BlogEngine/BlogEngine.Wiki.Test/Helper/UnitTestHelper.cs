using System;
using System.IO;
using System.Web;
using BlogEngine.Wiki.Test.Mocks;

namespace BlogEngine.Wiki.Test.Helper
{
    public class UnitTestHelper
    {
        internal static string GenerateUniqueHost()
        {
            return "MyHost";
        }


        /// <summary>
        /// Sets the HTTP context with a valid simulated request
        /// http://haacked.com/archive/2005/06/11/Simulating_HttpContext.aspx
        /// </summary>
        /// <param name="host">Host.</param>
        /// <param name="application">Application.</param>
        public static void SetHttpContextWithSimulatedRequest(string host, string application)
        {
            const string appVirtualDir = "/";
            const string appPhysicalDir = @"c:\projects\SubtextSystem\Subtext.Web\";
            string page = application.Replace("/", string.Empty) + "/default.aspx";
            string query = string.Empty;
            TextWriter output = null;

            var workerRequest = new SimulatedHttpRequest(appVirtualDir, appPhysicalDir, page, query, output, host);
            HttpContext.Current = new HttpContext(workerRequest);

            Console.WriteLine("Request.FilePath: " + HttpContext.Current.Request.FilePath);
            Console.WriteLine("Request.Path: " + HttpContext.Current.Request.Path);
            Console.WriteLine("Request.RawUrl: " + HttpContext.Current.Request.RawUrl);
            Console.WriteLine("Request.Url: " + HttpContext.Current.Request.Url);
            Console.WriteLine("Request.ApplicationPath: " + HttpContext.Current.Request.ApplicationPath);
            Console.WriteLine("Request.PhysicalPath: " + HttpContext.Current.Request.PhysicalPath);
        }

        /// <summary>
        /// Gets the test output path.
        /// </summary>
        /// <returns></returns>
        public static string GetTestOutputPath(string folder)
        {
            var returnValue = Directory.GetCurrentDirectory();

            // Find the TestResults folder - it is our root
            var offset = returnValue.IndexOf("TestResults");
            
            // From the root go into the BlogEngine.Wiki.Test
            // projects TestOutput folder
            returnValue = string.Format("{0}{1}{2}",
                returnValue.Substring(0, offset),
                "BlogEngine.Wiki.Test\\TestOutput\\",
                folder);
            
            return returnValue;
        }
        /// <summary>
        /// Gets the test output path.
        /// </summary>
        /// <returns></returns>
        public static string GetBlogEnginePath(string folder)
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null)
            {
                var directCandidate = Path.Combine(directory.FullName, "BlogEngine.NET", folder);
                if (Directory.Exists(directCandidate))
                    return directCandidate;

                var nestedCandidate = Path.Combine(directory.FullName, "BlogEngine", "BlogEngine.NET", folder);
                if (Directory.Exists(nestedCandidate))
                    return nestedCandidate;

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate the BlogEngine.NET folder for the test data path.");
        }

    }
}

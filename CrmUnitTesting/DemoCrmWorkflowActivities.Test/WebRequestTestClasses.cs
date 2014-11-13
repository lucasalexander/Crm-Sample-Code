using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace DemoCrmWorkflowActivities.Test
{
    /// <summary>
    /// Class used to mock System.Net.WebRequest calls using WebRequest.RegisterPrefix - see http://blog.salamandersoft.co.uk/index.php/2009/10/how-to-mock-httpwebrequest-when-unit-testing/ for details
    /// </summary>
    class TestWebRequestCreate : IWebRequestCreate
    {
        static WebRequest nextRequest;
        static object lockObject = new object();

        static public WebRequest NextRequest
        {
            get { return nextRequest; }
            set
            {
                lock (lockObject)
                {
                    nextRequest = value;
                }
            }
        }

        /// <summary>See <see cref="IWebRequestCreate.Create"/>.</summary>
        public WebRequest Create(Uri uri)
        {
            return nextRequest;
        }

        /// <summary>Utility method for creating a TestWebRequest and setting
        /// it to be the next WebRequest to use.</summary>
        /// <param name="response">The response the TestWebRequest will return.</param>
        public static TestWebRequest CreateTestRequest(string response)
        {
            TestWebRequest request = new TestWebRequest(response);
            NextRequest = request;
            return request;
        }

        public static TestWebRequest CreateTestRequest(string response, string error)
        {
            TestWebRequest request = new TestWebRequest(response, error);
            NextRequest = request;
            return request;
        }

    }

    /// <summary>
    /// Class used by the TestWebRequestCreate class to represent a WebRequest
    /// </summary>
    class TestWebRequest : WebRequest
    {
        MemoryStream requestStream = new MemoryStream();
        MemoryStream responseStream;

        public override string Method { get; set; }
        public override string ContentType { get; set; }
        public override long ContentLength { get; set; }

        private string _errorToThrow = "";

        /// <summary>Initializes a new instance of <see cref="TestWebRequest"/>
        /// with the response to return.</summary>
        public TestWebRequest(string response)
        {
            responseStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
        }

        /// <summary>
        /// Constructor used to simulate error responses
        /// </summary>
        /// <param name="response"></param>
        /// <param name="error">Currently throws errors for "timeout," "unknownerror" and "generalexception" inputs. All other input values do not throw errors.</param>
        public TestWebRequest(string response, string error)
        {
            responseStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
            _errorToThrow = error;
        }

        /// <summary>Returns the request contents as a string.</summary>
        public string ContentAsString()
        {
            return System.Text.Encoding.UTF8.GetString(requestStream.ToArray());
        }

        /// <summary>See <see cref="WebRequest.GetRequestStream"/>.</summary>
        public override Stream GetRequestStream()
        {
            return requestStream;
        }

        /// <summary>See <see cref="WebRequest.GetResponse"/>.</summary>
        public override WebResponse GetResponse()
        {
            switch (_errorToThrow)
            {
                case "timeout":
                    throw new WebException("timeout", WebExceptionStatus.Timeout);
                    break;
                case "unknownerror":
                    throw new WebException("unknownerror", WebExceptionStatus.UnknownError);
                    break;
                case "generalexception":
                    throw new Exception("generalexception");
                    break;
                default:
                    return new TestWebReponse(responseStream);
            }
        }
    }

    /// <summary>
    /// Class used by the TestWebRequestCreate class to represent a WebResponse
    /// </summary>
    class TestWebReponse : WebResponse
    {
        Stream responseStream;

        /// <summary>Initializes a new instance of <see cref="TestWebReponse"/>
        /// with the response stream to return.</summary>
        public TestWebReponse(Stream responseStream)
        {
            this.responseStream = responseStream;
        }

        /// <summary>See <see cref="WebResponse.GetResponseStream"/>.</summary>
        public override Stream GetResponseStream()
        {
            return responseStream;
        }
    }
}

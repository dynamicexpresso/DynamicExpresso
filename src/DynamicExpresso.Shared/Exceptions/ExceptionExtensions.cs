using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace DynamicExpresso.Exceptions
{
	internal static class ExceptionExtensions
	{
        // NOTE: With .NET 4.5 we can probably use ExceptionDispatchInfo to preserve the stack trace.
        // http://blogs.microsoft.co.il/blogs/sasha/archive/2011/10/19/capture-transfer-and-rethrow-exceptions-with-exceptiondispatchinfo-net-4-5.aspx
        // See also:
        // http://stackoverflow.com/questions/57383/in-c-how-can-i-rethrow-innerexception-without-losing-stack-trace?rq=1
        // http://stackoverflow.com/questions/4555599/how-to-rethrow-the-inner-exception-of-a-targetinvocationexception-without-losing

        // This method doesn't work for custom exception without serialization ctor
        //public static void PreserveStackTrace(this Exception e)
        //{
        //    var ctx = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.CrossAppDomain);
        //    var mgr = new System.Runtime.Serialization.ObjectManager(null, ctx);
        //    var si = new System.Runtime.Serialization.SerializationInfo(e.GetType(), new System.Runtime.Serialization.FormatterConverter());

        //    e.GetObjectData(si, ctx);
        //    mgr.RegisterObject(e, 1, si);
        //    mgr.DoFixups();
        //}

        public static void PreserveStackTrace(this Exception exception)
		{
			try {
				typeof(Exception).GetMethod("PrepForRemoting",
						System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
						.Invoke(exception, new object[0]);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Assert(false, ex.Message);
			}
		}
    }
}

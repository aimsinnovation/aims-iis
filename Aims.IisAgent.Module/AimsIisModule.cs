using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using Aims.IISAgent.Module.Pipes;

namespace Aims.IISAgent.Module
{
	public class AimsIisModule : IHttpModule
	{
		private EventLog _log;
		public string ModuleName => "AimsIisModule";

		// In the Init function, register for HttpApplication
		// events by adding your handlers.
		public void Init(HttpApplication application)
		{
			_log = new EventLog()
			{
				Source = "AIMS IIS Agent"
			};

			application.EndRequest +=
				Application_EndRequest;
			//application.Error += Application_Error;
			//application.PostAuthorizeRequest += Application_PostAuthorizeRequest;
			//application.PostAuthenticateRequest += Application_PostAuthenticateRequest;
		}

		public void Dispose()
		{
		}

		private void Application_PostAuthenticateRequest(object sender, EventArgs e)
		{
			HttpApplication application = (HttpApplication)sender;
			HttpContext context = application.Context;
		}

		private void Application_PostAuthorizeRequest(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void Application_Error(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void Application_BeginRequest(object source,
			EventArgs e)
		{
			// Create HttpApplication and HttpContext objects to access
			// request and response properties.
			HttpApplication application = (HttpApplication)source;
			HttpContext context = application.Context;
			context.Response.Write("<h1><font color=green>" +
									   "AimsIisModule: Beginning of Request" +
									   "</font></h1><hr>");
			context.Response.Flush();
		}

		private void Application_EndRequest(object source, EventArgs e)
		{
			byte[] buf = new byte[1024];
			var m = new Message()
			{
				StatType = "foo",
				DateTime = DateTime.UtcNow,
				Code = 0,
				Site = "bar",
			};

			_log.WriteEntry("Added to queue");
			Tracker.AddMessage(m);
		}
	}
}
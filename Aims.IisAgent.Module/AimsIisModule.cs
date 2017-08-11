using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Aims.IISAgent.Loggers;
using Aims.IISAgent.Module.Pipes;

namespace Aims.IISAgent.Module
{
	public class AimsIisModule : IHttpModule
	{
		private ILogger _log;
		public string ModuleName => "AimsIisModule";
		private PipeWriter _msgWriter;

		// In the Init function, register for HttpApplication
		// events by adding your handlers.
		public void Init(HttpApplication application)
		{
			_log = new WindowsEventLogger(new EventLog()
			{
				Source = "AIMS IIS Agent"
			});

			_msgWriter = new PipeWriter(_log);

			application.EndRequest += OnEndRequest;
			application.Error += OnError;
			application.AuthenticateRequest += OnAuthorizeRequest;
		}

		public void Dispose()
		{
		}

		private void OnAuthorizeRequest(object sender, EventArgs e)
		{
			try
			{
				HttpApplication application = (HttpApplication)sender;
				if (!application.Context.SkipAuthorization)
				{
					Message m = CreateTmplateMessage(application);
					if (application.User != null && application.User.Identity.IsAuthenticated)
						m.StatType = AgentConstants.StatType.LogonSuccessful;
					else
						m.StatType = AgentConstants.StatType.LogonFailed;
					_msgWriter.AddMessage(m);
				}
			}
			catch (Exception)
			{
				//ignored
			}
		}

		private void OnError(object sender, EventArgs e)
		{
			try
			{
				HttpApplication application = (HttpApplication)sender;

				var m = CreateTmplateMessage(application);
				m.StatType = AgentConstants.StatType.Error5xx;
				_msgWriter.AddMessage(m);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void OnEndRequest(object source, EventArgs e)
		{
			try
			{
				HttpApplication application = (HttpApplication)source;

				var m = CreateTmplateMessage(application);
				_msgWriter.AddMessage(m);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private Message CreateTmplateMessage(HttpApplication application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));
			var url = application.Context.Request.Url;
			return new Message()
			{
				StatType = string.Empty,
				DateTime = DateTime.UtcNow,
				Code = application.Context.Response.StatusCode,
				Scheme = url.Scheme,
				Domain = url.Host,
				Port = url.Port,
				Segment2 = url.Segments.Length >= 2 ? url.Segments[1] : string.Empty
			};
		}
	}
}
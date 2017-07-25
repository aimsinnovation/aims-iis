using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;

namespace InSolve.test
{
    public class HttpTestModule : IHttpModule
    {
        #region IHttpModule Members

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.EndRequest += (source, e) =>
                {
                    if (source is HttpApplication)
                    {
                        HttpApplication app = (HttpApplication)source;
                        app.Response.AddHeader("X-test", "Hi from " + this.GetType().Name + " at " + app.Request.Url.Host);
                    }
                };
        }

        #endregion
    }
}

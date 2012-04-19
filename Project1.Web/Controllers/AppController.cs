using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;
using Noodles;

namespace Project1.Web.Controllers
{
    public class AppController : Controller
    {
        private readonly ISession _session;

        public AppController(ISession session)
        {
            _session = session;
        }

        public ActionResult Index(string path)
        {
            using (var t = _session.BeginTransaction())
            {
                var result = this.ControllerContext.GetNoodleResult(Core.Domain.User.Current, doInvoke: Invoke);
                if (ModelState.IsValid)
                {
                    _session.Flush();
                    t.Commit();
                }

                return result;
            }

        }

        private void Invoke(INodeMethod method, object[] args)
        {
            method.Invoke(args);
            //var log = new Core.Domain.EventLog()
            //{
            //    Path = target.Path(),
            //    Type = ((IMarkerInterface)target).TypeName,
            //    Method = method.Name,
            //    Arguments = args.Zip(method.Parameters, (v, p) => new { v, p }).ToDictionary(o => o.p.Name, o => o.v),
            //    Time = DateTimeOffset.UtcNow,
            //    User = HttpContext.Request.IsAuthenticated ? HttpContext.User.Identity.Name : "",
            //    RequestId =
            //        ((HttpWorkerRequest)((IServiceProvider)HttpContext).GetService(typeof(HttpWorkerRequest))).
            //            RequestTraceIdentifier
            //};
            //_session.Save(log);
        }

    }
}

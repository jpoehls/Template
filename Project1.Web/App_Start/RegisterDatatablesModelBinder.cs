using System.Web.Mvc;
using System.Web.WebPages;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Project1.Web.App_Start.RegisterDatatablesModelBinder), "Start")]

namespace Project1.Web.App_Start {
    public static class RegisterDatatablesModelBinder {
        public static void Start() {
            if (!ModelBinders.Binders.ContainsKey(typeof(Mvc.JQuery.Datatables.DataTablesParam)))
                ModelBinders.Binders.Add(typeof(Mvc.JQuery.Datatables.DataTablesParam), new Mvc.JQuery.Datatables.DataTablesModelBinder());
        }
    }
}

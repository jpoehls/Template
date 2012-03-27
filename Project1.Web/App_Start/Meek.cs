using Meek;

[assembly: WebActivator.PostApplicationStartMethod(typeof(Project1.Web.App_Start.Meek), "Start")]
namespace Project1.Web.App_Start

{
    public static class Meek
    {

        public static void Start()
        {
            BootStrapper.Initialize();
        }

    }
}
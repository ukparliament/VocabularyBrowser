namespace WebApplication1
{
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            return this.View();
        }
    }
}

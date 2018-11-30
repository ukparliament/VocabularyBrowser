namespace WebApplication1
{
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : BaseController
    {
        [Route("")]
        public ActionResult Index()
        {
            return this.View();
        }
    }
}

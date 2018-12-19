namespace VocabularyBrowser
{
    using Microsoft.AspNetCore.Mvc;

    [Route("vocabulary/browser")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return this.View();
        }
    }
}

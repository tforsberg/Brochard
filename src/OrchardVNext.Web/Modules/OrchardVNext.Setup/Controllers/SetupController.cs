using Microsoft.AspNet.Mvc;
using OrchardVNext.Setup.ViewModels;
using System;

namespace OrchardVNext.Setup.Controllers {
    public class SetupController : Controller {

        private ActionResult IndexViewResult(SetupViewModel model) {
            return View(model);
        }

        public ActionResult Index() {
            return IndexViewResult(new SetupViewModel {
            });
        }
    }
}
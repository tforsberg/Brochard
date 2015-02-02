using Microsoft.AspNet.Mvc;
using OrchardVNext.Setup.ViewModels;
using System;
using Microsoft.AspNet.Http;
using OrchardVNext.Setup.Services;

namespace OrchardVNext.Setup.Controllers {
    public class SetupController : Controller {
        private readonly ISetupService _setupService;

        public SetupController(ISetupService setupService) {
            _setupService = setupService;
        }

        private ActionResult IndexViewResult(SetupViewModel model) {
            return View(model);
        }

        public ActionResult Index() {
            return IndexViewResult(new SetupViewModel {
            });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model) {
            if (!ModelState.IsValid) {
                return IndexViewResult(model);
            }

            var setupContext = new SetupContext {
                SiteName = model.SiteName,
                EnabledFeatures = null, // default list
            };

            _setupService.Setup(setupContext);

            return IndexViewResult(model);
        }
    }
}
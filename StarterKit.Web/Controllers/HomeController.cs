using MassTransit;
using StarterKit.Contracts;
using StarterKit.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace StarterKit.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBus _bus;

        public HomeController(IBus bus)
        {
            _bus = bus;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<ActionResult> Submit(MyMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sendEndpoint = _bus.GetSendEndpoint(new Uri(ConfigurationManager.AppSettings["MyCommandQueueFullUri"])).Result;

                for (int i = 1; i <= model.CommandCount; i++)
                    sendEndpoint.Send<MyCommand>(new
                    {
                        Message = string.Format("Command{0}", i)
                    });

                for (int i = 1; i <= model.EventCount; i++)
                    _bus.Publish<MyEvent>(new
                    {
                        Message = string.Format("Event{0}", i)
                    });

                
                    return Task.FromResult<ActionResult>(RedirectToAction("Index"));
            }

            return Task.FromResult<ActionResult>(View("Index"));
        }
    }
}
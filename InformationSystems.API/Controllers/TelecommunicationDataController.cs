using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InformationSystems.API.Controllers
{
    [ApiController]
    [Route("/telecommunicationdata")]
    public class TelecommunicationDataController : Controller
    {
        // GET: TelecommunicationDataController
        public ActionResult Index()
        {
            return View();
        }

        // GET: TelecommunicationDataController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TelecommunicationDataController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TelecommunicationDataController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TelecommunicationDataController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TelecommunicationDataController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TelecommunicationDataController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TelecommunicationDataController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Game001WebApplication.Models;

namespace Game001WebApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            IndexViewModel model = new IndexViewModel();

            var betsrc = new List<IndexViewModel.Bet>();

            betsrc.Add(new IndexViewModel.Bet() { AssetName = "Фабрика C1", Addition = "Накладные расходы в размере 200.1" });
            betsrc.Add(new IndexViewModel.Bet() { AssetName = "ГЭС S1", Addition = "Накладные расходы в размере 50.5; Сезонность" });
            betsrc.Add(new IndexViewModel.Bet() { AssetName = "Фабрика C2", Addition = "Накладные расходы в размере 70.1; Сезонность" });

            model.Bets = betsrc;

            return View(model);

        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
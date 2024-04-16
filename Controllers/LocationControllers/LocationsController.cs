using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RLBW_ERP.Data;

namespace RLBW_ERP.Controllers.LocationControllers
{
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public JsonResult GetStatesList(int countryId)
        {
            var statesList = (from states in _context.States
                              where states.CountryId == countryId
                              orderby states.Name
                              select states).ToList();
            statesList.Insert(0, new Models.LocationModels.State { Id = 0, Name = "Selecione" });

            return Json(new SelectList(statesList, "Id", "Name"));
        }

        public JsonResult GetCitiesList(int stateId)
        {
            var CitiesList = (from cities in _context.Cities
                              where cities.StateId == stateId
                              orderby cities.Name
                              select cities).ToList();
            CitiesList.Insert(0, new Models.LocationModels.City { Id = 0, Name = "Selecione" });

            return Json(new SelectList(CitiesList, "Id", "Name"));
        }
    }
}

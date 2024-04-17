using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CRM_Sample.Data;

namespace CRM_Sample.Controllers.LocationControllers
{
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public JsonResult GetCountriesList()
        {
            var countriesList = (from countries in _context.Countries
                              orderby countries.Name
                              select countries).ToList();
            countriesList.Insert(0, new Models.LocationModels.Country { Id = 0, Name = "Selecione" });

            return Json(new SelectList(countriesList, "Id", "Name"));
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

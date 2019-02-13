using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharpBird.Api.ViewModels;
using SharpBird.Api.Utils;
using SharpBird.Mongo;
using SharpBird.Ryan;

namespace SharpBird.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly IBirdSearch _birdSearch;

        public FlightController(IBirdSearch birdSearch)
        {
            _birdSearch = birdSearch;
        }

        [HttpGet]
        public ActionResult<GenericResultViewModel<FlightResultViewModel>> Get(
            [FromQuery] string origin = "STN",
            [FromQuery] string destination = "BDS")
        {
            var result = Helpers.EvaluateWithElapsed(() =>
            {
                return _birdSearch
                    .Search(origin, destination, DateTime.Today.AddDays(1))
                    .GroupBy(x => x.Price)
                    .Select(x => new FlightViewModel()
                    {
                        Fare = x.Key,
                        Dates = x.Select(flight => flight.TimeDepartureUtc).ToList()
                    })
                    .ToList();
            });

            return new GenericResultViewModel<FlightResultViewModel>
            {
                Elapsed = result.Item2,
                Result = new FlightResultViewModel
                {
                    Origin = origin,
                    Destination = destination,
                    Flights = result.Item1
                }
            };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

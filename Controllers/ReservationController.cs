using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JwtTokenAndCORS_example.Models;

namespace UploadFileToWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize(Roles = "Manager")]//bảo mật api bằng JWT authentication
    [Authorize]
    public class ReservationController : ControllerBase
    {
        //Dummy data
        public List<Reservation> CreateDummyReservations()
        {
            List<Reservation> rList = new List<Reservation> {
            new Reservation { Id=1, Name = "Ankit", StartLocation = "New York", EndLocation="Beijing" },
            new Reservation { Id=2, Name = "Bobby", StartLocation = "New Jersey", EndLocation="Boston" },
            new Reservation { Id=3, Name = "Jacky", StartLocation = "London", EndLocation="Paris" }
            };
            return rList;
        }

        [HttpGet]
        public IEnumerable<Reservation> Get() => CreateDummyReservations();

    }
}

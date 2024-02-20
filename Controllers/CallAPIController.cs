using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using UploadFileToWebAPI.Models;

namespace UploadFileToWebAPI.Controllers
{
    public class CallAPIController : Controller
    {


        public IActionResult Index(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        public List<Users> CreateDummyUsers()
        {
            List<Users> userList = new List<Users> 
            {
                new Users { Username = "jack", Password = "jack", Role = "Admin" },
                new Users { Username = "donald", Password = "donald", Role = "Manager" },
                new Users { Username = "thomas", Password = "thomas", Role = "Developer" }
            };
            return userList;
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            Users loginUser = CreateDummyUsers().Where(a => a.Username == username && a.Password == password).FirstOrDefault();
            if (loginUser == null)
                return View((object)"Login Failed");

            var claims = new[] {
                new Claim(ClaimTypes.Role, loginUser.Role)
            };

            var accessToken = GenerateJSONWebToken(claims);//tạo token
            this.SetJWTCookie(accessToken);

            return RedirectToAction("FlightReservation");
        }

        private string GenerateJSONWebToken(Claim[] claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("counterlogic_counterlogiccounterlogic_counterlogic"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //tạo token cho client issuer và audience phải giống với config của JWT ở progam.cs
            var token = new JwtSecurityToken(
                issuer: "https://counterlogic.com",
                audience: "dotnetclient",
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: credentials,
                claims : claims
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private void SetJWTCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(1),
            };

            //lưu cookie vào trong response
            Response.Cookies.Append("jwtCookie", token, cookieOptions);
        }



        public async Task<IActionResult> FlightReservation()
        {
            //khi gọi tới api này thì nó lấy cookie trong request nó gửi tới
            var jwt = Request.Cookies["jwtCookie"];

            List<Reservation> reservationList = new List<Reservation>();

            using (var httpClient = new HttpClient())
            {
                //add jwt token vào authentication header 
                //nó sẽ hiểu là scheme là Bearer vì ta đã config nó trong program.cs
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, jwt);
                //đoạn code này đang giả lập client gọi tới api
                using (var response = await httpClient.GetAsync("https://localhost:7292/Reservation")) // change API URL to yours 
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();

                        //biến chuỗi JSON nhận về thành class trong c#
                        reservationList = JsonConvert.DeserializeObject<List<Reservation>>(apiResponse);
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Index", new { message = "Please Login again" });
                    }
                }
            }

            return View(reservationList);
        }


    }
}

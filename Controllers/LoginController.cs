using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using JwtTokenAndCORS_example.Controllers;

using JwtTokenAndCORS_example.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
namespace JwtTokenAndCORS_example.Controllers
{
    //jwt refresh token có chức năng làm cho người dùng không cần đăng nhập lại 
    //jwt access token có chức năng xác thực người dùng
    //jwt chỉ hết hạn thi user đã lâu không sử dụng chương trình => người dùng cần đăng nhập lại

    //tư tưởng chủa jwt refresh token là 
    //khi người dụng đăng nhập vào chương trình sẽ 2 tạo ra 2 token : jwt access token và jwt refresh token 
    //2 token này thời gian tồn tại riêng (access < refresh)
    //khi người dùng truy cập vào 1 tài nguyên nào đó thì ta sẽ gán jwt access vào header của request gửi tới server
    //server sẽ kiểm tra jwt này có hợp lệ không 
    //nếu hợp lệ thì ok
    //ngược lại nếu thì jwt này hết hạn hoặc bị sai(chỉ sai khi có người đang hack token )
    //nếu trường hợp là hết hạn thì cần tạo lại token mới => người dùng cần phải login lại
    //do đó jwt refresh token ra đời 
    //ta kiểm tra jwt refresh token còn sử dụng được không(kiểm tra bằng cách check có match với cái lưu trên db k chẵng hạn)
    //nếu còn sử dụng được thì ra sẽ tạo ra 1 jwt access token mới và đồng thời tạo mới jwt refresh token luôn
    //và người dụng tiếp tục sử dụng chương trình 

    public class LoginController : Controller
    {
        public IActionResult Index(string username, string password)
        {
            if ((username != "secret") || (password != "secret"))
                return View((object)"Login Failed");

            var accessToken = GenerateJSONWebToken();//khi đăng nhập tạo 1 jwt access token và jwt refresh token
            return RedirectToAction("FlightReservation", new { token = accessToken });
        }

        public async Task<IActionResult> FlightReservation(string token)//action này lấy JWT access token
        {
            List<Reservation> reservationList = new List<Reservation>();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
                using (var response = await httpClient.GetAsync("https://localhost:7292/Reservation")) // change API URL to yours 
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        reservationList = JsonConvert.DeserializeObject<List<Reservation>>(apiResponse);
                    }

                    //nếu không xác thực được thì check refreshToken
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("RefreshToken");
                    }
                }
            }

            return View(reservationList);
        }

        //hàm này đảm bảo rằng Refresh Token hợp lệ sao đó tạo ra 1 JWT access token mới(do đó người dùng k cần login lại)
        public IActionResult RefreshToken()
        {
            string cookieValue = Request.Cookies["refreshToken"]; //lấy JWT refresh token

            // nếu không có cookie thì đăng nhập lại
            if (cookieValue == null)
                return RedirectToAction("Index");

            //nếu có cookie mà cookie không giống cái lưu trong db thì cũng đăng nhập lại
            if (!CheckCookieValue(cookieValue))
                return RedirectToAction("Index");

            // If cookie is revoked by admin
            if (!CheckCookieEnabled(cookieValue))
                return RedirectToAction("Index");

            var newToken = GenerateJSONWebToken(); //tạo 1 jwt access toke mới và jwt refresh token được làm mới lại
            return RedirectToAction("FlightReservation", new { token = newToken });
        }

        private bool CheckCookieValue(string cookieValue)
        {
            // Check the cookie value with stored in the db. If No match then it is forged cookie so return false.
            return true;
        }

        private bool CheckCookieEnabled(string cookieValue)
        {
            // Check if the cookie is enabled in the database. If cookie is not enabled then probably the admin has revoked it so return false.
            return true;
        }

        private string GenerateJSONWebToken()
        {
            string rt = CreateRefreshToken();
            SetRefreshTokenToCookie(rt);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("counterlogic_counterlogiccounterlogic_counterlogic"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "https://counterlogic.com",
                audience: "dotnetclient",
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private string CreateRefreshToken()
        {
            var randomNumber = new byte[32];//32 byte là 1 kí tự => 32byte là 32 kí tự
            using (var generator = new RNGCryptoServiceProvider())
            {
                generator.GetBytes(randomNumber);
                string token = Convert.ToBase64String(randomNumber);
                return token;
            }
        }

        private void SetRefreshTokenToCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(2), 
                //thòi gian tồn tại của cookie này là 2 phút 
                //mà cookie này chứa JWT Refresh token 
                //nếu cookie mât thì, JWT fresh token này cũng mất theo
                //nhưng cookie này chỉ mất khi đã lâu user chưa sử dụng lại chương trình
                //nếu user sử dụng chương trình liên tục thì JWT refresh token sẽ được tạo mới liên tục
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}

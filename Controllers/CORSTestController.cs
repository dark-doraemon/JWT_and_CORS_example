using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace UploadFileToWebAPI.Controllers
{
    [EnableCors("MyPolicy")] // chũng ta có thể áp dụng CORS cho từng controller 
    public class CORSTestController : ControllerBase
    {
        [EnableCors("MyPolicy")] // hoặc cho từng action
        public string Index()
        {
            return "Hello World!";
        }


        [DisableCors] //hoặc không áp dụng CORS 
        public string Index2()
        {
            return "Xin chao";
        }
    }
}

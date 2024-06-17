using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Reddit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        [Authorize]
        [HttpGet]
        [Route("TestAuthorize")]
        public string TestAuthorize()
        {
            return "Test Authorize perfecto";
        }


        [HttpGet]
        [Route("AllowAnonynous")]
        public string AllowAnonynous()
        {
            return "Test Anonymous perfecto";
        }
    }
}

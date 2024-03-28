using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        [HttpGet]
        public string getShop()
        {
            return "Shop";
        }

        [HttpGet]
        [Route("getShopById")]
        public string getShopById(int id)
        {
            return "Shop " + id;
        }
    }
}


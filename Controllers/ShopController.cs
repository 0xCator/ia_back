using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace WebAPI1.Controllers
{
    [Authorize]
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

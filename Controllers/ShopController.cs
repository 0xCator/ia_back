using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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
        [HttpGet]
        [Route("dbTest")]
        public string dbTest()
        {
            string connectionString = "Server=tcp:iaserver.database.windows.net,1433;Initial Catalog=IA;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";
            string result = "test";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                result = "Connection Opened";
                connection.Close();
            }
            
            return result;
        }   
    }
}


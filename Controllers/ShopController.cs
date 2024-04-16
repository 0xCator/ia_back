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
        public List<String> dbTest()
        {
            string connectionString = "Server=tcp:iaserver.database.windows.net,1433;Initial Catalog=IA;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";

            // Table would be created ahead of time in production
            var rows = new List<string>();

            using var conn = new SqlConnection(connectionString);
            conn.Open();

            var command = new SqlCommand("SELECT * FROM Users", conn);
            using SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    rows.Add($"{reader.GetInt32(0)}, {reader.GetString(1)}, {reader.GetString(2)}");
                }
            }

            return rows;
            
        }   
    }
}

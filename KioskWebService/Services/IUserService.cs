using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskWebService.Entities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KioskWebService.Services
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
        Task<IEnumerable<User>> GetAll();
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications

        private readonly IConfiguration _configuration;
        private static string username = "", password = "";
        public UserService(IConfiguration configuration) 
        {
            _configuration = configuration;
            var cred = _configuration.GetSection("BasicAuthenticationCredentials");
            var cred1 = Serialize(cred).ToString().Split(';');

            var getCred = JsonConvert.DeserializeObject<dynamic>(cred1.GetValue(0).ToString().Replace("\r\n", ""));

            password = getCred.password.Value;
            username = getCred.username.Value;
        }

        private List<User> _users = new List<User>
        {
            new User { Id = 1, FirstName = "ML", LastName = "KIOSK", Username = username, Password = password }
        };

        public async Task<User> Authenticate(string username, string password)
        {
            // wrapped in "await Task.Run" to mimic fetching user from a db
            var user = await Task.Run(() => _users.SingleOrDefault(x => x.Username == username && x.Password == password));

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so return user details
            return user;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            // wrapped in "await Task.Run" to mimic fetching users from a db
            return await Task.Run(() => _users);
        }

        public JToken Serialize(IConfiguration config)
        {
            JObject obj = new JObject();
            foreach (var child in config.GetChildren())
            {
                obj.Add(child.Key, Serialize(child));
            }

            if (!obj.HasValues && config is IConfigurationSection section)
                return new JValue(section.Value);

            return obj;
        }
    }
}

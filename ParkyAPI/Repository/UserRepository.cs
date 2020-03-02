using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ParkyAPI.Data;
using ParkyAPI.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ParkyAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDBContext _userDb;

        //This is to get out secret from appsettings.json
        private readonly IConfiguration _config; 
        public UserRepository(ApplicationDBContext userDb, IConfiguration config)
        {
            _userDb = userDb;
            _config = config;
        }
        public User Authenticate(string username, string password)
        {
            //Retrieve the user that matches the given user name and password
            var user = _userDb.Users.SingleOrDefault(x => x.Username == username && x.Password == password);

            //If this user does not exist, return NULL
            if (user == null)
                return null;
            
            //If the user was found, generate and set up an Authentication Token that user

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.GetSection("AppSettings:Token").Value);

            //Set token description
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                //Token expiration
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key)
                ,SecurityAlgorithms.HmacSha256Signature)
            };
            //Create the token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
            user.Password = "";

            return user;
        }

        public bool IsUniqueUser(string username)
        {
            //Retrieve the user that matches the given user name and password
            var user = _userDb.Users.SingleOrDefault(x => x.Username == username);

            //If this user does not exist, return NULL
            if (user == null)
                return true;

            return false;
        }

        public User Register(string username, string password)
        {
            User userObj = new User()
            {
                Username = username,
                Password = password,
                Role = "Admin"
            };

            _userDb.Users.Add(userObj);
            _userDb.SaveChanges();
            userObj.Password = "";
            return userObj;
        }
    }
}

    using Microsoft.AspNetCore.Mvc;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Caching.Distributed;
    using System.IdentityModel.Tokens.Jwt;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using Microsoft.AspNetCore.Http;

    using BE.Models;
using System.Text.Json.Serialization;
namespace MyApp.Namespace
    {



    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UsersDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly IDistributedCache _redisCache;
        private readonly CookieOptions _cookieConf;
        public AuthController(IConfiguration configuration, UsersDbContext context, IMemoryCache cache, IDistributedCache distirbutedCache)
        {
            _configuration = configuration;
            _context = context;
            _cache = cache;
            _cookieConf = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                SameSite = SameSiteMode.Lax,  
                Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                Path = "/"
            };
            _redisCache = distirbutedCache;
        }
        [HttpPost("register")]
            public IActionResult Register([FromBody] LoginModel login)
            {
                JsonSerializerOptions _jsonSerializerOptions = new()
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                };
                var hasher = new PasswordHasher<User>();
                var _usr = _context.Users.ToList().FirstOrDefault(x => x.username == login.username);
                System.Console.WriteLine(hasher.HashPassword(_usr,login.passwd));
                if(_usr==null)
                {
                    _usr = new User(){username = login.username,passwdhash=hasher.HashPassword(_usr,login.passwd),role=0};
                    _context.Users.Add(_usr);
                    _cache.Set(login.username,_usr,new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                    _redisCache.SetString(_usr.id.ToString(),JsonSerializer.Serialize(_usr),new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });
                    _redisCache.SetString(login.username,_usr.id.ToString(),new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });
                    _context.SaveChanges(); 
                    var token = GenerateToken(_usr.id.ToString(), 0);
                    Response.Cookies.Append("userId", _usr.id.ToString(), _cookieConf);
                    _usr.passwdhash = ""; 
                    CheckIn();
                    return Ok(new { token, _usr });
                }
                return Unauthorized(new { message = "User already exists." });
        
            }
            
        [HttpPost("login")]
            public IActionResult Login([FromBody] LoginModel login)
            {

                var hasher = new PasswordHasher<User>();
                User? _usr;
            try
            {
                var usrId = _redisCache.GetString(login.username);
                var usrString = usrId!=null? _redisCache.GetString(usrId) : null;
                if(usrString==null)
                {
                    throw new NullReferenceException();
                }
                    System.Console.WriteLine("GOT USER IN REDIS");
                    _usr  =JsonSerializer.Deserialize<User>(usrString);                                
            }
            catch
            {
                    _cache.TryGetValue(login.username, out _usr);
                    if(_usr==null)
                        _usr = _context.Users.ToList().FirstOrDefault(x => x.username == login.username);                
            }

                System.Console.WriteLine(hasher.HashPassword(_usr,login.passwd));
                if(_usr!=null)
                {
                    var hashCheck = hasher.VerifyHashedPassword(_usr, _usr.passwdhash, login.passwd);
                    //double check (if user is not an admin then it will pass anyway)
                    Admin? usrAdminCheck = _context.Admins.ToList().FirstOrDefault(x=>x.id==_usr.id);
                    bool adminDoubleCheck = (_usr.id==(usrAdminCheck!=null?usrAdminCheck.id:-1)) || (_usr.role == 0); 
                    System.Console.WriteLine(usrAdminCheck.id + " | "+_usr.id + " | "+adminDoubleCheck);

                    if (hashCheck == PasswordVerificationResult.Success && adminDoubleCheck)
                    {
                        _cache.Set(login.username,_usr,new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                        _redisCache.SetString(_usr.id.ToString(),JsonSerializer.Serialize(_usr),new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                        });
                        _redisCache.SetString(login.username,_usr.id.ToString(),new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                        });
                        var token = GenerateToken(_usr.id.ToString(), _usr.role);
                        Response.Cookies.Append("userId", _usr.id.ToString(), _cookieConf);
                        CheckIn();
                        _usr.passwdhash = "";
                        return Ok(new { token, _usr });
                    }
                }

                return Unauthorized(new { message = "Invalid credentials" });
            }

        [HttpDelete("delete")]
        [Authorize(Roles = "Administrator")]
        public IActionResult PurgeUser([FromBody] LoginModel login)
            {
                var _usr = _context.Users.ToList().FirstOrDefault(x => x.username == login.username);
                if(_usr!=null)
                {
                    _context.Users.Remove(_usr);
                    return Ok(new{message = $"Removed {login.username} successfully"});
                }
                return NotFound(new{message = $"Could not find {login.username} in database"});
            }
        [HttpGet("getUser")]
        [Authorize(Roles = "User")]
        public IActionResult getUser(int _id)
            {
                var _usr = _context.Users.ToList().FirstOrDefault(x => x.id == _id);
                if(_usr!=null)
                {
                    _usr.passwdhash = "";
                    return Ok(_usr);
                }
                return NotFound(new{message = $"Could not find user with id {_id} in database"});
            }
        [HttpGet("getUsers")]
        [Authorize(Roles = "Administrator")]
        public IActionResult getUsers()
            {
                return Ok(_context.Users.ToList());
            }
        [HttpGet("checkOut")]
        [Authorize(Roles = "User")]
        public IActionResult CheckOut()
            {
                TaskMetrics.UsersOnline.Add(-1);
                return Ok();
            }
        [HttpGet("checkIn")]
        [Authorize(Roles = "User")]
        public IActionResult CheckIn()
            {
                TaskMetrics.UsersOnline.Add(1);
                return Ok();
            }
        private string GenerateToken(string username, int role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.Role, role==1 ? "Administrator" : "")
            };
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddSeconds(30),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateRefreshToken(int length = 32)
        {
            var randomNumber = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        }
    }

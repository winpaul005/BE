
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Distributed;
using BE.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Diagnostics.Metrics;
using System.Text.Json.Serialization;
namespace MyApp.Namespace
{
[ApiController]
[Route("api/[controller]")]
[Authorize]
    public class HomeController : Controller
    {
    private readonly IMemoryCache _cache;
    private readonly UsersDbContext _context;

    private readonly IDistributedCache _redisCache;
    public HomeController(UsersDbContext context, IMemoryCache cache , IDistributedCache distributedCache)
    {
        _cache = cache;
        _redisCache = distributedCache;
        _context = context;
    }
    [HttpGet]
    [AllowAnonymous]
        public IActionResult Index()
        {
            var usrname = User.Identity?.Name;
            if(usrname!=null)
                return Ok();
            return NotFound();
        }
    [HttpGet]
    [Route("getTasks")]
    [Authorize(Roles = "User")]
    public IActionResult GetTasks()
    {
        return Ok(_context.Tasks);
    }
    [HttpGet]
    [Route("getAvatar")]
    [Authorize(Roles = "User")]
    public IActionResult GetAvatar()
    {
                JsonSerializerOptions _jsonSerializerOptions = new()
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                };
        int usrname = int.Parse(User.Identity?.Name);
        User? _usr;
        if(usrname!=null)
        {
            _usr = _context.Users.First(x => x.id == usrname);
            if(_usr!=null)
            {
                UserStatus? _status = JsonSerializer.Deserialize<UserStatus>(_usr.status, _jsonSerializerOptions);
                return Ok(_status.avatarString);
                
            }
            return NotFound();
        }
        return NotFound();
        
    }
    [HttpPost]
    [Route("setAvatar")]
    [Authorize(Roles = "User")]
    public IActionResult SetAvatar(string _encodedImage)
    {
                JsonSerializerOptions _jsonSerializerOptions = new()
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                };
        int usrname = int.Parse(User.Identity?.Name);
        User? _usr;
        if(usrname!=null)
        {
            _usr = _context.Users.First(x => x.id == usrname);
            if(_usr!=null)
            {
                UserStatus? _status = JsonSerializer.Deserialize<UserStatus>(_usr.status, _jsonSerializerOptions);
                _status.avatarString = _encodedImage;
                _usr.status = JsonSerializer.Serialize<UserStatus>(_status,_jsonSerializerOptions);
                _context.Entry(_usr).Property(x => x.status).IsModified = true;
                _context.SaveChanges();
                return Ok(_status.avatarString);
                
            }
            return NotFound();
        }
        return NotFound();
        
    }

    public void AddLog(Log _log)
        {
            System.Console.WriteLine("New log!");
            _context.Logs.Add(_log);
            _context.SaveChanges();
        }
    [HttpGet]
    [Route("getLogs")]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetLogs()
        {
            return Ok(_context.Logs.ToList());
        }
    [HttpGet]
    [Route("addCounter")]
    [Authorize(Roles = "User")]
    public IActionResult AddCounter()
    {
                JsonSerializerOptions _jsonSerializerOptions = new()
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                };
        int usrname = int.Parse(User.Identity?.Name);
        User? _usr;
        if(usrname!=null)
            {
                _usr = _context.Users.First(x => x.id == usrname);
                if(_usr!=null)
                {
                    UserStatus? _status = JsonSerializer.Deserialize<UserStatus>(_usr.status, _jsonSerializerOptions);
                    _usr.status = _status!=null? JsonSerializer.Serialize<UserStatus>(new UserStatus(){completedTasks = _status.completedTasks+1},_jsonSerializerOptions) : JsonSerializer.Serialize<UserStatus>(new UserStatus(),_jsonSerializerOptions);
                    _context.Entry(_usr).Property(x => x.status).IsModified = true;
                    _context.SaveChanges();
                    return Ok(_usr.status);
                }
            }
        return Unauthorized();
    }
    [HttpPost]
    [Route("addTask")]
    [Authorize(Roles = "Administrator")]
    public IActionResult AddTask([FromBody] Tasque _task)
    {
        _context.Tasks.Add(_task);
        _context.SaveChanges();
        AddLog(new Log(){message = $"New task (id {_task.id} : {_task.title}) was added by {_context.Users.ToList().FirstOrDefault(x=>x.id==_task.ownerid).username}"});
        return Ok(_context.Tasks);
    }
        [HttpPost]
        [Route("reportTasks")]
        [Authorize(Roles = "User")]
        public IActionResult ReportTask([FromBody] TaskReportDto request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null");
            }
            
            var _tsk = _context.Tasks.FirstOrDefault(x => x.id == request.idx);
            if (_tsk == null)
            {
                return NotFound($"Task with id {request.idx} not found");
            }
            
            var usrname = User.Identity;
            if (usrname != null)
            {
                ClaimsIdentity? usr_ident = usrname as ClaimsIdentity;
                if (usr_ident != null)
                {
                    var usrIdClaim = usr_ident.FindFirst(ClaimTypes.NameIdentifier);
                    if (usrIdClaim != null && int.TryParse(usrIdClaim.Value, out int userId))
                    {
                        if (_tsk.status != null && _tsk.status.assignedUsers.Contains(userId))
                        {   
                            _tsk.status.isClosed = true;
                            _context.Entry(_tsk).Property(x => x.status).IsModified = true;
                            _context.Props.ToList()[0].completedtasks += 1;
                            _context.SaveChanges();
                            
                            // Use the metrics class instead
                            TaskMetrics.TasksCompleted.Add(1);
                            AddLog(new Log(){message = $"User {userId} finished the task {_tsk.id}"});
                            
                            return Ok(_context.Tasks);
                        }
                        return NotFound("User not assigned to this task");
                    }
                }
                return NotFound("User identity not found");
            }
            return Unauthorized();
        }
    [HttpDelete]
    [Route("deleteTask")]
    [Authorize(Roles = "Administrator")]
    public IActionResult DeleteTask(int taskId)
    {
        var _task = _context.Tasks.ToList().FirstOrDefault(x => x.id == taskId);
        if(_task!=null)
        {
            _context.Tasks.Remove(_task);
            _context.Props.ToList()[0].completedtasks -=1;
            _context.SaveChanges();
            AddLog(new Log(){message = $"Task {taskId} was deleted"});

            return Ok();
        }
        return NotFound();
    }
    [HttpGet]
    [OutputCache]
    [Route("getRole")]
    public IActionResult GetRole()
    {
        var usrname = User.Identity;
        if(usrname!=null)
        {
            ClaimsIdentity? usr_ident =(usrname as ClaimsIdentity);
            var roles = usr_ident.FindAll(usr_ident.RoleClaimType).Select(c=>c.Value);
            return Ok(roles);
        }
        return NotFound();
    }
    [HttpGet]
    [OutputCache]
    [Route("getStats")]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetStats()
    {
        var res = _context.Props.ToList()[0];
        if(res!=null)
            return Ok(res);
        return NotFound();
    }
    [HttpGet]
    [Route("sessionTest")]
    [AllowAnonymous]
        public IActionResult Sessiontest()
        {
            var usrname = User.Identity?.Name;
            if(usrname!=null)
                return Ok();
            return Unauthorized();
        }
    [HttpGet("checkRefresh")]
    public IActionResult CheckRefresh()
    {
            var clientToken = Request.Cookies["refresh-token"].FirstOrDefault();
            if (clientToken!=null)
            {
                return _redisCache.Get(clientToken.ToString())!=null ? Ok() : Unauthorized();
            }
            return BadRequest();
    }
    [HttpPost]
        public IActionResult Create([FromBody]UserModel _user)
        {
            if(_user.name.Length > 10)
                ModelState.AddModelError("FATAL","Maximum length (10) exceeded");
            if(ModelState.IsValid)
                return Ok(_user);
            return NotFound();
        }
    }

}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace react_playground.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AccountController : ControllerBase
{
    //-- presunout do appsettings
    public const string KEY = "Decently long and strong key";
    public const string ISSUER = "http adress 1";
    public const string AUCIENCE = "http adress 2";
    public const string AUTH_TYPE = "OurAuthClaimType";

    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult SignIn(SignInDTO dto)
    {
        WriteJWT(dto, HttpContext);
        Console.WriteLine($"auth: {dto.Login} {dto.Name} {dto.Role}");
        return Ok();
    }

    public static void WriteJWT(SignInDTO dto, HttpContext context)
    {
        SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY)); //-- klic pro zasifrovani JWT
        SigningCredentials credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
        JwtHeader header = new JwtHeader(credentials);
        JwtPayload payload = new JwtPayload(ISSUER, AUCIENCE, new List<Claim>() //-- nezasifrovany payload, umoznuje pristup k rolim na klientovi
        {
            new Claim(ClaimTypes.NameIdentifier, dto.Login),
            new Claim(ClaimTypes.Name, dto.Name),
            new Claim(ClaimTypes.Role, dto.Role),
            new Claim("OurAuthClaimType", "JWT")

        }, null, DateTime.Today.AddDays(1));
        JwtSecurityToken securityToken = new JwtSecurityToken(header, payload);

        string token = new JwtSecurityTokenHandler().WriteToken(securityToken); //-- prevod tokenu na string
        context.Response.Cookies.Append("jwt", token, new CookieOptions //-- zapsani tokenu do cookie
        {
            HttpOnly = true //-- zajisteni, ze ke cookie nelze pristopit z JS v prohlizeci (i kdyz JWT zajistuje integritu dat)
        });

        //-- zapsani role do cookie pristupne z JS pro podminene generovani GUI 
        //-- pokud si to nekdo v JS prepise, vygeneruje se mu treba vice tlacitek/odkazu, ale ty stejne budou nefunkci, 
        //-- protoze kliknuti na ne povede na dotaz na server, ktery vrati 403 Unauthorized
        context.Response.Cookies.Append("roles", dto.Role, new CookieOptions 
        { 
            SameSite = SameSiteMode.Strict, 
            Secure = true
        }); 
    }

    public static void DeleteJWT(HttpContext context)
    {
        if (context.Request.Cookies.Any(x => x.Key == "jwt"))
        {
            context.Response.Cookies.Append("jwt", "", new CookieOptions
            {
                Expires = DateTime.MinValue, // expirovana cookie, browser ji smaze
                HttpOnly = true
            });
        }
    }
    
    [HttpGet]
    public SignInDTO Get()
    {
        Console.WriteLine("auth");
        return new SignInDTO
        {
            Login = ":D",
            Password = ":)"
        };
    }
}

public class SignInDTO
{
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
}

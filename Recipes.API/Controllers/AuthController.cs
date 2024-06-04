using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Recipes.BLL.Configurations;
using Recipes.BLL.Interfaces;
using Recipes.BLL.Services.Interfaces;
using Recipes.DAL;
using Recipes.Data.DataTransferObjects.UserDTOs;
using Recipes.Data.Models;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Recipes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RecipesContext context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender;
        private readonly GoogleClientConfiguration googleClientConfiguration;
        public AuthController(ITokenService tokenService, UserManager<User> userManager, SignInManager<User> signInManager, IEmailSender emailSender, GoogleClientConfiguration googleClientConfiguration, RecipesContext context)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            this.googleClientConfiguration = googleClientConfiguration;
            this.context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName= registerDto.FirstName,
                LastName= registerDto.LastName,
                Avatar=registerDto.Avatar
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!roleResult.Succeeded)
                return BadRequest(roleResult.Errors);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Email);

            if (user == null)
                return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
                return Unauthorized();

            var token = await _tokenService.GenerateTokenAsync(user);
            return Ok(new { Token = token });
        }

        [HttpPost("loginWithGoogle")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] string credentials)
        {
            var setings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { googleClientConfiguration.GoogleClientID }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credentials,setings);

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
                return Unauthorized();

            var token = await _tokenService.GenerateTokenAsync(user);
            return Ok(new { Token = token });

        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody]UserInfo userInfo)
        {

            var user = await _userManager.FindByIdAsync(userInfo.Id.ToString());

            if (user == null)
                return Unauthorized();

            if(!string.IsNullOrEmpty(userInfo.Avatar))
                user.Avatar = userInfo.Avatar;
            if(!string.IsNullOrEmpty(userInfo.FirstName))
                user.FirstName = userInfo.FirstName;
            if (!string.IsNullOrEmpty(userInfo.LastName))
                user.LastName = userInfo.LastName; 
            if (!string.IsNullOrEmpty(userInfo.Avatar))
                user.Avatar = userInfo.Avatar;

            await context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<GetUserDto>> GetUserData()
        {
            var username = User.FindFirst("userName")?.Value;

            var user = await context.Users.Where(x => x.UserName == username).Include(x=>x.SavedRecipes).FirstOrDefaultAsync();

            if (user == null)
                return Unauthorized();

            GetUserDto userDTO = new GetUserDto()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Avatar = user.Avatar,
                Id = user.Id,
                SavedRecipes = user.SavedRecipes.Count
            };

            return Ok(userDTO);
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromQuery] Guid Id, [FromQuery] string Code, [FromQuery] string newpas)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.Select(x => x.Errors.FirstOrDefault().ErrorMessage));

            Code = Code.Replace('&', '/');
            var request = new ResetPasswordRequest() { Id = Id, Code = Code, NewPasword = newpas };
            try
            {
                var user = await _userManager.FindByIdAsync(request.Id.ToString());
                if (user == null)
                {
                    return Unauthorized();
                }
                var result = await _userManager.ResetPasswordAsync(user, request.Code, request.NewPasword);
                if (!result.Succeeded)
                {
                    return Unauthorized();
                }
                return Ok();
            }
            catch (ArgumentException e)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { e.Message });
            }
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string Email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(Email);
                if (user == null)
                    return Unauthorized();

                var code = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);

                code = code.Replace('/','&');
   
                var callbackUrl = $"http://localhost:4200/password-reset-form/{user.Id}/{code}";
                await _emailSender.SendEmailAsync(user.Email, "Reset password", callbackUrl);
                return Ok();
            }
            catch (ArgumentException e)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { e.Message });
            }
        }



    }
}

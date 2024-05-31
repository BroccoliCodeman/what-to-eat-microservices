using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipes.BLL.Interfaces;
using Recipes.BLL.Services.Interfaces;
using Recipes.Data.DataTransferObjects.UserDTOs;
using Recipes.Data.Models;

namespace Recipes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender;
        public AuthController(ITokenService tokenService, UserManager<User> userManager, SignInManager<User> signInManager, IEmailSender emailSender)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
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

          /*  if (!roleResult.Succeeded)
                return BadRequest(roleResult.Errors);*/

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

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromQuery] Guid Id, [FromQuery] string Code, [FromBody] string newpas)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.Select(x => x.Errors.FirstOrDefault().ErrorMessage));
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

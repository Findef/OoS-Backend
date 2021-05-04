using System;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.EmailService;
using OutOfSchool.IdentityServer.ViewModels;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Controllers
{
    /// <summary>
    /// Handles authentication.
    /// Contains methods for log in and sign up.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly IIdentityServerInteractionService interactionService;
        private readonly ILogger<AuthController> logger;
        private readonly IEmailSender emailSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userManager"> ASP.Net Core Identity User Manager.</param>
        /// <param name="signInManager"> ASP.Net Core Identity Sign in Manager.</param>
        /// <param name="interactionService"> Identity Server 4 interaction service.</param>
        /// <param name="logger"> ILogger class.</param>
        /// <param name="emailSender"> IEmailSender class.</param>
        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IIdentityServerInteractionService interactionService,
            ILogger<AuthController> logger,
            IEmailSender emailSender)
        {
            this.logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.interactionService = interactionService;
            this.emailSender = emailSender;
        }

        /// <summary>
        /// Logging out a user who is authenticated.
        /// </summary>
        /// <param name="logoutId"> Identifier of cookie captured the current state needed for sign out.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await signInManager.SignOutAsync();

            var logoutRequest = await interactionService.GetLogoutContextAsync(logoutId);

            if (string.IsNullOrEmpty(logoutRequest.PostLogoutRedirectUri))
            {
                throw new NotImplementedException();
            }

            return Redirect(logoutRequest.PostLogoutRedirectUri);
        }

        /// <summary>
        /// Generates a view for user to log in.
        /// </summary>
        /// <param name="returnUrl"> URL used to redirect user back to client.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = "Login")
        {
            var externalProviders = await signInManager.GetExternalAuthenticationSchemesAsync();
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalProviders = externalProviders,
            });
        }

        /// <summary>
        /// Authenticate user based on model.
        /// </summary>
        /// <param name="model"> View model that contains credentials for logging in.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(new LoginViewModel
                {
                    ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                });
            }

            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

            if (result.Succeeded)
            {
                return string.IsNullOrEmpty(model.ReturnUrl) ? Redirect(nameof(Login)) : Redirect(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                return BadRequest();
            }

            bool emailConfirmed = await userManager.IsEmailConfirmedAsync(new User {UserName = model.Username});
            if (!emailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Email is not confirmed");  
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Login or password is wrong");
            }

            return View(new LoginViewModel
            {
                ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = model.ReturnUrl,
            });
        }

        /// <summary>
        /// Generates a view for user to register.
        /// </summary>
        /// <param name="returnUrl"> URL used to redirect user back to client.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public IActionResult Register(string returnUrl = "Login")
        {
            return View(
                new RegisterViewModel { ReturnUrl = returnUrl });
        }

        /// <summary>
        /// Creates user based on model.
        /// </summary>
        /// <param name="model"> View model that contains credentials for signing in.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User()
            {
               UserName = model.Email,
               FirstName = model.FirstName,
               LastName = model.LastName,
               MiddleName = model.MiddleName,
               Email = model.Email,
               PhoneNumber = model.PhoneNumber,
               CreatingTime = DateTime.Now,
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                IdentityResult roleAssignResult = IdentityResult.Failed();
                if (Request.Form["Provider"].Count == 1)
                {
                    roleAssignResult = await userManager.AddToRoleAsync(user, "provider");
                }
                else
                if (Request.Form["Parent"].Count == 1)
                {
                    roleAssignResult = await userManager.AddToRoleAsync(user, "parent");
                }

                if (roleAssignResult.Succeeded)
                {
                    //await signInManager.SignInAsync(user, false);
                    //return Redirect(model.ReturnUrl);
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { token, email = user.Email, model.ReturnUrl }, Request.Scheme);
                    var message = new Message(new string[] { user.Email }, "Confirmation email link", confirmationLink);
                    await emailSender.SendEmailAsync(message);
                    return RedirectToAction(nameof(SuccessRegistration));
                }

                var deletionResult = await userManager.DeleteAsync(user);
                if (!deletionResult.Succeeded)
                {
                    logger.Log(LogLevel.Warning, "User was created without role");
                }

                foreach (var error in roleAssignResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {                   
                    if (error.Code == "DuplicateUserName")
                    {
                        error.Description = $"Email {error.Description.Substring(10).Split('\'')[0]} is alredy taken";                     
                    }

                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string email, string returnUrl)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return View("Error");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Redirect(returnUrl);
            }
            else
            {
                return View("Error");
            }
            
           //return View(result.Succeeded ? nameof(ConfirmEmail) : "Error");
        }

        [HttpGet]
        public IActionResult SuccessRegistration()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }

        public Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            throw new NotImplementedException();
        }
    }
}
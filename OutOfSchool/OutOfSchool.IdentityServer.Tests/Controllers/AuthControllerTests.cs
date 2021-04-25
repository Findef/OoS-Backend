using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.IdentityServer.Controllers;
using OutOfSchool.IdentityServer.ViewModels;
using OutOfSchool.Services.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using ILogger = Serilog.ILogger;

namespace OutOfSchool.IdentityServer.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private readonly Mock<FakeUserManager> fakeUserManager;
        private readonly Mock<FakeSignInManager> fakeSignInManager;
        private readonly Mock<IIdentityServerInteractionService> fakeInteractionService;
        private readonly Mock<ILogger> fakeLogger;
        private AuthController authController;

        public AuthControllerTests()
        {
            fakeUserManager = new Mock<FakeUserManager>();
            fakeInteractionService = new Mock<IIdentityServerInteractionService>();
            fakeSignInManager = new Mock<FakeSignInManager>();
            fakeLogger = new Mock<ILogger>();
        }

        [SetUp]
        public void Setup()
        {
            authController = new AuthController(
                fakeUserManager.Object,
                fakeSignInManager.Object,
                fakeInteractionService.Object, 
                fakeLogger.Object);
        }

        [Test]
        public async Task Logout_WithLogoutId_ReturnsRedirectResult()
        {
            // Arrange
            var logoutRequest = new LogoutRequest("iFrameUrl", new LogoutMessage())
            {
                PostLogoutRedirectUri = "True logout id"
            };
            fakeSignInManager.Setup(manager => manager.SignOutAsync())
                .Returns(Task.CompletedTask);
            fakeInteractionService.Setup(service => service.GetLogoutContextAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(logoutRequest));
            // Act
            var result = await authController.Logout("Any logout ID");

            // Assert
            Assert.IsInstanceOf(typeof(RedirectResult), result);
        }

        [Test]
        public void Logout_WithoutLogoutId_ThrowsNotImplementedException()
        {
            // Arrange
            var logoutRequest = new LogoutRequest("iFrameUrl", new LogoutMessage())
            {
                PostLogoutRedirectUri = ""
            };
            fakeSignInManager.Setup(manager => manager.SignOutAsync())
                .Returns(Task.CompletedTask);
            fakeInteractionService.Setup(service => service.GetLogoutContextAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(logoutRequest));

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotImplementedException>(
                () => this.authController.Logout("Any logout ID"));
        }

        [TestCase(null)]
        [TestCase("Return url")]
        public async Task Login_WithAndWithoutReturnUrl_ReturnsViewResult(string returnUrl)
        {
            // Arrange
            var logoutRequest = new LogoutRequest("iFrameUrl", new LogoutMessage());
            logoutRequest.PostLogoutRedirectUri = "";

            // Act
            IActionResult viewResult;
            viewResult = returnUrl == null ? await authController.Login() : await authController.Login(returnUrl);

            // Assert
            Assert.IsInstanceOf(typeof(ViewResult), viewResult);
        }

        [TestCaseSource(nameof(LoginViewModelsTestData))]
        public async Task Login_WithLoginVMAndWithModelError_ReturnsViewWithModelErrors(LoginViewModel loginViewModel)
        {
            // Arrange
            var fakeErrorMessage = "Model is invalid";
            authController.ModelState.AddModelError(string.Empty, fakeErrorMessage);
            var errorMessage = authController.ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;

            // Act
            var result = await authController.Login(loginViewModel);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(fakeErrorMessage));
            Assert.IsInstanceOf(typeof(ViewResult), result);
        }

        [TestCaseSource(nameof(LoginViewModelsWithSignInResultTestData))]
        public async Task Login_WithLoginVMWithoutModelError_ReturnsActionResult(
            LoginViewModel loginViewModel, KeyValuePair<SignInResult, Type> expectedResult)
        {
            // Arrange
            var authController = this.authController;
            fakeSignInManager.Setup(manager =>
                    manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false))
                .ReturnsAsync(expectedResult.Key);

            // Act
            var result = await authController.Login(loginViewModel);

            // Assert
            Type type = expectedResult.Value;
            Assert.IsInstanceOf(type, result);
        }

        [Test]
        public void Register_WithoutReturnUrl_ReturnsViewResult()
        {
            // Arrange 

            // Act
            IActionResult result = authController.Register();
            RegisterViewModel viewResultModel = (RegisterViewModel) ((ViewResult) result).Model;

            // Assert
            Assert.That(viewResultModel.ReturnUrl, Is.EqualTo("Login"));
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void Register_WithReturnUrl_ReturnsViewResult()
        {
            // Arrange
            var returnUrl = "Return url";

            // Act
            IActionResult result = authController.Register(returnUrl);
            RegisterViewModel viewResultModel = (RegisterViewModel) ((ViewResult) result).Model;

            // Assert
            Assert.That(viewResultModel.ReturnUrl, Is.EqualTo(returnUrl));
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public async Task Register_WithVMAndWithModelError_ReturnsViewResultWithModelErrors()
        {
            // Arrange 
            var fakeModelErrorMessage = "Model State is invalid";
            authController.ModelState.AddModelError(string.Empty, fakeModelErrorMessage);

            // Act
            var result = await authController.Register(new RegisterViewModel());
            var errorMessageFromController = authController.ModelState.Values.FirstOrDefault()
                .Errors.FirstOrDefault().ErrorMessage;

            // Assert
            Assert.That(errorMessageFromController, Is.EqualTo(fakeModelErrorMessage));
            Assert.IsInstanceOf<ViewResult>(result);
        }


        [TestCaseSource(nameof(RegisterViewModelsTestData))]
        public async Task Register_WithVMAndCreateUserError_ReturnsView(RegisterViewModel viewModel)
        {
            // Arrange 
            var error = new IdentityError()
                {Code = "User cant be created", Description = "The program failed to create user"};
            var identityResult = IdentityResult.Failed(new List<IdentityError> {error}.ToArray());

            fakeUserManager.Setup(manager => manager.CreateAsync(It.IsAny<User>(), viewModel.Password))
                .Returns(Task.FromResult<IdentityResult>(identityResult));
            fakeUserManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));
            fakeSignInManager.Setup(manager => manager.SignInAsync(
                    It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await authController.Register(viewModel);
            var errorMessageFromController = authController.ModelState.Values.FirstOrDefault()
                .Errors.FirstOrDefault().ErrorMessage;

            // Assert
            Assert.That(errorMessageFromController, Is.EqualTo(error.Description));
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void ExternalLogin_ReturnsNotImplementedEx()
        {
            // Arrange
            var authController = this.authController;

            // Assert & Act
            Assert.ThrowsAsync<NotImplementedException>(() =>
                authController.ExternalLogin("Provider", "return url"));
        }
        
        public static IEnumerable<TestCaseData> RegisterViewModelsTestData =>
            new List<TestCaseData>()
            {
                new TestCaseData(new RegisterViewModel()
                {
                    Email = "test123@gmail.com",
                    PhoneNumber = "0502391222",
                    ReturnUrl = "Return url",
                }),
                new TestCaseData(new RegisterViewModel()
                {
                    Email = "test123@gmail.com",
                    PhoneNumber = "0502391222",
                    ReturnUrl = "Return url2",
                }),
            };

        private static IEnumerable<LoginViewModel> LoginViewModels
        {
            get => new List<LoginViewModel>()
            {
                new LoginViewModel
                {
                    ReturnUrl = "Return url",
                    ExternalProviders = EmptySchemes,
                },
                new LoginViewModel {ExternalProviders = EmptySchemes},
                new LoginViewModel {ReturnUrl = "Return url"},
                new LoginViewModel(),
            };
        }

        public static IEnumerable<TestCaseData> LoginViewModelsTestData
        {
            get
            {
                foreach (var loginViewModel in LoginViewModels)
                {
                    yield return new TestCaseData(loginViewModel);
                }
            }
        }

        public static IEnumerable<TestCaseData> LoginViewModelsWithSignInResultTestData
        {
            get
            {
                foreach (var signInResult in SignInAndActionResults)
                {
                    foreach (var loginViewModel in LoginViewModels)
                    {
                        yield return new TestCaseData(loginViewModel, signInResult);
                    }
                }
            }
        }

        private static IEnumerable<KeyValuePair<SignInResult, Type>> SignInAndActionResults
        {
            get
            {
                yield return new KeyValuePair<SignInResult, Type>(SignInResult.Success, typeof(RedirectResult));
                yield return new KeyValuePair<SignInResult, Type>(SignInResult.Failed, typeof(ViewResult));
                yield return new KeyValuePair<SignInResult, Type>(SignInResult.LockedOut, typeof(BadRequestResult));
            }
        }

        private static IEnumerable<AuthenticationScheme> EmptySchemes => new List<AuthenticationScheme>();
    }
}
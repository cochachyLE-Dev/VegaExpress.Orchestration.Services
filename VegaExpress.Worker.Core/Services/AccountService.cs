using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using VegaExpress.Worker.Core.Common;
using VegaExpress.Worker.Core.Entities;
using VegaExpress.Worker.Core.Models.Auth;
using VegaExpress.Worker.Core.Models.Settings;
using VegaExpress.Worker.Core.Persistence.Enums;
using VegaExpress.Worker.Core.Services.Contracts;

namespace VegaExpress.Worker.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JWTSettings _jwtSettings;
        private readonly IDateTimeService _dateTimeService;
        private readonly IFeatureManager _featureManager;
        private readonly ICurrentUserService _currentUserService;
        public AccountService(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JWTSettings> jwtSettings,
            IDateTimeService dateTimeService,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IFeatureManager featureManager,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _dateTimeService = dateTimeService;
            _signInManager = signInManager;
            _emailService = emailService;
            _featureManager = featureManager;
            _currentUserService = currentUserService;
        }

        public async Task<Response<AuthenticationResponse>> AutenticarUsuarioAsync(AuthenticationRequest request, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new InvalidOperationException($"No Accounts Registered with {request.Email}.");
            }
            SignInResult result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (result.RequiresTwoFactor)
            {
                user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                if (user != null && !string.IsNullOrWhiteSpace(request.TwoFactorCode))
                {
                    result = await _signInManager.TwoFactorSignInAsync("Email", request.TwoFactorCode, false, rememberClient: false);
                }
            }
            if (!result.Succeeded)
            {
                user ??= await _userManager.FindByEmailAsync(request.Email);
                if (!user.TwoFactorEnabled)
                    throw new InvalidOperationException($"Invalid Credentials for '{request.Email}'.");
                else
                {
                    var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
                    if (!(providers.Contains("Email") /*&& await _featureManager.IsEnabledAsync(nameof(FeatureManagement.EnableEmailService))*/))
                        throw new InvalidOperationException("The email provider is not available for two-factor authentication.");
                    else
                    {
                        var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                        await _emailService.SendEmailAsync(new MailRequest() { ToEmail = user.Email, Body = $"Access token: {token}", Subject = "[FarmaciaERP] Authentication Token" });

                        return new Response<AuthenticationResponse>(StatusCode.PermissionDenied, message: "Requires Two-Factor. We send a token to your email account.");
                    }
                }
            }
            if (!user!.EmailConfirmed)
            {
                throw new InvalidOperationException($"Account Not Confirmed for '{request.Email}'.");
            }
            JwtSecurityToken jwtSecurityToken = await GenerarJWToken(user);
            AuthenticationResponse response = new AuthenticationResponse();
            response.Id = user.Id;
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = user.Email;
            response.UserName = user.UserName;
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;
            var refreshToken = generarRefreshToken(ipAddress);
            response.RefreshToken = refreshToken.Token;
            return new Response<AuthenticationResponse>(value: response, StatusCode.OK, $"Authenticated {user.UserName}");
        }

        public async Task<Response> RegistrarUsuarioAsync(RegisterRequest request, string origin)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null)
            {
                throw new InvalidOperationException($"Username '{request.UserName}' is already token.");
            }
            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Roles.Basic.ToString());
                    var verificationUri = await CorreoElectronicoEnviarVerificacionAsync(user, origin);

                    //if (await _featureManager.IsEnabledAsync(nameof(FeatureManagement.EnableEmailService)))
                    {
                        await _emailService.SendEmailAsync(new MailRequest() { ToEmail = user.Email, Body = $"Please confirm your account by visiting this URL {verificationUri}", Subject = "Confirm Registration" });
                    }
                    return new Response<string>(value: user.Id, StatusCode.OK, message: $"User Registered. Please confirm your account by visiting this URL {verificationUri}");
                }
                else
                {
                    throw new InvalidOperationException($"{result.Errors.ToList()[0].Description}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Email {request.Email} is already registered.");
            }
        }

        private async Task<JwtSecurityToken> GenerarJWToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }

            string ipAddress = IpHelper.GetIpAddress();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
                new Claim("ip", ipAddress)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key!));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }

        private string RandomTokenString()
        {
            var randomBytes = GenerateRandomBytes(40);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
        private byte[] GenerateRandomBytes(int size)
        {
            using (var generator = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[size];
                generator.GetBytes(salt);
                return salt;
            }
        }

        private async Task<string> CorreoElectronicoEnviarVerificacionAsync(ApplicationUser user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/account/confirm-email/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            //Email Service Call Here
            return verificationUri;
        }

        public async Task<Response<string>> ConfirmarCorreoElectronicoAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return new Response<string>(user.Id, StatusCode.OK, message: $"Account Confirmed for {user.Email}. You can now use the /api/Account/authenticate endpoint.");
            }
            else
            {
                throw new InvalidOperationException($"An error occured while confirming {user.Email}.");
            }
        }

        private RefreshToken generarRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expira = DateTime.Now.AddHours(1),
                CreadoEn = DateTime.Now,
                CreadoPorIp = ipAddress
            };
        }

        public async Task RecuperarContraseniaAsync(ForgotPasswordRequest model, string origin)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);

            // always return ok response to prevent email enumeration
            if (account == null) return;

            var code = await _userManager.GeneratePasswordResetTokenAsync(account);
            var route = "api/account/reset-password/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            var emailRequest = new MailRequest()
            {
                Body = $"You reset token is - {code}",
                ToEmail = model.Email,
                Subject = "Reset Password",
            };
            await _emailService.SendEmailAsync(emailRequest);
        }

        public async Task<Response<string>> CambiarContraseniaAsync(ResetPasswordRequest model)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);
            if (account == null)
                throw new InvalidOperationException($"No Accounts Registered with {model.Email}.");
            else
            {
                var result = await _userManager.ResetPasswordAsync(account, model.Token, model.Password);
                if (result.Succeeded)
                    return new Response<string>(model.Email!, StatusCode.OK, message: $"Password Resetted.");
                else
                    throw new InvalidOperationException($"Error occured while reseting the password.");
            }
        }

        public async Task<Response> HabilitarTwoFactorAuth()
        {
            var account = await _userManager.FindByEmailAsync(_currentUserService.Email);
            if (account == null)
                throw new InvalidOperationException($"No Accounts Registered with {_currentUserService.Email}.");
            else
            {
                await _userManager.SetTwoFactorEnabledAsync(account, true);
                await _signInManager.SignInAsync(account, isPersistent: false);
                return new Response(StatusCode.OK, "User enabled two-factor authentication.");
            }
        }

        public async Task<Response> DeshabilitarTwoFactorAuth()
        {
            var account = await _userManager.FindByEmailAsync(_currentUserService.Email);
            if (account == null)
                throw new InvalidOperationException($"No Accounts Registered with {_currentUserService.Email}.");
            else
            {
                await _userManager.SetTwoFactorEnabledAsync(account, false);
                await _signInManager.SignInAsync(account, isPersistent: false);
                return new Response(StatusCode.OK, "User disabled two-factor authentication.");
            }
        }

        public async Task<Response<string>> RegistrarTelefono(string phoneNumber)
        {
            var account = await _userManager.FindByEmailAsync(_currentUserService.Email);
            if (account == null)
                throw new InvalidOperationException($"No Accounts Registered with {_currentUserService.Email}.");
            else
            {
                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(account, phoneNumber);
                if (string.IsNullOrWhiteSpace(phoneNumber))
                    return new Response<string>(StatusCode.InvalidArgument, "The phone number is invalid");
                else
                {
                    // Send an SMS to verify the phone number ...
                    return new Response<string>(StatusCode.OK, "A verification code has been sent via SMS.");
                }
            }
        }

        public async Task<Response> VerificarTelefono(VerifyPhoneNumberRequest request)
        {
            var account = await _userManager.FindByEmailAsync(_currentUserService.Email);
            if (account == null)
                throw new InvalidOperationException($"No Accounts Registered with {_currentUserService.Email}.");
            else
            {
                var result = await _userManager.ChangePhoneNumberAsync(account, request.PhoneNumber, request.Code);
                if (!result.Succeeded)
                    return new Response(StatusCode.InvalidArgument, "Failed to verify phone number.");
                else
                {
                    await _signInManager.SignInAsync(account, isPersistent: false);
                    return new Response(StatusCode.OK, "Phone number added successfully.");
                }
            }
        }

        public async Task<AuthenticationProperties> AutenticarUsuarioProveedorExterno(string provider, string redirectUrl)
        {
            return await Task.Run(() => _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl));
        }
        public async Task<Response<string>> RegistrarUsuarioProveedorExterno(string origin)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return new Response<string>(StatusCode.InvalidArgument, "Error register external user");
            else
            {
                RegisterRequest request = new RegisterRequest();
                request.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                request.UserName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                request.FirstName = info.Principal.FindFirstValue(ClaimTypes.Name);
                request.LastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

                await RegistrarUsuarioAsync(request, origin);
                return new Response<string>(StatusCode.OK, "Successful process!");
            }
        }
        public async Task<Response<string>> AutenticarUsuarioProveedorExternoCallback()
        {
            var account = await _userManager.FindByEmailAsync(_currentUserService.Email);
            if (account == null)
                return new Response<string>(StatusCode.NotFound, $"No Accounts Registered with {_currentUserService.Email}.");
            else
            {
                Response<string> response = new Response<string>();
                var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(account));
                if (info == null)
                {
                    response.Code = StatusCode.InvalidArgument;
                    response.Message = "Error login info";
                }
                else
                {
                    var result = await _userManager.AddLoginAsync(account, info);
                    if (!result.Succeeded)
                    {
                        response.Code = StatusCode.InvalidArgument;
                        response.Message = $"Invalid Credentials fo '{_currentUserService.Email}'.";
                    }
                    else
                    {
                        response.Code = StatusCode.OK;
                        response.Message = "Successful process!";
                    }
                }
                return response;
            }
        }
    }
}

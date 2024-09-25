using Microsoft.AspNetCore.Authentication;

using VegaExpress.Worker.Core.Common;
using VegaExpress.Worker.Core.Models.Auth;

namespace VegaExpress.Worker.Core.Services.Contracts
{
    public interface IAccountService
    {
        Task<Response<AuthenticationResponse>> AutenticarUsuarioAsync(AuthenticationRequest request, string ipAddress);
        Task<Response> RegistrarUsuarioAsync(RegisterRequest request, string origin);
        Task<Response<string>> ConfirmarCorreoElectronicoAsync(string userId, string code);
        Task RecuperarContraseniaAsync(ForgotPasswordRequest model, string origin);
        Task<Response<string>> CambiarContraseniaAsync(ResetPasswordRequest model);
        Task<Response> HabilitarTwoFactorAuth();
        Task<Response> DeshabilitarTwoFactorAuth();
        Task<Response<string>> RegistrarTelefono(string phoneNumber);
        Task<Response> VerificarTelefono(VerifyPhoneNumberRequest request);
        Task<AuthenticationProperties> AutenticarUsuarioProveedorExterno(string provider, string redirectUrl);
        Task<Response<string>> AutenticarUsuarioProveedorExternoCallback();
        Task<Response<string>> RegistrarUsuarioProveedorExterno(string origin);
    }
}

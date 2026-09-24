using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KikasEBonitas.Api.Dtos;
using Microsoft.IdentityModel.Tokens;

namespace KikasEBonitas.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        var group = app.MapGroup("/api/auth").WithTags("Autenticação");

        group.MapPost("/login", (LoginDto dto) =>
        {
            // Validação simples de utilizador/palavra-passe (Administrador de teste)
            if (dto.Usuario != "admin" || dto.Senha != "Admin123!")
            {
                return Results.Unauthorized();
            }

            // Gerar Token JWT
            var jwtSettings = config.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, dto.Usuario),
                    new Claim(ClaimTypes.Role, "Admin")    
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiracaoEmMinutos"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Results.Ok(new RespostaTokenDto
            {
                Token = tokenString,
                Expiracao = tokenDescriptor.Expires.Value
            });
        });
    }
}
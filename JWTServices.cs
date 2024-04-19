using Cybage_Connect.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JWTServices
{
    private readonly IConfiguration _config;

    public JWTServices(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(UserRegistration user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Secret"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, user.UserName)
            }),
            Expires = DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
            Audience = _config["Jwt:Audience"],
            Issuer = _config["Jwt:Issuer"], // Set issuer from configuration
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }



}

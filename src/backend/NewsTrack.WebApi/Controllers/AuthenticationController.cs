﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NewsTrack.Identity.Repositories;
using NewsTrack.Identity.Results;
using NewsTrack.Identity.Services;
using NewsTrack.WebApi.Configuration;
using NewsTrack.WebApi.Dtos;

namespace NewsTrack.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IIdentityService _identityService;
        private readonly IIdentityRepository _identityRepository;

        public AuthenticationController(
            IConfigurationProvider configurationProvider, 
            IIdentityService identityService, 
            IIdentityRepository identityRepository)
        {
            _configurationProvider = configurationProvider;
            _identityService = identityService;
            _identityRepository = identityRepository;
        }

        [Route("generate")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AuthenticationDto dto)
        {
            if (ModelState.IsValid)
            {
                var result = await _identityService.Authenticate(dto.Username, dto.Password);
                if (result == AuthenticateResult.Ok)
                {
                    var identity = await _identityRepository.GetByEmail(dto.Username);
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, identity.Id.ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Email, identity.Email),                        
                        new Claim(ClaimTypes.Name, identity.Username), 
                        new Claim(ClaimTypes.Actor, identity.IdType.ToString())                      
                    };

                    var creds = new SigningCredentials(
                        _configurationProvider.TokenConfiguration.SigningKey,
                        SecurityAlgorithms.HmacSha256
                    );

                    var token = new JwtSecurityToken(
                        _configurationProvider.TokenConfiguration.Issuer,
                        _configurationProvider.TokenConfiguration.Audience,
                        claims,
                        expires: DateTime.Now.AddMinutes(30),
                        signingCredentials: creds
                    );

                    return Ok(new TokenResponseDto
                    {
                        IsSuccessful = true,
                        Username = dto.Username,
                        Token = new JwtSecurityTokenHandler().WriteToken(token)
                    });
                }

                return Ok(new TokenResponseDto
                {
                    Username = dto.Username,
                    Failure = result == AuthenticateResult.Failed 
                        ? TokenResponseDto.FailureReason.Authentication
                        : TokenResponseDto.FailureReason.Lockout
                });
            }

            return BadRequest();
        }
    }
}
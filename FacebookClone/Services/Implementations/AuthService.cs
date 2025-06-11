using FacebookClone.Models.Constants;
using FacebookClone.Models.DomainModels;
using FacebookClone.Models.DTOs;
using FacebookClone.Repositories.Interfaces;
using FacebookClone.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FacebookClone.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UserResponseDto?> GetCurrentUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user == null ? null : UserService.MapToUserResponseDto(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Invalid credentials");
                return null;
            }

            if (!VerifyPassword(loginDto.Password, user.PasswordHash)) {
                _logger.LogWarning("Invalid credentials");
                return null;
            }


            var userDto = UserService.MapToUserResponseDto(user);
            var token = GenerateJwtToken(userDto);
            var expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes());
            _logger.LogInformation("User registered successfully: {UserId}", user.Id);

            return new AuthResponseDto
            {
                User = userDto,
                Token = token,
                Expires = expires,
            };


        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            var emailExist = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (emailExist != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", registerDto.Email);
                return null;
            }

            var userNameExist = await _userRepository.GetByUsernameAsync(registerDto.Username);
            if (userNameExist != null)
            {
                _logger.LogWarning("Registration attempt with existing username: {Username}", registerDto.Username);
                return null;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                Role = RoleTypes.User,
                CreatedAt = DateTime.UtcNow,

            };

            var createdUser = await _userRepository.CreateUserAsync(user);
            if (createdUser == null)
            {
                _logger.LogError("Failed to create user during registration: {Email}", registerDto.Email);
                return null;
            }

            var userDto = UserService.MapToUserResponseDto(createdUser);
            var token = GenerateJwtToken(userDto);
            var expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes());
            _logger.LogInformation("User registered successfully: {UserId}", createdUser.Id);

            return new AuthResponseDto
            {
                User = userDto,
                Token = token,
                Expires = expires,
            };
        }

        public string GenerateJwtToken(UserResponseDto userDto)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSecretKey()));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                new Claim(ClaimTypes.Name, userDto.Username),
                new Claim(ClaimTypes.Email, userDto.Email),
                new Claim(ClaimTypes.Role, userDto.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Private helper methods
        private string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            var hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            var salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }

            return true;
        }

        private string GetSecretKey()
        {
            return _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");
        }

        private int GetTokenExpirationMinutes()
        {
            return int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60");
        }
    }
}

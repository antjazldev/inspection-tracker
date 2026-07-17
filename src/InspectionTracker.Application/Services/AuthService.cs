using InspectionTracker.Application.Dtos;
using InspectionTracker.Application.Exceptions;
using InspectionTracker.Application.Interfaces;
using InspectionTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionTracker.Application.Services
{
    public class AuthService(IUserRepository userRepository)
    {
        private const int MinPasswordLength = 8;

        public async Task<User> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
        {
            ValidateRegistration(dto);

            var existing = await userRepository.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant(), ct);
            if (existing is not null)
                throw new BusinessValidationException("That email is already registered.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                DisplayName = dto.DisplayName.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            await userRepository.AddAsync(user, ct);
            return user;
        }

        public async Task<User> LoginAsync(LoginDto dto, CancellationToken ct = default)
        {
            var user = await userRepository.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant(), ct);

            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid credentials.");

            return user;
        }

        private static void ValidateRegistration(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
                throw new BusinessValidationException("A valid email is required.");

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < MinPasswordLength)
                throw new BusinessValidationException($"The password must be at least {MinPasswordLength} characters.");
        }
    }
}

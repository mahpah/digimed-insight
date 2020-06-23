using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WebApp.Models;

namespace WebApp.Utils
{
    internal class PasswordEntropyValidator : IPasswordValidator<ApplicationUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
        {
            var r = 0;
            if (password.Any(char.IsDigit))
            {
                r += 10;
            }

            if (password.Any(char.IsLower))
            {
                r += 26;
            }

            if (password.Any(char.IsUpper))
            {
                r += 26;
            }

            if (password.Any(char.IsSymbol))
            {
                r += 32;
            }

            var entropy = Math.Log2(Math.Pow(r, password.Length));

            if (entropy < 39)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError()
                {
                    Code = "weak_password",
                    Description = "Password is too weak"
                }));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}

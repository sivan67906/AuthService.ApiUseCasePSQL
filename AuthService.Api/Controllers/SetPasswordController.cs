using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

public class SetPasswordController : Controller
{
    [HttpPost("api/setpassword")]
    public async Task<IActionResult> SetPasswords(
    [FromServices] UserManager<ApplicationUser> userManager,
    [FromBody] List<string> emails)
    {
        if (emails == null || !emails.Any())
            return BadRequest("No emails provided.");

        foreach (var email in emails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var removeResult = await userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                    return BadRequest($"Failed to remove password for {email}");

                var addResult = await userManager.AddPasswordAsync(user, "Welcome@123");
                if (!addResult.Succeeded)
                    return BadRequest($"Failed to add password for {email}");
            }
        }

        return Ok("Passwords updated successfully");
    }
}

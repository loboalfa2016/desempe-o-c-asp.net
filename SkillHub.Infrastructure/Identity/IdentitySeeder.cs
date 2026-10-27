
using Microsoft.AspNetCore.Identity;

namespace SkillHub.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Employee", "Participant"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"No se pudo crear el rol '{role}': {errors}");
                }
            }
        }
    }

    public static async Task SeedEmployeeAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName)
    {
        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(
                    existingUser, "Employee"))
            {
                throw new InvalidOperationException(
                    "El correo indicado ya pertenece a una cuenta que no es Employee.");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(
                existingUser);

            var resetResult = await userManager.ResetPasswordAsync(
                existingUser,
                token,
                password);

            if (!resetResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    resetResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"No se pudo actualizar la contraseña del empleado: {errors}");
            }

            existingUser.FullName = fullName;

            var updateResult = await userManager.UpdateAsync(existingUser);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    updateResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"No se pudo actualizar el empleado: {errors}");
            }

            return;
        }

        var employee = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(
            employee, password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"No se pudo crear el empleado: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(
            employee, "Employee");

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(employee);

            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"No se pudo asignar el rol Employee: {errors}");
        }
    }
}

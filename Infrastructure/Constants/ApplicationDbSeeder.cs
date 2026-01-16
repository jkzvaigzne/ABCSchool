using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Constants
{
    public class ApplicationDbSeeder(
        IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantContextAccessor,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext applicationDbContext)
    {
        private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantInfoContextAccessor = tenantContextAccessor;
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

        public async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
        {
            if (_applicationDbContext.Database.GetMigrations().Any())
            {
                if ((await _applicationDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                {
                    await _applicationDbContext.Database.MigrateAsync(cancellationToken);
                }

                if (await _applicationDbContext.Database.CanConnectAsync(cancellationToken))
                {
                    // Seeding data
                    // Default Roles > Assign permissions and claims
                    // Users > Assign roles
                }
            }
        }

        private async Task InitializeDefaultRolesAsync(CancellationToken ct)
        {
            foreach (var roleName in RoleContants.DefaultRoles)
            {
                if (await _roleManager.Roles.SingleOrDefaultAsync(role => role.Name == roleName, ct) is not ApplicationRole incomingRole)
                {
                    incomingRole = new ApplicationRole()
                    {
                        Name = roleName,
                        Description = $"{roleName} Role"
                    };

                    await _roleManager.CreateAsync(incomingRole);
                }

                // Assign Permissions

                if (roleName == RoleContants.Basic)
                {
                    await AssignPermissionsToRole(SchoolPermissions.Basic, incomingRole, ct);
                } 
                else if (roleName == RoleContants.Admin)
                {
                    await AssignPermissionsToRole(SchoolPermissions.Admin, incomingRole, ct);

                    if (_tenantInfoContextAccessor.MultiTenantContext?.TenantInfo.Id == TenancyConstants.Root)
                    {
                        await AssignPermissionsToRole(SchoolPermissions.Root, incomingRole, ct);
                    }
                }
            }
        }

        private async Task AssignPermissionsToRole(
            IReadOnlyList<SchoolPermission> rolePermissions,
            ApplicationRole role,
            CancellationToken ct)
        {
            var currentClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var rolePermission in rolePermissions)
            {
                if (!currentClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == rolePermission.Name))
                {
                    await _applicationDbContext.RoleClaims.AddAsync(new ApplicationRoleClaim
                    {
                        RoleId = role.Id,
                        ClaimType = ClaimConstants.Permission,
                        ClaimValue = rolePermission.Name,
                        Description = rolePermission.Description,
                        Group = rolePermission.Group
                    }, ct);

                    await _applicationDbContext.SaveChangesAsync(ct);
                }
            }
        }
    }
}

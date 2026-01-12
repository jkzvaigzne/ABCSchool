using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts
{
    public abstract class BaseDbContext :
        MultiTenantIdentityDbContext<
            ApplicationUser,
            ApplicationRole,
            string,
            IdentityUserClaim<string>,
            IdentityUserRole<string>,
            IdentityUserLogin<string>,
            ApplicationRoleClaim,
            IdentityUserToken<string>>
    {
        protected BaseDbContext(IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantInfoContextAccessor, DbContextOptions options) 
            : base(tenantInfoContextAccessor, options)
        {
        }
    }
}

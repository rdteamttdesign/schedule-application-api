namespace SchedulingTool.Api.Domain.Security.Hashing;

public interface IPasswordHasher
{
  string HashPassword( string password );
  bool VerifyPassword( string providedPassword, string passwordHash );
}

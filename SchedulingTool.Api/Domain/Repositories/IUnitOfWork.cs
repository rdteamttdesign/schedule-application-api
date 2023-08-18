namespace SchedulingTool.Api.Domain.Repositories
{
  public interface IUnitOfWork
  {
    Task CompleteAsync();
  }
}

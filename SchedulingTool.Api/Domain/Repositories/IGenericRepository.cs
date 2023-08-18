namespace SchedulingTool.Api.Domain.Repositories
{
  public interface IGenericRepository<TEntity> where TEntity : class
  {
    Task<TEntity> Create( TEntity ent );
    void Delete( TEntity ent );
    Task<TEntity?> GetById( long id );
    Task Update( TEntity ent );
  }
}

using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Persistence.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
  protected readonly AppDbContext _context;
  public GenericRepository( AppDbContext context )
  {
    _context = context;
  }

  public async Task<TEntity> Create( TEntity ent )
  {
    return ( await _context.AddAsync( ent ) ).Entity;
  }

  public async Task Update( TEntity ent )
  {
    _context.Update( ent );
  }

  public void Delete( TEntity ent )
  {
    _context.Remove( ent );
  }

  public async Task<TEntity?> GetById( long id )
  {
    return await _context.FindAsync<TEntity>( id );
  }
}

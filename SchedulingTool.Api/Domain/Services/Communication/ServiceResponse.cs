namespace SchedulingTool.Api.Domain.Services.Communication;

public class ServiceResponse<T> : BaseResponse where T : class
{
  public T Content;
  public ServiceResponse( bool success, string message, T result ) : base( success, message )
  {
    Content = result;
  }
  public ServiceResponse( T result ) : this( true, string.Empty, result )
  { }

  public ServiceResponse( string message ) : this( false, message, default )
  { }
}

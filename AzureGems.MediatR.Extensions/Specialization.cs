using MediatR;
using System.Threading.Tasks;

namespace AzureGems.MediatR.Extensions
{
	public interface ICommand<out TResult> : IRequest<TResult> { }

	public interface ICommandHandler<in TCommand, TResult> :
		IRequestHandler<TCommand, TResult> where TCommand : IRequest<TResult>
	{ }


	public interface IQuery<out TResult> : IRequest<TResult> { }

	public interface IQueryHandler<in TQuery, TResult> :
		IRequestHandler<TQuery, TResult> where TQuery : IRequest<TResult>
	{ }

	public static class MediatRExt
	{
		public static Task<TResult> SendQuery<TQuery, TResult>(this IMediator mediator, TQuery query) where TQuery: IQuery<TResult>
		{
			return mediator.Send(query);
		}

		public static Task<TResult> SendCommand<TCommand, TResult>(this IMediator mediator, TCommand cmd) where TCommand : ICommand<TResult>
		{
			return mediator.Send(cmd);
		}
	}
}

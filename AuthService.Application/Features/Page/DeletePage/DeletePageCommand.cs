namespace AuthService.Application.Features.Page.DeletePage;

public sealed record DeletePageCommand(Guid Id) : IRequest<bool>;

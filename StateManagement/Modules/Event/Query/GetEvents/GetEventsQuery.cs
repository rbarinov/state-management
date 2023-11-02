using MediatR;
using StateManagement.Modules.Shared.Models;

namespace StateManagement.Modules.Event.Query.GetEvents;

public sealed record GetEventsQuery(int Page, int PageSize, int? FromGlobalVersion) : IRequest<PagedListOut<EventModelOut>>;

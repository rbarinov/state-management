using Events.Modules.Shared.Models;
using MediatR;

namespace Events.Modules.Event.Query.GetEvents;

public sealed record GetEventsQuery(int Page, int PageSize, int? FromGlobalVersion) : IRequest<PagedListOut<EventModelOut>>;

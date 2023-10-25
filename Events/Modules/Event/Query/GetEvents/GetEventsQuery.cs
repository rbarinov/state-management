using Events.Models;
using MediatR;

namespace Events.Modules.Event.Query.GetEvents;

public sealed record GetEventsQuery(int Page, int PageSize, int? FromGlobalVersion) : IRequest<List<EventModelOut>>;

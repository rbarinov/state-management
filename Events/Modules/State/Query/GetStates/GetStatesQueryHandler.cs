using Events.Data;
using Events.Modules.Shared.Models;
using Events.Modules.State.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Events.Modules.State.Query.GetStates;

public class GetStatesQueryHandler : IRequestHandler<GetStatesQuery, PagedListOut<StateInfoModelOut>>
{
    private readonly EventDbContext _db;

    public GetStatesQueryHandler(EventDbContext db)
    {
        _db = db;
    }

    public async Task<PagedListOut<StateInfoModelOut>> Handle(
        GetStatesQuery request,
        CancellationToken cancellationToken
    )
    {
        var states = await _db.States
            .AsNoTracking()
            .Select(
                e => new StateInfoModelOut
                {
                    Key = e.Key,
                    ReferenceVersion = e.ReferenceVersion,
                    UpdatedAt = e.UpdatedAt
                }
            )
            .OrderBy(e => e.Key)
            .ToPagedListAsync(request.Page, request.PageSize, cancellationToken);

        return states;
    }
}

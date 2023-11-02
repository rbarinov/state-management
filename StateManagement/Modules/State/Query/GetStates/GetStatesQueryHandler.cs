using MediatR;
using Microsoft.EntityFrameworkCore;
using StateManagement.Data;
using StateManagement.Modules.Shared.Models;
using StateManagement.Modules.State.Models;

namespace StateManagement.Modules.State.Query.GetStates;

public class GetStatesQueryHandler : IRequestHandler<GetStatesQuery, PagedListOut<StateInfoModelOut>>
{
    private readonly StateManagementDbContext _db;

    public GetStatesQueryHandler(StateManagementDbContext db)
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

using Carter;
using Events.Modules.Shared.Models;
using Events.Modules.State.Command.PostState;
using Events.Modules.State.Models;
using Events.Modules.State.Query.GetState;
using Events.Modules.State.Query.GetStates;
using MediatR;

namespace Events.Modules.State;

public class StateModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/states")
            .WithOpenApi()
            .WithTags("State");

        group.MapGet("/", GetStates)
            .Produces<PagedListOut<StateInfoModelOut>>(StatusCodes.Status200OK)
            .WithName("getStates");

        group.MapGet("/{key}", GetState)
            .Produces<StateFullModelOut>(StatusCodes.Status200OK)
            .WithName("getState");

        group.MapPost("/{key}", PostState)
            .Produces<StateFullModelOut>(StatusCodes.Status200OK)
            .WithName("postState");
    }

    private static async Task<IResult> PostState(ISender sender, string key, StateModelIn model)
    {
        var command = new PostStateCommand
        {
            Key = key,
            ReferenceVersion = model.ReferenceVersion,
            UpdatedAt = model.UpdatedAt,
            Payload64 = model.Payload64
        };

        var response = await sender.Send(command);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetState(ISender sender, string key)
    {
        var query = new GetStateQuery
        {
            Key = key
        };

        var response = await sender.Send(query);

        if (response is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> GetStates(ISender sender, int page = 1, int pageSize = 5)
    {
        var query = new GetStatesQuery { Page = page, PageSize = pageSize };

        var response = await sender.Send(query);

        return Results.Ok(response);
    }
}

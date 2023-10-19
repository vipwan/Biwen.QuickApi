using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkTestAot.Apis
{
    public class MySotre
    {
        public static Todo[] SampleTodos()
        {
            return new Todo[] {
                new(1, "Walk the dog"),
                new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
                new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
                new(4, "Clean the bathroom"),
                new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
            };
        }
    }
    //public class Rsp : BaseResponse
    //{
    //    public Todo[]? Todos { get; set; }
    //}
    public class Req : BaseRequest<Req>
    {
        public int Id { get; set; }
        public Req()
        {
            RuleFor(x => x.Id).InclusiveBetween(1, 5);
        }
    }

    [QuickApi("todos", Group = "my")]
    public class TodosApi : BaseQuickApi<EmptyRequest, IResultResponse>
    {
        public override Task<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(Results.Ok(MySotre.SampleTodos()).AsRspOfResult());
        }
    }

    [QuickApi("todos/{id}", Group = "my")]
    public class TodoApi : BaseQuickApi<Req, IResultResponse>
    {
        public override async Task<IResultResponse> ExecuteAsync(Req request)
        {
            await Task.CompletedTask;
            return Results.Ok(MySotre.SampleTodos().FirstOrDefault(x => x.Id == request.Id)).AsRspOfResult();
        }
    }
}
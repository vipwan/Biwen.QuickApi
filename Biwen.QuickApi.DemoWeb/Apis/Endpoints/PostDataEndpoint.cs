using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Biwen.QuickApi.DemoWeb.Apis.Endpoints
{
    [ProducesResponseType<PostData>(200)]
    [OpenApiMetadata("POST测试", "Post测试", Tags = ["Endpoints"])]
    [EndpointGroupName("test")]
    public class PostDataEndpoint : IQuickEndpoint
    {
        [FromBody]
        public class PostData : BaseRequest<PostData>
        {
            [Description("Hello的描述")]
            public string? Hello { get; set; }

            [Description("World的描述")]
            [Required, Length(5, 12)]
            public string? World { get; set; }
        }

        public static Verb Verbs => Verb.POST;

        public static Delegate Handler => (PostData request) =>
        {
            return TypedResults.Ok(request);
        };
    }
}
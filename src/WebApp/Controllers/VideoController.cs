using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApp.CommandResults;
using WebApp.Commands;

namespace WebApp.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VideoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("upload")]
        public async Task<UploadKey> GenerateUploadSas(GenerateUploadKeyCommand command)
        {
            return await _mediator.Send(command);
        }
    }
}

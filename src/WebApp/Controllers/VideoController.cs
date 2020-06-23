using System;
using System.Threading.Tasks;
using Insights.VideoPackager.Azure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.Commands;
using WebApp.Commands.CommandResults;
using WebApp.Queries;
using WebApp.Queries.QueryResults;

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

        [HttpPost("cleanUp")]
        public async Task<IActionResult>CleanUp(CleanAssetCommand command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process(ProcessVideoCommand command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpGet("jobs")]
        public async Task<QueryResult<JobInformation>> ListJobs(string nextToken)
        {
            return await _mediator.Send(new ListJobsQuery(nextToken));
        }

        [HttpGet("streamingInformation/drm")]
        public async Task<StreamingOutput> GetDrmStreamingOutput(string assetName)
        {
            return await _mediator.Send(new GetStreamOutputQuery(assetName, ProtectionType.Drm));
        }

        [HttpGet("streamingInformation/aes")]
        public async Task<StreamingOutput> GetAesStreamingOutput(string assetName)
        {
            return await _mediator.Send(new GetStreamOutputQuery(assetName, ProtectionType.Aes));
        }
    }
}

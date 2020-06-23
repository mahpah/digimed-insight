using MediatR;
using WebApp.Commands.CommandResults;

namespace WebApp.Commands
{
    public class GenerateUploadKeyCommand : IRequest<UploadKey>
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}

using MediatR;
using WebApp.CommandResults;

namespace WebApp.Commands
{
    public class GenerateUploadKeyCommand : IRequest<UploadKey>
    {
        public string FileName { get; set; }
    }
}

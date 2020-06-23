using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace WebApp.Utils
{
    public class RequestValidationPipeline<TReq, TRes> : IPipelineBehavior<TReq, TRes>
    {
        private readonly IEnumerable<IValidator<TReq>> _validators;

        public RequestValidationPipeline(IEnumerable<IValidator<TReq>> validators)
        {
            _validators = validators;
        }
        public Task<TRes> Handle(TReq request, CancellationToken cancellationToken, RequestHandlerDelegate<TRes> next)
        {
            var context = new ValidationContext(request);
            var errors = _validators.Select(t => t.Validate(context))
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .ToList();

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }

            return next();
        }
    }
}

using System;
using FluentValidation;

namespace WebApp.Commands.CommandValidators
{
    public class GenerateUploadKeyCommandValidator : AbstractValidator<GenerateUploadKeyCommand>
    {
        public GenerateUploadKeyCommandValidator()
        {
            RuleFor(x => x.ContentType)
                .NotEmpty()
                .Must(x => x.StartsWith("video/", StringComparison.OrdinalIgnoreCase));
        }
    }
}

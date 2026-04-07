using FluentValidation;
using MusicMigrater.BLL.DTO.VkMusic;

namespace MusicMigrater.Validation;

public class VkTokenDtoValidator : AbstractValidator<VkTokenDto>
{
    public VkTokenDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();
    }
}

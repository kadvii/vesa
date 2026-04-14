using eVisaPlatform.Application.DTOs.Auth;
using eVisaPlatform.Application.DTOs.User;
using eVisaPlatform.Application.DTOs.Visa;
using eVisaPlatform.Application.DTOs.Consultants;
using eVisaPlatform.Application.DTOs.Agents;
using FluentValidation;

namespace eVisaPlatform.Application.Validators;

// ── Auth ─────────────────────────────────────────────────────────────────────

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

// ── User management ───────────────────────────────────────────────────────────

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.Role).IsInEnum();
    }
}

// ── Visa ─────────────────────────────────────────────────────────────────────

public class CreateVisaApplicationValidator : AbstractValidator<CreateVisaApplicationDto>
{
    public CreateVisaApplicationValidator()
    {
        RuleFor(x => x.VisaType).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(4000).When(x => x.Notes != null);
        RuleFor(x => x.DestinationCountry).MaximumLength(200).When(x => x.DestinationCountry != null);
        RuleFor(x => x.ApplicantFullName).MaximumLength(200).When(x => x.ApplicantFullName != null);
        RuleFor(x => x.PassportNumber).MaximumLength(80).When(x => x.PassportNumber != null);
        RuleFor(x => x.Nationality).MaximumLength(120).When(x => x.Nationality != null);
    }
}

public class UpdateVisaApplicationValidator : AbstractValidator<UpdateVisaApplicationDto>
{
    public UpdateVisaApplicationValidator()
    {
        RuleFor(x => x.VisaType).IsInEnum().When(x => x.VisaType.HasValue);
    }
}

// ── Consultants ───────────────────────────────────────────────────────────────

public class BookConsultantValidator : AbstractValidator<BookConsultantDto>
{
    public BookConsultantValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty().WithMessage("ApplicationId is required.");
    }
}

// ── Agents ───────────────────────────────────────────────────────────────────

public class OrderAgentValidator : AbstractValidator<OrderAgentDto>
{
    public OrderAgentValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty().WithMessage("ApplicationId is required.");
    }
}

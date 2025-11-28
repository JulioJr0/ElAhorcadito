using ElAhorcadito.Models.Entities;
using ElAhorcadito.Models.DTOs.Auth;
using ElAhorcadito.Repositories;
using FluentValidation;
using System.Text.RegularExpressions;

namespace ElAhorcadito.Models.Validators
{
    public class RegistroDTOValidator : AbstractValidator<RegistroDTO>
    {
        public IRepository<Usuarios> Repository { get; }

        public RegistroDTOValidator(IRepository<Usuarios> repository)
        {
            Repository = repository;

            RuleFor(x => x.NombreUsuario)
                .NotEmpty().WithMessage("Debe escribir el nombre de usuario.")
                .MaximumLength(50).WithMessage("Escriba un nombre de máximo 50 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Debe escribir el correo del usuario.")
                .MaximumLength(100).WithMessage("Escriba un correo de máximo 100 caracteres.")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
                .Must(EmailNoRepetido).WithMessage("El correo ya está registrado");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Debe escribir la contraseña del usuario.")
                .MaximumLength(10).WithMessage("La contraseña debe tener máximo 10 caracteres.")
                .MinimumLength(8).WithMessage("La contraseña debe tener mínimo 8 caracteres.")
                .Must(PasswordValida).WithMessage("La contraseña debe incluir al menos una letra mayúscula, una letra minúscula, un número y un carácter especial.");
        }

        private bool PasswordValida(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;
            return Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^\w\s]).{8,10}$");
        }

        private bool EmailNoRepetido(string email)
        {
            if (string.IsNullOrEmpty(email) || Repository == null)
                return false;
            return !Repository.GetAll().Any(x => x.Email.ToLower() == email.ToLower());
        }
    }
}

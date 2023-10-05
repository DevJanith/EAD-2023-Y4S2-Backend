using FluentValidation;
using Rest.Entities;
using System.Text.RegularExpressions;

namespace Rest.util
{
    public class UserDetailsValidator : AbstractValidator<UserDetails>
    {
        public UserDetailsValidator()
        {
            RuleFor(user => user.NIC)
                 .NotEmpty().WithMessage("NIC is required.")
                 .Must(BeValidNIC).WithMessage("Invalid NIC format.");

            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Must(BeValidEmail).WithMessage("Invalid email address format.");

            RuleFor(user => user.ContactNumber)
                .NotEmpty().WithMessage("Contact number is required.")
                .Must(BeValidSriLankanPhoneNumber).WithMessage("Invalid contact number format.");

            /* RuleFor(user => user.FirstName)
                 .NotEmpty().WithMessage("First name is required.")
                 .MaximumLength(50).WithMessage("First name should not exceed 50 characters.");

             RuleFor(user => user.LastName)
                 .NotEmpty().WithMessage("Last name is required.")
                 .MaximumLength(50).WithMessage("Last name should not exceed 50 characters.");
            */

            // Add additional validation rules as needed.
        }

        private bool BeValidNIC(string nic)
        {
            return IsValidOldNIC(nic) || IsValidNewNIC(nic);
        }

        private bool BeValidEmail(string email)
        {
            return IsValidEmail(email);
        }

        private bool BeValidSriLankanPhoneNumber(string contactNumber)
        {
            return IsValidSriLankanPhoneNumber(contactNumber);
        }

        private bool IsValidOldNIC(string nic)
        {
            // Old NIC format: 123456789V
            string oldNicPattern = @"^\d{9}[Vv]$";
            return Regex.IsMatch(nic, oldNicPattern);
        }

        private bool IsValidNewNIC(string nic)
        {
            // New NIC format: 200001234567
            string newNicPattern = @"^\d{12}$";
            return Regex.IsMatch(nic, newNicPattern);
        }

        private bool IsValidEmail(string email)
        {
            // Simple email pattern validation (you can use a more comprehensive regex pattern if needed).
            string emailPattern = @"^\S+@\S+\.\S+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private bool IsValidSriLankanPhoneNumber(string phoneNumber)
        {
            // Sri Lankan phone number formats: +94771234567, 0771234567, 0112345678, 071-123-4567, 0701 234 567
            string phonePattern = @"^(\+94|0|011)?[1-9]\d{8}$";
            return Regex.IsMatch(phoneNumber, phonePattern);
        }
    }
}


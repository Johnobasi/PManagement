
namespace PermissionManagement.Model
{
    public class ChangePasswordDto
    {
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Old password must be supplied")]
        public string OldPassword { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "New password must be supplied")]
        public string NewPassword { get; set; }

       //[Required(AllowEmptyStrings = false, ErrorMessage = "Confirm password must be supplied")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordDto
    {
        //[Required(AllowEmptyStrings = false, ErrorMessage = "User name must be supplied")]
        public string Username { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Email must be supplied")]
        public string Email { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recipes.Data.DataTransferObjects.UserDTOs
{

    public class UserInfo
    {
        [Required(ErrorMessage = "Id is required.")]
        public Guid Id { get; set; }

        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string? FirstName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string? LastName { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid URL for Avatar.")]
        public string? Avatar { get; set; } = string.Empty;
    }
}

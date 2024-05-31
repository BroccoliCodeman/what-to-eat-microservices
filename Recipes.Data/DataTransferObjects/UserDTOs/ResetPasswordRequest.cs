using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recipes.Data.DataTransferObjects.UserDTOs
{
    public class ResetPasswordRequest
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string NewPasword { get; set; }

    }
}

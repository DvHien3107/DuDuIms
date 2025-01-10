using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Authentication
{
    public class AuthenticationResponse
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int ExpiresIn { get; set; }

        public int RefreshTokenExpiresIn { get; set; }
        
        public DateTime? Issued { get; set; }
    }
}

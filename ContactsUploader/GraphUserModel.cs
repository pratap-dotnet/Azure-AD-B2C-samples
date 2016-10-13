using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsUploader
{
    public class GraphUserModel
    {
        public bool accountEnabled { get; set; }
        public SignInName[] signInNames { get; set; }
        public string creationType { get; set; }
        public string displayName { get; set; }
        public string mailNickname { get; set; }
        public Passwordprofile passwordProfile { get; set; }
        public string passwordPolicies { get; set; }
    }

    public class Passwordprofile
    {
        public string password { get; set; }
        public bool forceChangePasswordNextLogin { get; set; }

        public Passwordprofile(string password, bool forceChangePasswordNextLogin)
        {
            this.password = password;
            this.forceChangePasswordNextLogin = forceChangePasswordNextLogin;
        }
    }

    public class SignInName
    {
        public string type { get; private set; }
        public string value { get; private set; }

        public SignInName(string type, string value)
        {
            this.type = type;
            this.value = value;
        }
    }
}

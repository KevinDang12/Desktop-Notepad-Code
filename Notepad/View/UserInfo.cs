using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notepad.View
{
    /// <summary>
    /// The User Info for getting the user's notes
    /// </summary>
    public class UserInfo
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    /// <summary>
    /// The Facebook User Info after using
    /// Facebook authentication
    /// </summary>
    public class FacebookUserInfo
    {
        public string Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
    }
}

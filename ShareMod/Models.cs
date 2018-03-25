using System;
using System.Collections;
using System.Collections.Generic;

namespace ShareMod
{
    internal class Model
    {
        public string Error { get; set; }

        public string Status { get; set; }
    }
    
    internal class LoginModel : Model
    {
        public int UserID { get; set; }
        
        public string Token { get; set; }
    }
    
    internal class WorldsResponseModel : Model
    {
        public int Page { get; set; }
        
        public int PageSize { get; set; }

        public bool LastPage { get; set; }

        public WorldModel[] Items { get; set; }
    }

    internal class WorldModel : Model
    {
        public int ID { get; set; }

        public int AuthorID { get; set; }

        public string Title { get; set; }

        public long CreatedAt { get; set; }

        public string Hash { get; set; }
    }

    internal class UserModel : Model
    {
        public int ID { get; set; }

        public string Username { get; set; }

        public bool Trusted { get; set; }
    }
}

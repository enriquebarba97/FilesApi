using System;

namespace FilesApi.Models
{
    public class TokenResponse
    {

        public TokenResponse(string _Id, string _FirstName, string _LastName, string _username, string _Token)
        {
            Id = _Id;
            FirstName = _FirstName;
            LastName = _LastName;
            Username = _username;
            Token = _Token;
        }
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
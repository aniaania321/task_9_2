using Models.Models;

namespace API.Services.Tokens;
public interface ITokenService
{ 
    string CreateToken(Account account, string role);
}


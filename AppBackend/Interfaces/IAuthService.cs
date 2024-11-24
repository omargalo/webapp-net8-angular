using System.Threading.Tasks;

namespace AppBackend.Interfaces
{
    public interface IAuthService
    {
        Task<string> Authenticate(string username, string password);
        Task<bool> Register(
            string username,
            string password,
            string role,
            string name,
            string lastName,
            string mothersMaidenName,
            string email,
            string cellPhone);
    }
}

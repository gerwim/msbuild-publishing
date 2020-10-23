namespace GerwimFeiken.Publishing.Tasks.Repositories
{
    public interface IRegistry
    {
        string Read(string keyName, string valueName);
    }
}
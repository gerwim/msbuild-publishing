namespace GerwimFeiken.Publishing.Tasks.Repositories
{
    public class Registry : IRegistry
    {
        public string Read(string keyName, string valueName)
        {
            return Microsoft.Win32.Registry.GetValue(keyName, valueName, "").ToString();
        }
    }
}

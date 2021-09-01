namespace GerwimFeiken.Publishing.Tasks.Repositories
{
    public class Version : IVersion
    {
        public System.Version GetOsVersion()
        {
            return System.Environment.OSVersion.Version;
        }
    }
}

using System.Configuration;

namespace YSGM.Handlers
{
    public class GMCommand : BaseCommand
    {
        public string Execute(string[] args)
        {
            
            string uid = ConfigurationManager.AppSettings.Get("UID");
            string cmd = string.Join(" ", args);
            
            return MUIPManager.Instance.GM(uid, cmd);
        }
    }
}

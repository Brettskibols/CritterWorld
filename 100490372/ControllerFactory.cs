using CritterController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _100490372
{
    class ControllerFactory : ICritterControllerFactory
    {
        public string Author => "Brett Jones 100490372";

        public ICritterController[] GetCritterControllers()
        {
            List<ICritterController> controllers = new List<ICritterController>();
            {
                controllers.Add(new Porthos("Porthos"));
                controllers.Add(new Porthos("Porthos"));
                controllers.Add(new Porthos("Porthos"));
                controllers.Add(new Aramis("Aramis"));
                controllers.Add(new Dartagnan("Dartagnan"));
            }
            return controllers.ToArray();
        }
    }
}

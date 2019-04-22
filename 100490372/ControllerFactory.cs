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
        public string Author => "100490372";

        public ICritterController[] GetCritterControllers()
        {
            List<ICritterController> controllers = new List<ICritterController>();
            for (int i = 0; i < 25; i++)
            {
                controllers.Add(new Porthos("Porthos" + (i + 1)));
                controllers.Add(new Aramis("Aramis" + (i + 1)));
                controllers.Add(new Dartagnan("Dartagnan" + (i + 1)));
            }
            return controllers.ToArray();
        }
    }
}

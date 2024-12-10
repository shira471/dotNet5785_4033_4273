using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalApi
{
    public interface Idal
    {
        Ivolunteer volunteer { get; }
        Icall call { get; }
        Iassignment assignment { get; }
        Iconfig config { get; }
        object Config { get; }

        void ResetDB();//the method will reset the data base
    }
}

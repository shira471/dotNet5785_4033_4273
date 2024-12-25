using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO;

namespace PL;



internal class VolunteerSortByCollection : IEnumerable
{
    static readonly IEnumerable<BO.VolunteerSortBy> v_enums =
     (Enum.GetValues(typeof(BO.VolunteerSortBy)) as IEnumerable<BO.VolunteerSortBy>)!;

    public IEnumerator GetEnumerator() => v_enums.GetEnumerator();
}
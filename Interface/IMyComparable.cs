using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Interface
{
    public interface IMyComparable<in T>
    {
        int CompareTo(T other);
    }
}

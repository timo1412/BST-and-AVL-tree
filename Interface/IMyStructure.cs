using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Interface
{
    public interface IMyStructure<T> where T : IMyComparable<T>
    {
        bool Add(T value);

        bool Find(T value, out T found);

        bool Delete(T value);
    }
}

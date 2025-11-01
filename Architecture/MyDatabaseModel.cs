using SemestralnaPracaAUS2.Structures;
using SemestralnaPracaAUS2.TestData;
using SemestralnaPracaAUS2.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemestralnaPracaAUS2.Model
{
    public sealed class MyDatabaseModel
    {
        private readonly AVLTree<PersonByUniqueNumber> _idxPersonById = new AVLTree<PersonByUniqueNumber>();
        private readonly AVLTree<PersonByBirth> _idxPersonByBirth = new AVLTree<PersonByBirth>();
        private readonly AVLTree<PcrByCode> _idxByCode = new AVLTree<PcrByCode>();
        private readonly AVLTree<PcrByPersonDate> _idxByPerson = new AVLTree<PcrByPersonDate>();
        private readonly AVLTree<PcrByDate> _idxByDate = new AVLTree<PcrByDate>();
        private readonly AVLTree<PcrByRegionDate> _idxByRegionDate = new AVLTree<PcrByRegionDate>();
        private readonly AVLTree<PcrByDistrictDate> _idxByDistrictDate = new AVLTree<PcrByDistrictDate>();
        private readonly AVLTree<PcrByWorkplaceDate> _idxByWorkplaceDate = new AVLTree<PcrByWorkplaceDate>();


        public MyDatabaseModel() 
        {
        
        }

        public void InsertPcr(PCRTest t)
        {
            if (t is null) throw new ArgumentNullException(nameof(t));

            _idxByCode.Add(PcrByCode.Of(t));
            _idxByPerson.Add(PcrByPersonDate.Of(t));
            _idxByDate.Add(PcrByDate.Of(t));
            _idxByRegionDate.Add(PcrByRegionDate.Of(t));
            _idxByDistrictDate.Add(PcrByDistrictDate.Of(t));
            _idxByWorkplaceDate.Add(PcrByWorkplaceDate.Of(t));
        }
    }
}

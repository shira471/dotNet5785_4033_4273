using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DO;

namespace DalApi
{
    public interface Icrud<T> where T : class
    {
        void Create(T item);
        T? Read(int id); //Reads entity object by its ID 
        //List<T> ReadAll(); //stage 1 only, Reads all entity objects
        IEnumerable<T> ReadAll(Func<T, bool>? filter = null); // stage 2
        T? Read(Func<T, bool> filter); // stage 2
        void Update(T item); //Updates entity object
        void Delete(int id); //Deletes an object by its Id
        void DeleteAll(); //Delete all entity objects
    }
}


using System.Collections.Generic;

namespace NmarketTestTask.Models
{
    internal interface IParser
    {
        IList<House> GetHouses(string path);
    }
}
using System.Collections.Generic;

namespace Spacemaker.Data
{
    public interface IDataAccess
    {
        public List<Solution> GetSolutions();
    }
}

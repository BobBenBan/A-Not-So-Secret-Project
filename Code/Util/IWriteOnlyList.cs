using System.Collections.Generic;

namespace MusicMachine.Util
{
public interface IWriteOnlyList<in T>
{
    void Insert(int index, T item);
}
}
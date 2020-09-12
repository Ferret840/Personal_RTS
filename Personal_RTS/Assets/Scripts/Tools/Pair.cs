
namespace Tools
{

    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T _first, U _second)
        {
            this.First = _first;
            this.Second = _second;
        }

        public T First
        {
            get; set;
        }
        public U Second
        {
            get; set;
        }
    };

}
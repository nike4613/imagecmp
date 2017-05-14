namespace Utilites
{
    public class Pair<T>
    {
        public T First { get; set; }
        public T Last { get; set; }

        public Pair(T a, T b)
        {
            First = a;
            Last = b;
        }
    }
}
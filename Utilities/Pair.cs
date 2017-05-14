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

        public Pair(T[] ar) : this(ar[0],ar[1])
        {

        }

        public override string ToString()
        {
            return "Pair({0},{1})".SFormat(First,Last);
        }
    }
}
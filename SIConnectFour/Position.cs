namespace SIConnectFour
{
    public class Position
    {
        public int Row;
        public int Column;

        public Position() : this(0,0)
        {
        }

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
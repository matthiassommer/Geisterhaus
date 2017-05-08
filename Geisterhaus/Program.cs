namespace Geisterhaus
{
    static class Program
    {
        static void Main(string[] args)
        {
            using (Geisterhaus game = new Geisterhaus())
            {
                game.Run();
            }
        }
    }
}


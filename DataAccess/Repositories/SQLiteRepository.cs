namespace DataAccess.Repositories
{
    public abstract class SQLiteRepository
    {
        protected SQLiteDAO DAO { get; }

        public SQLiteRepository(SQLiteDAO dao)
        {
            DAO = dao;
        }
    }
}

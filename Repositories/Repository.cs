using ElAhorcadito.Models.Entities;

namespace ElAhorcadito.Repositories
{
    public class Repository<T>: IRepository<T> where T : class
    {
        public AhorcaditoContext Context { get; }

        public Repository(AhorcaditoContext context)
        {
            Context = context;
        }

        public IEnumerable<T> GetAll()
        {
            return Context.Set<T>();
        }

        public T? Get(object id)
        {
            return Context.Find<T>(id);
        }

        public void Insert(T entity)
        {
            Context.Add(entity);
            Context.SaveChanges();
        }

        public void Update(T entity)
        {
            Context.Update(entity);
            Context.SaveChanges();
        }

        public void Delete(object id)
        {
            var entity = Context.Find<T>(id);
            if (entity != null)
            {
                Context.Remove(entity);
                Context.SaveChanges();
            }
        }

    }
}

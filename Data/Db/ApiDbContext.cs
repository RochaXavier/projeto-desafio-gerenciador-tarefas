using Data.Entidade;
using Microsoft.EntityFrameworkCore;


namespace Data.Db
{
    public class ApiDbContext : DbContext
    {
        public DbSet<Tarefa> Tarefas { get; set; }

        public ApiDbContext(DbContextOptions options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tarefa>(_entity =>
            {
                _entity.HasKey(d => d.Id);
                _entity.Property(d => d.Status).IsRequired();
                _entity.Property(d => d.Parametros).IsRequired();
                _entity.Property(d => d.TipoTarefa).IsRequired();
                _entity.Property(d => d.DataGravacao).IsRequired();
                _entity.Property(d => d.DataUltimaAlteracao);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

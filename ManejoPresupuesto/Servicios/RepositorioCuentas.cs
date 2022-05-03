using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }

    public class RepositorioCuentas: IRepositorioCuentas
    {
        private readonly string connectionString;

        public RepositorioCuentas(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"insert into 
                                                         Cuentas(Nombre, TipoCuentaId, Balance, Descripcion)
                                                         values(@Nombre, @TipoCuentaId, @Balance, @Descripcion)
                                                         select SCOPE_IDENTITY();",
                                                         cuenta);

            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>(@"select Cuentas.Id, Cuentas.Nombre, Balance, tc.Nombre as TipoCuenta
                                                         from Cuentas
                                                         inner join TiposCuentas tc
                                                         on
                                                         tc.Id = Cuentas.TipoCuentaId
                                                         where tc.UsuarioId = @UsuarioId
                                                         Order by tc.Orden", new { usuarioId });
            
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(@"select Cuentas.Id, Cuentas.Nombre, Balance, Descripcion, tc.Id
                                                                       from Cuentas
                                                                       inner join TiposCuentas tc
                                                                       on
                                                                       tc.Id = Cuentas.TipoCuentaId
                                                                       where tc.UsuarioId = @UsuarioId
                                                                       and Cuentas.Id = @Id",
                                                                       new {id, usuarioId});
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"update Cuentas
                                            set Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId
                                            where Id = @Id;",
                                            cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("delete cuentas where Id = @Id", new { id });
        }
    }
}

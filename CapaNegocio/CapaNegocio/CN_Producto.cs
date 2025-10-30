using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CapaNegocio
{
    public class CN_Producto
    {
        private CD_Producto cdProducto = new CD_Producto();

        public List<Producto> Listar()
        {
            return cdProducto.Listar();
        }

        // En CN_Producto.cs, modifica el método Registrar y Editar para validar
        public bool Registrar(Producto producto, out string mensaje)
        {
            mensaje = string.Empty;

            // Validaciones básicas
            if (producto.IdTipo <= 0)
            {
                mensaje = "Debe seleccionar un tipo de producto";
                return false;
            }

            if (producto.IdSabor <= 0)
            {
                mensaje = "Debe seleccionar un sabor de producto";
                return false;
            }

            if (producto.PrecioVenta <= 0)
            {
                mensaje = "El precio de venta debe ser mayor a cero";
                return false;
            }

            if (producto.StockActual < 0)
            {
                mensaje = "El stock actual no puede ser negativo";
                return false;
            }

            if (producto.StockMinimo < 0)
            {
                mensaje = "El stock mínimo no puede ser negativo";
                return false;
            }

            // El costo de producción puede ser 0 inicialmente, se calculará después
            if (producto.CostoProduccion < 0)
            {
                mensaje = "El costo de producción no puede ser negativo";
                return false;
            }

            int idGenerado = cdProducto.Registrar(producto, out mensaje);
            return idGenerado > 0;
        }

        public bool Editar(Producto producto, out string mensaje)
        {
            mensaje = string.Empty;

            // Validaciones básicas
            if (producto.IdTipo <= 0)
            {
                mensaje = "Debe seleccionar un tipo de producto";
                return false;
            }

            if (producto.IdSabor <= 0)
            {
                mensaje = "Debe seleccionar un sabor de producto";
                return false;
            }

            if (producto.PrecioVenta <= 0)
            {
                mensaje = "El precio de venta debe ser mayor a cero";
                return false;
            }

            if (producto.StockActual < 0)
            {
                mensaje = "El stock actual no puede ser negativo";
                return false;
            }

            if (producto.StockMinimo < 0)
            {
                mensaje = "El stock mínimo no puede ser negativo";
                return false;
            }

            // El costo de producción puede ser 0 inicialmente
            if (producto.CostoProduccion < 0)
            {
                mensaje = "El costo de producción no puede ser negativo";
                return false;
            }

            return cdProducto.Editar(producto, out mensaje);
        }

        public bool Eliminar(int idProducto, out string mensaje)
        {
            return cdProducto.Eliminar(idProducto, out mensaje);
        }

        // MÉTODOS CORREGIDOS
        public List<string> ListarTiposProducto()
        {
            return cdProducto.ListarTiposProducto();
        }

        // NUEVO MÉTODO: Listar sabores
        public List<string> ListarSaboresProducto()
        {
            return cdProducto.ListarSaboresProducto();
        }

        // NUEVO MÉTODO: Obtener ID de Tipo por nombre
        public int ObtenerIdTipoPorNombre(string nombreTipo)
        {
            return cdProducto.ObtenerIdTipoPorNombre(nombreTipo);
        }

        // NUEVO MÉTODO: Obtener ID de Sabor por nombre
        public int ObtenerIdSaborPorNombre(string nombreSabor)
        {
            return cdProducto.ObtenerIdSaborPorNombre(nombreSabor);
        }

        // MÉTODOS NUEVOS (sin cambios)
        public List<Producto> ObtenerProductosParaVerificar()
        {
            return cdProducto.ObtenerProductosParaVerificar();
        }

        // En CN_Producto.cs, modifica el método ActualizarCostoProducto
        public bool ActualizarCostoProducto(int idProducto)
        {
            try
            {
                // Solo obtenemos los costos fijos unitarios (que ya incluyen la división entre 2500)
                decimal costosFijosUnitarios = cdProducto.CalcularCostosFijosUnitarios();
                decimal nuevoCosto = costosFijosUnitarios;

                Producto producto = ObtenerProductoPorId(idProducto);
                if (producto != null)
                {
                    decimal nuevoPrecio = nuevoCosto * (1 + (producto.MargenGanancia / 100));
                    return cdProducto.ActualizarCostoYPrecioProducto(idProducto, nuevoCosto, nuevoPrecio);
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar costo del producto: " + ex.Message);
            }
        }

        public bool ActualizarStockProducto(int idProducto, int cantidad)
        {
            return cdProducto.ActualizarStockProducto(idProducto, cantidad);
        }

        public Producto ObtenerProductoPorId(int idProducto)
        {
            return cdProducto.ObtenerProductoPorId(idProducto);
        }

        public void VerificarAlertasStock(int idProducto)
        {
            CN_Alerta cnAlerta = new CN_Alerta();
            cnAlerta.VerificarAlertasProducto(idProducto);
        }

        public bool CalcularCostoProducto(int idProducto, out decimal costo, out decimal precio, out string mensaje)
        {
            try
            {
                costo = cdProducto.CalcularCostoMateriasPrimas(idProducto);
                decimal costosFijos = cdProducto.CalcularCostosFijosUnitarios();
                costo += costosFijos;

                Producto producto = ObtenerProductoPorId(idProducto);
                if (producto != null)
                {
                    precio = costo * (1 + (producto.MargenGanancia / 100));
                    mensaje = "Costo calculado correctamente";
                    return true;
                }
                else
                {
                    precio = 0;
                    mensaje = "No se encontró el producto";
                    return false;
                }
            }
            catch (Exception ex)
            {
                costo = 0;
                precio = 0;
                mensaje = "Error al calcular costo: " + ex.Message;
                return false;
            }
        }

        public void RecalcularTodosLosProductos()
        {
            try
            {
                var productos = Listar();
                foreach (var producto in productos)
                {
                    ActualizarCostoProducto(producto.IdProducto);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recalcular productos: " + ex.Message);
            }
        }

        public decimal CalcularCostosFijosUnitarios()
        {
            return cdProducto.CalcularCostosFijosUnitarios();
        }

        //boton cambiar de estado 
        public bool CambiarVisible(int idProducto, bool nuevoVisible, out string mensaje)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(Conexion.cadena))
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Producto SET Visible = @Visible WHERE IdProducto = @IdProducto";

                    // Parámetro tipado (recomendado)
                    var pVisible = cmd.Parameters.Add("@Visible", SqlDbType.Bit);
                    pVisible.Value = nuevoVisible;

                    cmd.Parameters.Add("@IdProducto", SqlDbType.Int).Value = idProducto;

                    cn.Open();
                    int filas = cmd.ExecuteNonQuery();

                    mensaje = filas > 0 ? "La visibilidad se actualizó correctamente." : "No se encontró el producto o no se actualizó.";
                    return filas > 0;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error al cambiar la visibilidad: " + ex.Message;
                return false;
            }
        }

        // NUEVO MÉTODO: Buscar productos por tipo y sabor
        public List<Producto> BuscarProductos(int? idTipo = null, int? idSabor = null)
        {
            // Este método podría implementarse en CD_Producto si es necesario
            // Por ahora, filtramos desde la lista completa
            var todosProductos = Listar();
            var productosFiltrados = todosProductos;

            if (idTipo.HasValue && idTipo > 0)
            {
                productosFiltrados = productosFiltrados.FindAll(p => p.IdTipo == idTipo.Value);
            }

            if (idSabor.HasValue && idSabor > 0)
            {
                productosFiltrados = productosFiltrados.FindAll(p => p.IdSabor == idSabor.Value);
            }

            return productosFiltrados;
        }

        // NUEVO MÉTODO: Validar combinación única de Tipo y Sabor
        public bool ExisteCombinacionTipoSabor(int idTipo, int idSabor, int idProductoExcluir = 0)
        {
            var productos = Listar();
            return productos.Exists(p =>
                p.IdTipo == idTipo &&
                p.IdSabor == idSabor &&
                p.IdProducto != idProductoExcluir &&
                p.Visible);
        }

        // NUEVO MÉTODO: Obtener producto por tipo y sabor
        public Producto ObtenerProductoPorTipoYSabor(int idTipo, int idSabor)
        {
            var productos = Listar();
            return productos.Find(p => p.IdTipo == idTipo && p.IdSabor == idSabor && p.Visible);
        }

        public int ContarProductosEnBD()
        {
            return cdProducto.ContarProductosEnBD();
        }

    }
}
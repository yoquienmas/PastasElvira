using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

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
            // Validar que no se intente manipular los campos automáticos
            if (producto.CostoProduccion <= 0)
            {
                mensaje = "El costo de producción debe ser mayor a cero";
                return false;
            }

            if (producto.PrecioVenta <= producto.CostoProduccion)
            {
                mensaje = "El precio de venta debe ser mayor al costo de producción";
                return false;
            }

            int idGenerado = cdProducto.Registrar(producto, out mensaje);
            return idGenerado > 0;
        }

        public bool Editar(Producto producto, out string mensaje)
        {
            // Validar que no se intente manipular los campos automáticos
            if (producto.CostoProduccion <= 0)
            {
                mensaje = "El costo de producción debe ser mayor a cero";
                return false;
            }

            if (producto.PrecioVenta <= producto.CostoProduccion)
            {
                mensaje = "El precio de venta debe ser mayor al costo de producción";
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
                    precio = costo * (1 + (producto.MargenGanancia / 100));

                    mensaje = "Costo calculado correctamente";
                    return true;
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

    }
    }
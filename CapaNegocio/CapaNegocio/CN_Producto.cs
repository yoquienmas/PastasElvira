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

        public bool Registrar(Producto producto, out string mensaje)
        {
            int idGenerado = cdProducto.Registrar(producto, out mensaje);
            return idGenerado > 0; // True si se insertó correctamente
        }

        public bool Editar(Producto producto, out string mensaje)
            {
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

            public bool ActualizarCostoProducto(int idProducto)
            {
                try
                {
                    decimal costoMateriasPrimas = cdProducto.CalcularCostoMateriasPrimas(idProducto);
                    decimal costosFijosUnitarios = cdProducto.CalcularCostosFijosUnitarios();
                    decimal nuevoCosto = costoMateriasPrimas + costosFijosUnitarios;

                    return cdProducto.ActualizarCostoProducto(idProducto, nuevoCosto);
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
        }
    }
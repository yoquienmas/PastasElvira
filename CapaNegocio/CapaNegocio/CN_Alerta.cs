using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_Alerta
    {
        private CD_Alerta cdAlerta = new CD_Alerta();

        public List<AlertaStock> ListarAlertas()
        {
            return cdAlerta.ListarAlertas();
        }

        public int ObtenerCantidadAlertasPendientes()
        {
            return cdAlerta.ObtenerCantidadAlertasPendientes();
        }

        public void VerificarYGenerarAlertas()
        {
            try
            {
                // Verificar productos
                CN_Producto cnProducto = new CN_Producto();
                var productos = cnProducto.ObtenerProductosParaVerificar();

                foreach (var producto in productos)
                {
                    if (producto.StockActual <= producto.StockMinimo)
                    {
                        cdAlerta.GenerarAlertaProducto(producto.IdProducto, producto.Nombre,
                                                      producto.StockActual, producto.StockMinimo);
                    }
                }

                // Verificar materias primas
                CN_MateriaPrima cnMateria = new CN_MateriaPrima();
                var materiasPrimas = cnMateria.ObtenerMateriasPrimasParaVerificar();

                foreach (var materia in materiasPrimas)
                {
                    if (materia.CantidadDisponible <= materia.StockMinimo)
                    {
                        cdAlerta.GenerarAlertaMateriaPrima(materia.IdMateria, materia.Nombre,
                                                          materia.CantidadDisponible, materia.StockMinimo);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar alertas: " + ex.Message);
            }
        }

        public bool EliminarAlerta(int idAlerta)
        {
            return cdAlerta.EliminarAlerta(idAlerta);
        }

        public bool LimpiarAlertasAntiguas()
        {
            return cdAlerta.LimpiarAlertasAntiguas();
        }

        public void VerificarAlertasProducto(int idProducto)
        {
            CN_Producto cnProducto = new CN_Producto();
            var producto = cnProducto.ObtenerProductoPorId(idProducto);

            if (producto != null && producto.StockActual <= producto.StockMinimo)
            {
                cdAlerta.GenerarAlertaProducto(producto.IdProducto, producto.Nombre,
                                              producto.StockActual, producto.StockMinimo);
            }
        }
    }
}
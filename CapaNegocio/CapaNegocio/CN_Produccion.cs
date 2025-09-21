using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidad;
using System.Linq; // Added for LINQ usage if needed

    namespace CapaNegocio
    {
        public class CN_Produccion
        {
            private CD_Produccion cdProduccion = new CD_Produccion();

            public int Registrar(Produccion produccion, out string mensaje)
            {
                try
                {
                    // 1. Verificar disponibilidad de materias primas
                    string errorMaterias = ValidarDisponibilidadMateriasPrimas(produccion.IdProducto, produccion.CantidadProducida);
                    if (!string.IsNullOrEmpty(errorMaterias))
                    {
                        mensaje = errorMaterias;
                        return 0;
                    }

                    // 2. Registrar la producción (CORREGIDO: llamar al método de capa de datos)
                    int idProduccion = cdProduccion.Registrar(produccion, out mensaje);
                    if (idProduccion == 0) return 0;

                    // 3. Consumir las materias primas
                    bool consumoExitoso = ConsumirMateriasPrimas(produccion.IdProducto, produccion.CantidadProducida, idProduccion);
                    if (!consumoExitoso)
                    {
                        mensaje = "Error al consumir materias primas";
                        return 0;
                    }

                    // 4. Actualizar stock del producto
                    CN_Producto cnProducto = new CN_Producto();
                    bool stockActualizado = cnProducto.ActualizarStockProducto(produccion.IdProducto, produccion.CantidadProducida);
                    if (!stockActualizado)
                    {
                        mensaje = "Error al actualizar stock del producto";
                        return 0;
                    }

                    // 5. Verificar y generar alertas si es necesario
                    cnProducto.VerificarAlertasStock(produccion.IdProducto);

                    mensaje = "Producción registrada exitosamente";
                    return idProduccion;
                }
                catch (Exception ex)
                {
                    mensaje = $"Error: {ex.Message}";
                    return 0;
                }
            }

            public string ValidarDisponibilidadMateriasPrimas(int idProducto, int cantidadProducida)
            {
                return cdProduccion.ValidarDisponibilidadMateriasPrimas(idProducto, cantidadProducida);
            }

            private bool ConsumirMateriasPrimas(int idProducto, int cantidadProducida, int idProduccion)
            {
                try
                {
                    // Obtener la receta del producto
                    var receta = cdProduccion.ObtenerRecetaProducto(idProducto);

                    foreach (var item in receta)
                    {
                        decimal cantidadNecesaria = (decimal)item.CantidadNecesaria * cantidadProducida;

                        // Actualizar stock de materia prima
                        CN_MateriaPrima cnMateria = new CN_MateriaPrima();
                        bool stockActualizado = cnMateria.ActualizarStock(item.IdMateria, (float)-cantidadNecesaria);

                        if (!stockActualizado)
                        {
                            return false;
                        }

                        // Registrar en detalle de producción
                        bool detalleRegistrado = cdProduccion.RegistrarDetalleProduccion(idProduccion, item.IdMateria, cantidadNecesaria);

                        if (!detalleRegistrado)
                        {
                            return false;
                        }
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            // MÉTODOS CORREGIDOS - Eliminada la recursión infinita
            public List<Produccion> Listar()
            {
                return cdProduccion.Listar(); // CORREGIDO: llamar al método de capa de datos
            }

            public List<DetalleReceta> ListarRecetaPorProducto(int idProducto)
            {
                return cdProduccion.ObtenerRecetaProducto(idProducto);
            }

            public bool AgregarMateriaPrimaAReceta(int idProducto, int idMateria, float cantidad)
            {
                return cdProduccion.AgregarMateriaPrimaAReceta(idProducto, idMateria, cantidad);
            }

            public bool EliminarMateriaPrimaDeReceta(int idProductoMateriaPrima)
            {
                return cdProduccion.EliminarMateriaPrimaDeReceta(idProductoMateriaPrima);
            }

            // Este método parece duplicado, puedes eliminarlo si no es necesario
            public string ValidarDisponibilidadMateriasPrimas(object idProducto, int cantidad)
            {
                throw new NotImplementedException();
            }
        }
    }
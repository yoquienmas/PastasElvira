using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_Reporte
    {
        private CD_Reporte cdReporte = new CD_Reporte();

        public List<ReporteVenta> ObtenerVentasPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            return cdReporte.ObtenerVentasPorFecha(fechaInicio, fechaFin);
        }

        public decimal ObtenerTotalVentasPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            return cdReporte.ObtenerTotalVentasPeriodo(fechaInicio, fechaFin);
        }

        public List<ReporteStock> ObtenerProductosStockBajo()
        {
            return cdReporte.ObtenerProductosStockBajo();
        }

        public int ObtenerCantidadVentasPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            return cdReporte.ObtenerCantidadVentasPeriodo(fechaInicio, fechaFin);
        }

        public List<ReporteConsumo> ObtenerConsumoMateriaPrima(DateTime fechaInicio, DateTime fechaFin)
        {
            return cdReporte.ObtenerConsumoMateriaPrima(fechaInicio, fechaFin);
        }

        // MÉTODOS NUEVOS QUE FALTAN:
        public List<ReporteStock> ObtenerTodosProductosConStock()
        {
            return cdReporte.ObtenerTodosProductosConStock();
        }

        public List<ReporteVenta> ObtenerVentasPorCliente(int idCliente)
        {
            return cdReporte.ObtenerVentasPorCliente(idCliente);
        }

        public List<ConsumoMateriaPrima> ObtenerConsumoMateriaPrimaPorVenta(int idVenta)
        {
            return cdReporte.ObtenerConsumoMateriaPrimaPorVenta(idVenta);
        }
    }
}